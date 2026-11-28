using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using MySqlConnector;
using ShopGiay.Control;
using ShopGiay.Entity;
using ShopGiay.Infrastructure;

var command = args.FirstOrDefault(arg => arg is "--verify-mail" or "--db-setup" or "--db-seed" or "--db-migrate");
var builder = WebApplication.CreateBuilder(args.Where(arg => arg != command).ToArray());
var config = builder.Configuration;
var connection = new MySqlConnectionStringBuilder {
    Server = config["DB_HOST"] ?? "127.0.0.1",
    Port = config.GetValue<uint?>("DB_PORT") ?? 3306,
    UserID = config["DB_USER"] ?? "root",
    Password = config["DB_PASSWORD"] ?? "",
    Database = config["DB_NAME"] ?? "sole_studio_nhom19",
    DateTimeKind = MySqlDateTimeKind.Utc,
    MaximumPoolSize = 10,
    ConnectionTimeout = 5,
    TreatTinyAsBoolean = false
};
if (command is "--db-setup" or "--db-seed" or "--db-migrate") {
    try {
        await DatabaseMaintenance.Run(command,connection);
    } catch (Exception e) when (e is MySqlException or InvalidOperationException) {
        Console.Error.WriteLine(e.Message);
        Environment.ExitCode = 1;
    }
    return;
}
builder.WebHost.ConfigureKestrel(options => {
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 1024 * 1024;
});
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 5 * 1024 * 1024 + 64 * 1024);
builder.Services.AddSingleton(_ => new MySqlDataSourceBuilder(connection.ConnectionString)
    .UseConnectionOpenedCallback(async (context,token) => {
        await using var utc = new MySqlCommand("SET time_zone = '+00:00'",context.Connection);
        await utc.ExecuteNonQueryAsync(token);
    }).Build());
builder.Services.AddSingleton<Sql>();
builder.Services.AddSingleton<StoreSettings>();
builder.Services.AddSingleton<OtpMail>();
builder.Services.AddSingleton<ImageStore>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CatalogControl>();
builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<SessionControl>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<CartControl>();
builder.Services.AddScoped<CheckoutControl>();
builder.Services.AddScoped<AuthControl>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderControl>();
builder.Services.AddScoped<InventoryRepository>();
builder.Services.AddScoped<InventoryControl>();
builder.Services.AddScoped<CatalogRepository>();
builder.Services.AddScoped<CatalogManagement>();
builder.Services.AddScoped<ReportControl>();
builder.Services.AddScoped<PromotionRepository>();
builder.Services.AddScoped<PromotionControl>();
builder.Services.AddScoped<ReturnRepository>();
builder.Services.AddScoped<ReturnControl>();
builder.Services.AddScoped<UserControl>();
builder.Services.AddHostedService<OrderExpiryWorker>();
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options => {
    options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(new { message = "Dữ liệu yêu cầu không hợp lệ." });
});
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = 429;
    options.OnRejected = async (context,token) =>
        await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Quá nhiều yêu cầu xác thực. Vui lòng thử lại sau." },token);
    options.AddPolicy("auth",context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(15), QueueLimit = 0 }));
});

var app = builder.Build();
if (command == "--verify-mail") {
    try {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await app.Services.GetRequiredService<OtpMail>().Verify(timeout.Token);
        Console.WriteLine("Kết nối và xác thực SMTP thành công.");
    } catch (Exception e) {
        Console.Error.WriteLine("Chưa kết nối hoặc xác thực được SMTP (" + (e is ApiException api ? api.Code : e.GetType().Name) + ").");
        Environment.ExitCode = 1;
    }
    await app.DisposeAsync();
    return;
}

var proxyHops = config.GetValue<int?>("TRUST_PROXY_HOPS") ?? 0;
if (proxyHops > 0) {
    var forwarded = new ForwardedHeadersOptions {
        ForwardLimit = proxyHops,
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    if (config["TRUST_PROXY_IP"] is { Length: > 0 } address) forwarded.KnownProxies.Add(IPAddress.Parse(address));
    app.UseForwardedHeaders(forwarded);
}

app.Use(async (context,next) => {
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

var frontendDist = config["FRONTEND_DIST"];
if (frontendDist is not null) {
    var assets = new PhysicalFileProvider(Path.GetFullPath(frontendDist));
    app.UseStaticFiles(new StaticFileOptions { FileProvider = assets });
    app.MapFallbackToFile("index.html",new StaticFileOptions { FileProvider = assets });
}

app.UseRouting();
app.UseRateLimiter();
app.Use(async (context,next) => {
    if (!context.Request.Path.StartsWithSegments("/api")) {
        await next();
        return;
    }
    try {
        context.Response.Headers.CacheControl = "no-store";
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method)) {
            var origin = context.Request.Headers.Origin.ToString();
            if (context.Request.Headers["X-Sole-Request"] != "1" ||
                (origin.Length > 0 && (!Uri.TryCreate(origin,UriKind.Absolute,out var uri) ||
                !string.Equals(uri.Authority,context.Request.Host.Value,StringComparison.OrdinalIgnoreCase))))
                throw new ApiException("Yêu cầu không hợp lệ.",403);
        }
        if (context.Request.Path != "/api/health") {
            var session = context.RequestServices.GetRequiredService<SessionControl>();
            await session.Identify(context);
            var expected = context.Request.Headers["X-Sole-Account"].ToString();
            var path = (context.Request.Path.Value ?? "").TrimEnd('/').ToLowerInvariant();
            var skipAccountCheck = path is "/api/me" or "/api/auth/login" or "/api/auth/register" or
                "/api/auth/forgot" or "/api/auth/verify-otp" or "/api/auth/reset";
            if (!skipAccountCheck && expected.Length > 0 && session.User != null &&
                expected != session.User.Id.ToString(CultureInfo.InvariantCulture))
                throw new ApiException("Tài khoản trong trình duyệt đã thay đổi. Vui lòng kiểm tra lại giỏ hàng.",409,"SESSION_CHANGED");
        }
        await next();
    } catch (Exception e) when (!context.Response.HasStarted && !context.RequestAborted.IsCancellationRequested) {
        var (status,message,code) = e switch {
            ApiException error => (error.Status,error.Message,error.Code),
            DomainRuleException error => (400,error.Message,null),
            BadHttpRequestException error => (error.StatusCode,"Dữ liệu yêu cầu không hợp lệ.",null),
            JsonException => (400,"Dữ liệu yêu cầu không hợp lệ.",null),
            MySqlException { Number: 1062 or 1451 } => (409,"Dữ liệu đã tồn tại hoặc đang được tham chiếu.",null),
            MySqlException { Number: 1452 or 1406 or 1264 or 3819 } => (400,"Dữ liệu không hợp lệ.",null),
            _ => (500,"Không thể hoàn thành thao tác.",null)
        };
        if (status >= 500) app.Logger.LogWarning("API {Method} {Path}: {ErrorType}",context.Request.Method,context.Request.Path,e.GetType().Name);
        if (!HttpMethods.IsGet(context.Request.Method) && status != 429) {
            try {
                var user = context.RequestServices.GetRequiredService<SessionControl>().User;
                await context.RequestServices.GetRequiredService<Sql>().Run(db => db.Execute(
                    "INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,NoiDung,KetQua) VALUES(?,'YEU_CAU_THAT_BAI','API',?,'THAT_BAI')",
                    user?.Id,$"{context.Request.Method} {ApiInput.Clean(context.Request.Path,800)} · HTTP {status}"),context.RequestAborted);
            } catch { /* Preserve the original HTTP result if audit storage is unavailable. */ }
        }
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(code is null ? (object)new { message } : new { message,code });
    }
});
app.MapControllers();
app.MapFallback("/api/{**path}",() => Results.NotFound(new { message = "Không tìm thấy API." }));
app.Run();
