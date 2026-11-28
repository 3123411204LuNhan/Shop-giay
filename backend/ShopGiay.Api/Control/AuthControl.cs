using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;

namespace ShopGiay.Control;

public sealed class AuthControl(Sql sql,SessionControl session,OtpMail mail,IMemoryCache cooldown)
{
    private const string Invalid = "Mã xác minh không hợp lệ, đã hết hạn hoặc đã được sử dụng.";
    private const string Wait = "Vui lòng chờ 60 giây trước khi yêu cầu mã mới.";
    private static readonly object CooldownLock = new();
    private static byte[] OtpHash(string key,string otp) => Password.TokenHash(key + ":" + otp);

    public async Task<object> Login(HttpContext context,JsonElement body)
    {
        var login = body.Text("login");
        var password = body.String("password") ?? "";
        var token = context.RequestAborted;
        return await sql.Run<object>(async db => {
            var account = await db.One("SELECT TaiKhoanID id,MatKhauHash hash,TrangThai status,KhoaDenLuc locked FROM TaiKhoan WHERE NgayXoa IS NULL AND (Email=? OR TenDangNhap=?)",login,login);
            if (account is null || !Password.Verify(password,(string)account.hash)) {
                if (account is not null)
                    await db.Execute("UPDATE TaiKhoan SET SoLanDangNhapSai=SoLanDangNhapSai+1,KhoaDenLuc=IF(SoLanDangNhapSai>=5,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 15 MINUTE),KhoaDenLuc) WHERE TaiKhoanID=?",(long)account.id);
                throw new ApiException("Email hoặc mật khẩu không đúng.",401);
            }
            if (account.status != "HOAT_DONG" || (DateTime?)account.locked > DateTime.UtcNow)
                throw new ApiException("Tài khoản đang bị khóa.",403);
            var id = (long)account.id;
            var roles = await db.Rows("SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=?",id);
            if (!SessionControl.CanUse(session.Area,roles.Select(r => (string)r.role)))
                throw new ApiException("Tài khoản không có quyền đăng nhập khu vực này.",403);
            await CreateSession(db,context,id);
            await db.Execute("UPDATE TaiKhoan SET LanDangNhapCuoi=UTC_TIMESTAMP(),SoLanDangNhapSai=0 WHERE TaiKhoanID=?",id);
            return new { ok = true, area = session.Area };
        },token);
    }

    public async Task<object> Register(HttpContext context,JsonElement body)
    {
        if (session.Area != "customer") throw new ApiException("Vui lòng đăng ký tại khu vực mua sắm.",403);
        var name = body.Required("name","họ tên",150);
        var email = body.Required("email","email").ToLowerInvariant();
        var password = Password.Validate(body.String("password"));
        if (!Password.ValidEmail(email) || email.Length > 100) throw new ApiException("Email không hợp lệ.");
        var hash = Password.Hash(password);
        var id = await sql.Transaction(async db => {
            var id = await db.Insert("INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai) VALUES(?,?,?,?,?)",
                email,email,hash,name,body.Text("phone",20).NullIfEmpty());
            await db.Execute("INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID) SELECT ?,VaiTroID FROM VaiTro WHERE MaVaiTro='CUSTOMER'",id);
            return id;
        },context.RequestAborted);
        await sql.Run(async db => { await CreateSession(db,context,id); return true; },context.RequestAborted);
        return new { ok = true, area = session.Area };
    }

    private async Task CreateSession(SqlSession db,HttpContext context,long id)
    {
        var value = Password.Token();
        await db.Execute("INSERT INTO PhienDangNhap(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 7 DAY))",id,Password.TokenHash(value));
        var options = SessionControl.CookieOptions(context);
        options.MaxAge = TimeSpan.FromDays(7);
        context.Response.Cookies.Append(session.Cookie,value,options);
    }

    public async Task<object> Logout(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue(session.Cookie,out var value) && Password.ValidToken(value))
            await sql.Run(db => db.Execute("UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TokenHash=?",Password.TokenHash(value)),context.RequestAborted);
        context.Response.Cookies.Delete(session.Cookie,SessionControl.CookieOptions(context));
        return Ok;
    }

    public async Task<object> Profile(long user,JsonElement body,CancellationToken token)
    {
        await sql.Run(db => db.Execute("UPDATE TaiKhoan SET HoTen=?,SoDienThoai=?,DiaChi=?,NgayCapNhat=UTC_TIMESTAMP() WHERE TaiKhoanID=?",
            body.Required("name","họ tên",150),body.Text("phone",20).NullIfEmpty(),body.Text("address",500).NullIfEmpty(),user),token);
        return Ok;
    }

    public async Task<object> StaffPassword(HttpContext context,long id,JsonElement body)
    {
        var hash = Password.Hash(Password.Validate(body.String("password")));
        await sql.Transaction(async db => {
            var account = await db.One("SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? AND TrangThai='HOAT_DONG' AND NgayXoa IS NULL FOR UPDATE",id);
            var sessionRow = await db.One("SELECT PhienID FROM PhienDangNhap WHERE TaiKhoanID=? AND TokenHash=? AND ThuHoiLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP() FOR UPDATE",
                id,Password.TokenHash(context.Request.Cookies[session.Cookie]!));
            if (account is null || sessionRow is null) throw new ApiException("Vui lòng đăng nhập để tiếp tục.",401);
            await SavePassword(db,id,hash);
            await db.Audit(id,"DOI_MAT_KHAU","TaiKhoan",id,"Nhân sự tự đổi mật khẩu");
            return Ok;
        },context.RequestAborted);
        context.Response.Cookies.Delete(session.Cookie,SessionControl.CookieOptions(context));
        return Ok;
    }

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
                await db.Execute("INSERT INTO DatLaiMatKhau(TaiKhoanID,TokenHash,MaXacMinhHash,HetHanLuc) VALUES(?,?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 10 MINUTE))",id,hash,OtpHash(challenge,code));
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

    private static async Task<dynamic> LockReset(SqlSession db,byte[] hash)
    {
        var id = await db.Value<long?>("SELECT TaiKhoanID FROM DatLaiMatKhau WHERE TokenHash=?",hash);
        if (id is null || await db.One("SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? AND TrangThai='HOAT_DONG' FOR UPDATE",id) is null)
            throw new ApiException(Invalid);
        return await db.One("SELECT * FROM DatLaiMatKhau WHERE TokenHash=? AND DaSuDungLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP() FOR UPDATE",hash)
            ?? throw new ApiException(Invalid);
    }

    public async Task<object> Verify(JsonElement body,CancellationToken token)
    {
        var challenge = body.Text("challenge");
        var code = body.Text("code");
        if (!Password.ValidToken(challenge) || !Regex.IsMatch(code,@"\A[0-9]{6}\z")) throw new ApiException("Nhập đủ mã xác minh 6 chữ số.");
        var resetToken = Password.Token();
        var error = await sql.Transaction<string?>(async db => {
            var row = await LockReset(db,Password.TokenHash(challenge));
            var expected = row.MaXacMinhHash as byte[];
            if (expected is null || (int)row.SoLanThu >= 5) throw new ApiException(Invalid);
            if (!CryptographicOperations.FixedTimeEquals(OtpHash(challenge,code),expected)) {
                var attempts = (int)row.SoLanThu + 1;
                await db.Execute("UPDATE DatLaiMatKhau SET SoLanThu=?,DaSuDungLuc=IF(? >= 5,UTC_TIMESTAMP(),NULL) WHERE YeuCauID=?",attempts,attempts,(long)row.YeuCauID);
                return attempts >= 5 ? "Bạn đã nhập sai 5 lần. Vui lòng yêu cầu mã mới." : "Mã xác minh chưa đúng.";
            }
            await db.Execute("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE YeuCauID=?",(long)row.YeuCauID);
            await db.Execute("INSERT INTO DatLaiMatKhau(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 10 MINUTE))",
                (long)row.TaiKhoanID,Password.TokenHash(resetToken));
            return null;
        },token);
        if (error != null) throw new ApiException(error);
        return new { resetToken, expiresIn = 600 };
    }

    public Task<object> Reset(JsonElement body,CancellationToken token)
    {
        var key = body.Text("token");
        if (!Password.ValidToken(key)) throw new ApiException("Phiên đặt lại mật khẩu không hợp lệ.");
        var password = Password.Validate(body.String("password"));
        return sql.Transaction(async db => {
            var row = await LockReset(db,Password.TokenHash(key));
            if (row.MaXacMinhHash is not null) throw new ApiException("Bạn cần xác minh OTP trước khi đặt mật khẩu mới.");
            await SavePassword(db,(long)row.TaiKhoanID,Password.Hash(password));
            return Ok;
        },token);
    }

    private static async Task SavePassword(SqlSession db,long id,string hash)
    {
        await db.Execute("UPDATE TaiKhoan SET MatKhauHash=?,NgayDoiMatKhau=UTC_TIMESTAMP(),SoLanDangNhapSai=0,KhoaDenLuc=NULL WHERE TaiKhoanID=?",hash,id);
        await db.Execute("UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL",id);
        await db.Execute("UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=?",id);
    }
}
