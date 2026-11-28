using System.Threading.RateLimiting;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using MySqlConnector;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using ShopGiay.Web.Services;

// Điểm vào ứng dụng: cấu hình → Dependency Injection → Middleware → Controller.
var builder = WebApplication.CreateBuilder(args);
// Thực hiện bước builder.Configuration.AddJsonFile("appsettings.Local.json",optional:true,reloadOnChange:true).AddEnvironmentVariables trong Program; tiếp tục theo luồng của hàm.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
// Thực hiện bước builder.WebHost.ConfigureKestrel trong Program; tiếp tục theo luồng của hàm.
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 1024 * 1024);
// Thực hiện bước builder.Services.AddHttpContextAccessor trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddHttpContextAccessor();
// Thực hiện bước builder.Services.AddScoped<Database> trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddScoped<Database>();
// Thực hiện bước builder.Services.AddScoped<SessionContext> trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddScoped<SessionContext>();
// Thực hiện bước builder.Services.AddSingleton<IPasswordHasher,PasswordHasher> trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
// Thực hiện bước builder.Services.AddSingleton<StoreSettings> trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddSingleton<StoreSettings>();
// AuthController → AuthService → IAuthRepository → AuthRepository.
builder.Services.AddScoped<AuthService>();
// Khôi phục mật khẩu được AuthController gọi riêng để luồng OTP dễ theo dõi.
builder.Services.AddScoped<PasswordResetService>();
// CatalogController → CatalogService quản lý sản phẩm và phân loại.
builder.Services.AddScoped<CatalogService>();
// CheckoutController → CheckoutService quản lý giỏ và tạo đơn.
builder.Services.AddScoped<CheckoutService>();
// OrderController và OrderExpiryWorker cùng dùng OrderService.
builder.Services.AddScoped<OrderService>();
// ReturnController → ReturnService xử lý trả hàng và hoàn tiền.
builder.Services.AddScoped<ReturnService>();
// InventoryController → InventoryService xử lý phiếu kho.
builder.Services.AddScoped<InventoryService>();
// PromotionController → PromotionService xử lý ưu đãi.
builder.Services.AddScoped<PromotionService>();
// ReportController → ReportService tổng hợp dữ liệu.
builder.Services.AddScoped<ReportService>();
// UserController → UserService quản trị tài khoản.
builder.Services.AddScoped<UserService>();
// Khi Service yêu cầu IAuthRepository, DI tạo AuthRepository với Database của request.
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
// Liên kết hợp đồng truy cập sản phẩm với MySQL implementation.
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
// Liên kết hợp đồng giỏ/đặt hàng với MySQL implementation.
builder.Services.AddScoped<ICheckoutRepository, CheckoutRepository>();
// Liên kết hợp đồng đơn hàng với MySQL implementation.
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Liên kết hợp đồng trả hàng/hoàn tiền với MySQL implementation.
builder.Services.AddScoped<IReturnRepository, ReturnRepository>();
// Liên kết hợp đồng phiếu kho với MySQL implementation.
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
// Liên kết hợp đồng khuyến mãi với MySQL implementation.
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
// Liên kết hợp đồng báo cáo với MySQL implementation.
builder.Services.AddScoped<IReportRepository, ReportRepository>();
// Liên kết hợp đồng tài khoản với MySQL implementation.
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Thực hiện bước builder.Services.AddScoped<Operations> trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddScoped<Operations>();
// Thực hiện bước builder.Services.AddControllers().AddJsonOptions trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.DictionaryKeyPolicy = null);
// Thực hiện bước builder.Services.AddRateLimiter trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddRateLimiter(o =>
{
    // Gán kết quả vào o.RejectionStatusCode để bước xử lý tiếp theo dùng dữ liệu mới.
    o.RejectionStatusCode = 429;
    // Thực hiện bước o.AddPolicy trong Program; tiếp tục theo luồng của hàm.
    o.AddPolicy("auth", http => RateLimitPartition.GetFixedWindowLimiter(http.Connection.RemoteIpAddress?.ToString() ?? "local", _ => new FixedWindowRateLimiterOptions { PermitLimit = 100, Window = TimeSpan.FromMinutes(15), QueueLimit = 0 }));
    // Gán kết quả vào o.OnRejected để bước xử lý tiếp theo dùng dữ liệu mới.
    o.OnRejected = async (context, token) =>
    {
        // Gán kết quả vào context.HttpContext.Response.StatusCode để bước xử lý tiếp theo dùng dữ liệu mới.
        context.HttpContext.Response.StatusCode = 429;
        // Thực hiện bước context.HttpContext.Response.WriteAsJsonAsync trong Program; tiếp tục theo luồng của hàm.
        await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Quá nhiều yêu cầu xác thực. Vui lòng thử lại sau." }, token);
    };
});
// Thực hiện bước builder.Services.AddHostedService<OrderExpiryWorker> trong Program; tiếp tục theo luồng của hàm.
builder.Services.AddHostedService<OrderExpiryWorker>();
// Tạo biến app để dùng ở các bước tiếp theo trong phạm vi hiện tại.
var app = builder.Build();
// Lệnh khởi tạo/chuyển schema chạy riêng, không tự sửa database khi nhấn F5.
if (args.Any(value => new[] { "--db-config", "--db-check", "--db-setup", "--db-seed", "--db-migrate" }.Contains(value)))
{
    // Tạo biến scope để dùng ở các bước tiếp theo trong phạm vi hiện tại.
    using var scope = app.Services.CreateScope();
    // Chặn lỗi lệnh database để trả hướng dẫn thay vì stack trace dài.
    try
    {
        // Điều phối cấu hình/kiểm tra/khởi tạo/migrate/seed.
        await DatabaseCommands.Run(scope.ServiceProvider, args);
    }
    catch (Exception error)when (error is MySqlException or InvalidOperationException or JsonException or ArgumentException)
    {
        // Không in connection string hoặc mật khẩu trong thông báo lỗi.
        Console.Error.WriteLine(error is MySqlException mysql ? DatabaseConfiguration.Explain(mysql) : error.Message);
        // Thực hiện bước Console.Error.WriteLine trong Program; tiếp tục theo luồng của hàm.
        Console.Error.WriteLine("Cấu hình: " + Path.Combine(app.Environment.ContentRootPath, "appsettings.Local.json"));
        // Thực hiện bước Console.Error.WriteLine trong Program; tiếp tục theo luồng của hàm.
        Console.Error.WriteLine("Chạy: dotnet run --project src/ShopGiay.Web -- --db-config");
        // Script nhận mã lỗi khác 0 và không tự tiếp tục seed sau khi thất bại.
        Environment.ExitCode = 1;
    }

    // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
    return;
}

// Lỗi tập trung: thông báo tiếng Việt cho client, không đưa SQL/mật khẩu vào response.
app.Use(async (http, next) =>
{
    // Gán kết quả vào http.Response.Headers["X-Content-Type-Options"] để bước xử lý tiếp theo dùng dữ liệu mới.
    http.Response.Headers["X-Content-Type-Options"] = "nosniff";
    // Gán kết quả vào http.Response.Headers["X-Frame-Options"] để bước xử lý tiếp theo dùng dữ liệu mới.
    http.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    // Gán kết quả vào http.Response.Headers["Referrer-Policy"] để bước xử lý tiếp theo dùng dữ liệu mới.
    http.Response.Headers["Referrer-Policy"] = "no-referrer";
    // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
    try
    {
        // Thực hiện bước next trong Program; tiếp tục theo luồng của hàm.
        await next();
    }
    catch (Exception e)
    {
        // Gán kết quả vào var(status,message,code) để bước xử lý tiếp theo dùng dữ liệu mới.
        var(status, message, code) = e switch
        {
            ApiException a => (a.Status, a.Message, a.Code),
            MySqlException { Number: 1062 } => (409, "Dữ liệu đã tồn tại hoặc yêu cầu đã được xử lý.", (string? )null),
            MySqlException { Number: 1451 } => (409, "Mục này đang được tham chiếu nên chưa thể xóa.", (string? )null),
            MySqlException { Number: 1452 } => (400, "Dữ liệu liên kết không hợp lệ.", (string? )null),
            // Phân biệt thiếu database/bảng và lỗi đăng nhập với lỗi nghiệp vụ.
            MySqlException mysql => (503, DatabaseConfiguration.Explain(mysql), (string?)null),
            _ => (500, "Không thể hoàn thành thao tác. Kiểm tra kết nối MySQL hoặc dữ liệu.", (string? )null)};
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (status == 500)
            // Thực hiện bước app.Logger.LogError trong Program; tiếp tục theo luồng của hàm.
            app.Logger.LogError("API lỗi loại {Type}", e.GetType().Name);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (http.Request.Method != "GET")
        {
            // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
            try
            {
                // Tạo biến db để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var db = http.RequestServices.GetRequiredService<Database>();
                // Tạo biến user để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var user = http.RequestServices.GetRequiredService<SessionContext>().User;
                // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query("INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,NoiDung,KetQua) VALUES(?,'YEU_CAU_THAT_BAI','API',?,'THAT_BAI')", [user?["id"], http.Request.Method + " " + http.Request.Path + " · HTTP " + status]);
            }
            catch
            { /* Lỗi ghi nhật ký không che lỗi ban đầu. */
            }
        }

        // Gán kết quả vào http.Response.StatusCode để bước xử lý tiếp theo dùng dữ liệu mới.
        http.Response.StatusCode = status;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (code is null)
            // Thực hiện bước http.Response.WriteAsJsonAsync trong Program; tiếp tục theo luồng của hàm.
            await http.Response.WriteAsJsonAsync(new { message });
        else
            // Thực hiện bước http.Response.WriteAsJsonAsync trong Program; tiếp tục theo luồng của hàm.
            await http.Response.WriteAsJsonAsync(new { message, code });
    }
});
// Thực hiện bước app.UseDefaultFiles trong Program; tiếp tục theo luồng của hàm.
app.UseDefaultFiles();
// Thực hiện bước app.UseStaticFiles trong Program; tiếp tục theo luồng của hàm.
app.UseStaticFiles();
// Thực hiện bước app.UseRouting trong Program; tiếp tục theo luồng của hàm.
app.UseRouting();
// Thực hiện bước app.UseRateLimiter trong Program; tiếp tục theo luồng của hàm.
app.UseRateLimiter();
// Thực hiện bước app.UseMiddleware<ApiSecurityMiddleware> trong Program; tiếp tục theo luồng của hàm.
app.UseMiddleware<ApiSecurityMiddleware>();
// Thực hiện bước app.MapGet trong Program; tiếp tục theo luồng của hàm.
app.MapGet("/api/health", async (Database db) =>
{
    // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
    try
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("SELECT 1");
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Results.Json(new { status = "ok", database = "connected" });
    }
    catch (MySqlException error)
    {
        // Trạng thái kiểm tra phải chỉ rõ thiếu database, thiếu bảng hoặc bị từ chối đăng nhập.
        return Results.Json(new { status = "setup_required", message = DatabaseConfiguration.Explain(error) }, statusCode: 503);
    }
    catch
    {
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Results.Json(new { status = "setup_required", message = "Chưa kết nối MySQL. Kiểm tra ConnectionStrings:ShopGiay và hướng dẫn khởi tạo." }, statusCode: 503);
    }
});
// Thực hiện bước app.MapControllers trong Program; tiếp tục theo luồng của hàm.
app.MapControllers();
// Thực hiện bước app.MapFallback trong Program; tiếp tục theo luồng của hàm.
app.MapFallback("/api/{**path}", () => Results.NotFound(new { message = "Không tìm thấy API." }));
// Thực hiện bước app.MapFallbackToFile trong Program; tiếp tục theo luồng của hàm.
app.MapFallbackToFile("index.html");
// Thực hiện bước app.RunAsync trong Program; tiếp tục theo luồng của hàm.
await app.RunAsync();
