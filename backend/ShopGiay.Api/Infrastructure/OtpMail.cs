using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text.RegularExpressions;

namespace ShopGiay.Infrastructure;

public sealed class OtpMail(IConfiguration config)
{
    public string DirectoryPath => Path.GetFullPath(config["MAIL_LOCAL_DIR"] ?? ".local/mail");

    private async Task Send(string email,string subject,string body,CancellationToken token)
    {
        var port = config.GetValue<int?>("SMTP_PORT") ?? 465;
        var user = config["SMTP_USER"]?.Trim();
        var password = config["SMTP_APP_PASSWORD"] is { Length: > 0 } appPassword
            ? Regex.Replace(appPassword,@"\s","") : config["SMTP_PASSWORD"];
        if (string.IsNullOrWhiteSpace(config["SMTP_HOST"]) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            throw new ApiException("SMTP chưa được cấu hình.",503,"SMTP_NOT_CONFIGURED");
        var mode = (config.GetValue<bool?>("SMTP_SECURE") ?? port == 465)
            ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        using var smtp = new SmtpClient { Timeout = 12000 };
        await smtp.ConnectAsync(config["SMTP_HOST"]!.Trim(),port,mode,token);
        await smtp.AuthenticateAsync(user,password,token);
        if (email.Length > 0) {
            var message = new MimeMessage { Subject = subject, Body = new TextPart("plain") { Text = body } };
            message.From.Add(new MailboxAddress(config["SMTP_FROM"] ?? "SOLE STUDIO",user));
            message.To.Add(MailboxAddress.Parse(email));
            await smtp.SendAsync(message,token);
        }
        await smtp.DisconnectAsync(true,token);
    }

    public Task Verify(CancellationToken token) => Send("","","",token);

    public async Task Deliver(string email,string code,string challenge,bool accountExists,CancellationToken token)
    {
        var domain = email.Split('@')[1];
        var reason = !accountExists ? "EMAIL_NOT_REGISTERED" :
            Regex.IsMatch(domain,@"\.(test|invalid|localhost|example|local)$") ||
            domain is "localhost" or "example.com" or "example.net" or "example.org" ? "RESERVED_DOMAIN" : "";
        if (reason == "") {
            try {
                await Send(email,"SOLE STUDIO — Mã xác minh đặt lại mật khẩu",
                    $"Mã xác minh của bạn: {code}\n\nMã có hiệu lực 10 phút và chỉ dùng một lần. Không chia sẻ mã với người khác.",token);
                return;
            } catch (Exception e) when (!token.IsCancellationRequested) {
                reason = e is ApiException api ? api.Code ?? "SMTP_SEND_FAILED" : "SMTP_SEND_FAILED";
            }
        }
        if (config["MAIL_FALLBACK_LOCAL"] == "false")
            throw new ApiException("Chưa gửi được mã xác minh. Vui lòng thử lại sau.",503);
        Directory.CreateDirectory(DirectoryPath);
        var file = Path.Combine(DirectoryPath,$"reset-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{challenge[..8]}.txt");
        var origin = config["APP_ORIGIN"] ?? "http://localhost:5173";
        var text = $"SOLE STUDIO — HỘP THƯ LOCAL\nĐến: {email}\nMã xác minh: {code}\nHiệu lực: 10 phút. Chỉ dùng một lần.\nLý do lưu local: {reason}\nMở lại màn hình nhập mã: {origin}/?verify={challenge}\n";
        var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write };
        if (!OperatingSystem.IsWindows()) options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        await using var stream = new FileStream(file,options);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(text.AsMemory(),token);
    }
}
