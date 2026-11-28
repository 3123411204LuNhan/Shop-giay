using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using ShopGiay.Entity;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;

namespace ShopGiay.Control;

public sealed class AuthControl(Sql sql,SessionControl session,OtpMail mail,IMemoryCache cooldown,AccountRepository accounts)
{
    private const string Invalid = "Mã xác minh không hợp lệ, đã hết hạn hoặc đã được sử dụng.";
    private const string Wait = "Vui lòng chờ 60 giây trước khi yêu cầu mã mới.";
    private static readonly object CooldownLock = new();
    // Ràng buộc bản băm OTP với từng challenge để mã không dùng được cho yêu cầu khác.
    private static byte[] OtpHash(string key,string otp) => Password.TokenHash(key + ":" + otp);

    // Đọc tài khoản, kiểm tra mật khẩu, trạng thái và vai trò trước khi tạo phiên theo khu vực.
    public async Task<object> Login(HttpContext context,JsonElement body)
    {
        var login = body.Text("login");
        var password = body.String("password") ?? "";
        var token = context.RequestAborted;
        return await sql.Run<object>(async db => {
            var account = await accounts.TimDangNhap(db,login);
            if (account is null || !Password.Verify(password,account.MatKhauHash)) {
                if (account is not null)
                    await db.Execute("UPDATE TaiKhoan SET SoLanDangNhapSai=SoLanDangNhapSai+1,KhoaDenLuc=IF(SoLanDangNhapSai>=5,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 15 MINUTE),KhoaDenLuc) WHERE TaiKhoanID=?",account.TaiKhoanID);
                throw new ApiException("Email hoặc mật khẩu không đúng.",401);
            }
            if (!account.CoTheDangNhap(DateTime.UtcNow))
                throw new ApiException("Tài khoản đang bị khóa.",403);
            var id = account.TaiKhoanID;
            var roles = await db.Rows("SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=?",id);
            if (!VaiTro.KiemTraQuyen(session.Area,roles.Select(r => (string)r.role)))
                throw new ApiException("Tài khoản không có quyền đăng nhập khu vực này.",403);
            await CreateSession(db,context,id);
            await db.Execute("UPDATE TaiKhoan SET LanDangNhapCuoi=UTC_TIMESTAMP(),SoLanDangNhapSai=0 WHERE TaiKhoanID=?",id);
            return new { ok = true, area = session.Area };
        },token);
    }

    // Kiểm tra dữ liệu, băm mật khẩu, lưu tài khoản với vai trò khách rồi tạo phiên.
    public async Task<object> Register(HttpContext context,JsonElement body)
    {
        if (session.Area != "customer") throw new ApiException("Vui lòng đăng ký tại khu vực mua sắm.",403);
        var name = body.Required("name","họ tên",150);
        var email = body.Required("email","email").ToLowerInvariant();
        var password = Password.Validate(body.String("password"));
        if (!Password.ValidEmail(email) || email.Length > 100) throw new ApiException("Email không hợp lệ.");
        var hash = Password.Hash(password);
        var id = await sql.Transaction(async db => {
            var id = await accounts.Them(db,TaiKhoan.DangKy(email,hash,name,body.Text("phone",20).NullIfEmpty()));
            await db.Execute("INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID) SELECT ?,VaiTroID FROM VaiTro WHERE MaVaiTro='CUSTOMER'",id);
            return id;
        },context.RequestAborted);
        await sql.Run(async db => { await CreateSession(db,context,id); return true; },context.RequestAborted);
        return new { ok = true, area = session.Area };
    }

    // Lưu bản băm của phiên, rồi gửi token gốc trong cookie của đúng khu vực.
    private async Task CreateSession(SqlSession db,HttpContext context,long id)
    {
        var value = Password.Token();
        await accounts.ThemPhien(db,new PhienDangNhap {
            TaiKhoanID = id, TokenHash = Password.TokenHash(value), HetHanLuc = DateTime.UtcNow.AddDays(7)
        });
        var options = SessionControl.CookieOptions(context);
        options.MaxAge = TimeSpan.FromDays(7);
        context.Response.Cookies.Append(session.Cookie,value,options);
    }

    // Thu hồi phiên còn lưu qua Entity rồi xóa cookie của khu vực hiện tại.
    public async Task<object> Logout(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(session.Cookie,out var value) && Password.ValidToken(value))
            await sql.Transaction(async db => {
                var phien = await accounts.KhoaPhien(db,Password.TokenHash(value));
                if (phien is null) return Ok;
                phien.ThuHoi(DateTime.UtcNow);
                await accounts.LuuPhien(db,phien);
                return Ok;
            },context.RequestAborted);
        context.Response.Cookies.Delete(session.Cookie,SessionControl.CookieOptions(context));
        return Ok;
    }

    // Khóa tài khoản của người dùng và chỉ cho cập nhật thông tin hồ sơ cho phép.
    public Task<object> Profile(long user,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var account = await accounts.KhoaHoatDong(db,user) ?? throw new ApiException("Vui lòng đăng nhập để tiếp tục.",401);
            account.CapNhatHoSo(body.Required("name","họ tên",150),body.Text("phone",20).NullIfEmpty(),body.Text("address",500).NullIfEmpty());
            await accounts.LuuHoSo(db,account);
            return Ok;
        },token);

    // Khóa tài khoản trước phiên, xác minh phiên còn hiệu lực rồi đổi mật khẩu và thu hồi các phiên cũ.
    public async Task<object> StaffPassword(HttpContext context,long id,JsonElement body)
    {
        var hash = Password.Hash(Password.Validate(body.String("password")));
        await sql.Transaction(async db => {
            var account = await accounts.KhoaHoatDong(db,id);
            var sessionRow = await accounts.KhoaPhien(db,Password.TokenHash(context.Request.Cookies[session.Cookie]!));
            if (account is null || sessionRow is null || sessionRow.TaiKhoanID != id || !sessionRow.ConHieuLuc(DateTime.UtcNow))
                throw new ApiException("Vui lòng đăng nhập để tiếp tục.",401);
            await SavePassword(db,account,hash);
            await db.Audit(id,"DOI_MAT_KHAU","TaiKhoan",id,"Nhân sự tự đổi mật khẩu");
            return Ok;
        },context.RequestAborted);
        context.Response.Cookies.Delete(session.Cookie,SessionControl.CookieOptions(context));
        return Ok;
    }

    // Giới hạn gửi lại, thay mã cũ bằng challenge mới rồi gửi OTP sau khi commit.
    public async Task<object> Forgot(JsonElement body,CancellationToken token)
    {
        var email = body.Text("email",254).ToLowerInvariant();
        if (!Password.ValidEmail(email)) throw new ApiException("Vui lòng nhập địa chỉ email hợp lệ.");
        var key = Convert.ToHexString(Password.TokenHash(email));
        lock (CooldownLock) {
            if (cooldown.TryGetValue(key,out _)) throw new ApiException(Wait,429);
            cooldown.Set(key,true,TimeSpan.FromSeconds(60));
        }
        var challenge = Password.Token();
        var code = RandomNumberGenerator.GetInt32(1000000).ToString("D6");
        var hash = Password.TokenHash(challenge);
        try {
            var hasAccount = await sql.Transaction(async db => {
                var id = await db.Value<long?>("SELECT TaiKhoanID FROM TaiKhoan WHERE Email=? AND TrangThai='HOAT_DONG' AND NgayXoa IS NULL FOR UPDATE",email);
                if (id is null) return false;
                if (await db.One("SELECT YeuCauID FROM DatLaiMatKhau WHERE TaiKhoanID=? AND MaXacMinhHash IS NOT NULL AND NgayTao>DATE_SUB(UTC_TIMESTAMP(),INTERVAL 60 SECOND) LIMIT 1",id) is not null)
                    throw new ApiException(Wait,429);
                await db.Execute("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL",id);
                await accounts.ThemMa(db,new DatLaiMatKhau { TaiKhoanID = id.Value, TokenHash = hash, MaXacMinhHash = OtpHash(challenge,code) });
                return true;
            },token);
            try { await mail.Deliver(email,code,challenge,hasAccount,token); }
            catch {
                if (hasAccount) await sql.Run(db => db.Execute("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TokenHash=?",hash));
                throw;
            }
        } catch (Exception e) {
            if (e is not ApiException { Status: 429 }) cooldown.Remove(key);
            throw;
        }
        return new { challenge, expiresIn = 600, resendAfter = 60,
            message = "Yêu cầu đã được xử lý. Kiểm tra email hoặc hộp thư local trên máy chạy server." };
    }

    // Khóa tài khoản trước mã để các luồng gửi, xác minh và đặt lại dùng cùng thứ tự khóa.
    private async Task<(TaiKhoan Account,DatLaiMatKhau Reset)> LockReset(SqlSession db,byte[] hash)
    {
        var id = await db.Value<long?>("SELECT TaiKhoanID FROM DatLaiMatKhau WHERE TokenHash=?",hash);
        if (id is null) throw new ApiException(Invalid);
        var account = await accounts.KhoaHoatDong(db,id.Value) ?? throw new ApiException(Invalid);
        var reset = await accounts.KhoaMa(db,hash) ?? throw new ApiException(Invalid);
        return (account,reset);
    }

    // Kiểm tra OTP bằng so sánh cố định thời gian, lưu lần thử hoặc cấp token đã xác minh.
    public async Task<object> Verify(JsonElement body,CancellationToken token)
    {
        var challenge = body.Text("challenge");
        var code = body.Text("code");
        if (!Password.ValidToken(challenge) || !Regex.IsMatch(code,@"\A[0-9]{6}\z")) throw new ApiException("Nhập đủ mã xác minh 6 chữ số.");
        var resetToken = Password.Token();
        var error = await sql.Transaction<string?>(async db => {
            var (_,row) = await LockReset(db,Password.TokenHash(challenge));
            var expected = row.MaXacMinhHash;
            if (expected is null || !row.ConHieuLuc(DateTime.UtcNow)) throw new ApiException(Invalid);
            if (!CryptographicOperations.FixedTimeEquals(OtpHash(challenge,code),expected)) {
                row.GhiLanSai(DateTime.UtcNow);
                await accounts.LuuMa(db,row);
                return row.SoLanThu >= 5 ? "Bạn đã nhập sai 5 lần. Vui lòng yêu cầu mã mới." : "Mã xác minh chưa đúng.";
            }
            row.DanhDauDaSuDung(DateTime.UtcNow);
            await accounts.LuuMa(db,row);
            await accounts.ThemMa(db,new DatLaiMatKhau { TaiKhoanID = row.TaiKhoanID, TokenHash = Password.TokenHash(resetToken) });
            return null;
        },token);
        if (error != null) throw new ApiException(error);
        return new { resetToken, expiresIn = 600 };
    }

    // Chỉ nhận token đã xác minh, sau đó đổi mật khẩu và thu hồi các mã, phiên cũ.
    public Task<object> Reset(JsonElement body,CancellationToken token)
    {
        var key = body.Text("token");
        if (!Password.ValidToken(key)) throw new ApiException("Phiên đặt lại mật khẩu không hợp lệ.");
        var password = Password.Validate(body.String("password"));
        return sql.Transaction(async db => {
            var (account,row) = await LockReset(db,Password.TokenHash(key));
            if (row.MaXacMinhHash is not null) throw new ApiException("Bạn cần xác minh OTP trước khi đặt mật khẩu mới.");
            await SavePassword(db,account,Password.Hash(password));
            return Ok;
        },token);
    }

    // Lưu mật khẩu mới của Entity rồi vô hiệu mọi mã đặt lại và phiên của tài khoản.
    private async Task SavePassword(SqlSession db,TaiKhoan account,string hash)
    {
        var id = account.TaiKhoanID;
        account.DoiMatKhau(hash,DateTime.UtcNow);
        await accounts.LuuMatKhau(db,account);
        await db.Execute("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL",id);
        await db.Execute("UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=?",id);
    }
}
