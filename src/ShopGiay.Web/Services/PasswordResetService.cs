using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Services;
public sealed class PasswordResetService(Database db, IPasswordHasher passwords, IConfiguration config, IWebHostEnvironment environment)
{
    private const string Invalid = "Mã xác minh không hợp lệ, đã hết hạn hoặc đã được sử dụng.";
    // Quên mật khẩu → khóa tài khoản → tạo challenge và OTP có hạn → gửi email/hộp thư local.
    public async Task<object> Request(Row body)
    {
        // Tạo biến email để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var email = Input.Text(body["email"], 254).ToLowerInvariant();
        // Thực hiện bước AuthService.ValidateEmail trong PasswordResetService; tiếp tục theo luồng của hàm.
        AuthService.ValidateEmail(email);
        // Tạo biến challenge để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var challenge = SessionContext.NewToken();
        // Tạo biến code để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var code = RandomNumberGenerator.GetInt32(1000000).ToString("D6");
        // Database.Transaction bao các thao tác trong COMMIT/ROLLBACK để không ghi dữ liệu dở dang.
        var exists = await db.Transaction(async c =>
        {
            // Tạo biến user để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var user = (await db.Query("SELECT TaiKhoanID id FROM TaiKhoan WHERE Email=? AND TrangThai='HOAT_DONG' AND NgayXoa IS NULL FOR UPDATE", [email], c)).FirstOrDefault();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (user is null)
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return false;
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if ((await db.Query("SELECT YeuCauID FROM DatLaiMatKhau WHERE TaiKhoanID=? AND MaXacMinhHash IS NOT NULL AND NgayTao>DATE_SUB(UTC_TIMESTAMP(),INTERVAL 60 SECOND) LIMIT 1", [user.L("id")], c)).Count > 0)
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Vui lòng chờ 60 giây trước khi yêu cầu mã mới.", 429);
            // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL", [user.L("id")], c);
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO DatLaiMatKhau(TaiKhoanID,TokenHash,MaXacMinhHash,HetHanLuc) VALUES(?,?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 10 MINUTE))", [user.L("id"), SessionContext.Hash(challenge), SessionContext.Hash(challenge + ":" + code)], c);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return true;
        });
        // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
        try
        {
            // Thực hiện bước Deliver trong PasswordResetService; tiếp tục theo luồng của hàm.
            await Deliver(email, challenge, code, exists);
        }
        catch
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (exists)
                // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TokenHash=?", [SessionContext.Hash(challenge)]);
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chưa gửi được mã xác minh. Vui lòng thử lại sau.", 503);
        }

        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            challenge,
            expiresIn = 600,
            resendAfter = 60,
            message = "Yêu cầu đã được xử lý. Kiểm tra email hoặc hộp thư local trên máy chạy server."
        };
    }

    // Xác minh sai trả kết quả lỗi ra ngoài transaction để số lần thử vẫn được COMMIT.
    public async Task<object> Verify(Row body)
    {
        // Tạo biến challenge để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var challenge = body.S("challenge");
        // Tạo biến code để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var code = body.S("code");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!Regex.IsMatch(challenge, "^[a-f0-9]{64}$") || !Regex.IsMatch(code, "^[0-9]{6}$"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Nhập đủ mã xác minh 6 chữ số.");
        // Tạo biến result để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var result = await Locked(challenge, async (c, row) =>
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (row["MaXacMinhHash"] is not byte[] expected || row.L("SoLanThu") >= 5)
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return new Row
                {
                    ["error"] = Invalid
                };
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (!CryptographicOperations.FixedTimeEquals(SessionContext.Hash(challenge + ":" + code), expected))
            {
                // Tạo biến attempts để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var attempts = row.L("SoLanThu") + 1;
                // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query("UPDATE DatLaiMatKhau SET SoLanThu=?,DaSuDungLuc=IF(? >= 5,UTC_TIMESTAMP(),NULL) WHERE YeuCauID=?", [attempts, attempts, row.L("YeuCauID")], c);
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return new Row
                {
                    ["error"] = attempts >= 5 ? "Bạn đã nhập sai 5 lần. Vui lòng yêu cầu mã mới." : "Mã xác minh chưa đúng."
                };
            }

            // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE YeuCauID=?", [row.L("YeuCauID")], c);
            // Tạo biến token để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var token = SessionContext.NewToken();
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO DatLaiMatKhau(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 10 MINUTE))", [row.L("TaiKhoanID"), SessionContext.Hash(token)], c);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return new Row
            {
                ["resetToken"] = token,
                ["expiresIn"] = 600
            };
        });
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (result.ContainsKey("error"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException(result.S("error"));
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return result;
    }

    // Chỉ grant đã xác minh OTP mới được dùng đặt mật khẩu mới.
    public async Task<object> Reset(Row body)
    {
        // Thực hiện bước AuthService.ValidatePassword trong PasswordResetService; tiếp tục theo luồng của hàm.
        AuthService.ValidatePassword(body.S("password"));
        // Tạo biến token để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var token = body.S("token");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!Regex.IsMatch(token, "^[a-f0-9]{64}$"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Phiên đặt lại mật khẩu không hợp lệ.");
        // Tạo biến result để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var result = await Locked(token, async (c, row) =>
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (row["MaXacMinhHash"] is not null)
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return new Row
                {
                    ["error"] = "Bạn cần xác minh OTP trước khi đặt mật khẩu mới."
                };
            // Thực hiện bước SavePassword trong PasswordResetService; tiếp tục theo luồng của hàm.
            await SavePassword(c, row.L("TaiKhoanID"), body.S("password"));
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return new Row
            {
                ["ok"] = true
            };
        });
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (result.ContainsKey("error"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException(result.S("error"));
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return result;
    }

    // Thứ tự khóa giống đổi mật khẩu nhân sự để tránh dùng lại mã/phiên cũ.
    private async Task<Row> Locked(string token, Func<DbSession, Row, Task<Row>> operation)
    {
        // Tạo biến meta để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var meta = (await db.Query("SELECT TaiKhoanID id FROM DatLaiMatKhau WHERE TokenHash=?", [SessionContext.Hash(token)])).FirstOrDefault();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (meta is null)
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return new Row
            {
                ["error"] = Invalid
            };
        // Database.Transaction bao các thao tác trong COMMIT/ROLLBACK để không ghi dữ liệu dở dang.
        return await db.Transaction(async c =>
        {
            // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            var user = await db.Query("SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? AND TrangThai='HOAT_DONG' AND NgayXoa IS NULL FOR UPDATE", [meta.L("id")], c);
            // Tạo biến row để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var row = (await db.Query("SELECT * FROM DatLaiMatKhau WHERE TokenHash=? AND DaSuDungLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP() FOR UPDATE", [SessionContext.Hash(token)], c)).FirstOrDefault();
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return user.Count == 0 || row is null ? new Row
            {
                ["error"] = Invalid
            }

            : await operation(c, row);
        });
    }

    // AuthService và Reset cùng gọi: đổi hash, vô hiệu OTP và thu hồi mọi phiên.
    public async Task SavePassword(DbSession c, long id, string password)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("UPDATE TaiKhoan SET MatKhauHash=?,NgayDoiMatKhau=UTC_TIMESTAMP(),SoLanDangNhapSai=0,KhoaDenLuc=NULL WHERE TaiKhoanID=?", [passwords.Hash(password), id], c);
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL", [id], c);
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=?", [id], c);
    }

    // SMTP STARTTLS cổng 587; hộp thư local chỉ ở máy chủ, ngoài wwwroot.
    private async Task Deliver(string email, string challenge, string code, bool exists)
    {
        // Tạo biến host để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var host = config["Mail:Host"];
        // Tạo biến user để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var user = config["Mail:User"];
        // Tạo biến password để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var password = config["Mail:Password"];
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (exists && !string.IsNullOrWhiteSpace(host) && !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
        {
            // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
            try
            {
                // Tạo biến smtp để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                using var smtp = new SmtpClient(host, config.GetValue("Mail:Port", 587))
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(user, password),
                    Timeout = 12000
                };
                // Tạo biến message để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                using var message = new MailMessage(user, email, "SOLE STUDIO — Mã xác minh đặt lại mật khẩu", $"Mã xác minh: {code}\nHiệu lực 10 phút, chỉ dùng một lần.");
                // Thực hiện bước smtp.SendMailAsync trong PasswordResetService; tiếp tục theo luồng của hàm.
                await smtp.SendMailAsync(message);
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return;
            }
            catch when (config.GetValue("Mail:LocalFallback", true))
            {
            }
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!config.GetValue("Mail:LocalFallback", true))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new InvalidOperationException("SMTP không khả dụng.");
        // Tạo biến directory để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var directory = Path.Combine(environment.ContentRootPath, ".local", "mail");
        // Thực hiện bước Directory.CreateDirectory trong PasswordResetService; tiếp tục theo luồng của hàm.
        Directory.CreateDirectory(directory);
        // Thực hiện bước File.WriteAllTextAsync trong PasswordResetService; tiếp tục theo luồng của hàm.
        await File.WriteAllTextAsync(Path.Combine(directory, $"reset-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{challenge[..8]}.txt"), $"Đến: {email}\nMã xác minh: {code}\nHiệu lực: 10 phút\nChallenge: {challenge}\n{(exists ? "Dùng mã ở màn hình xác minh." : "Email chưa đăng ký; mã minh họa không có hiệu lực.")}");
    }
}
