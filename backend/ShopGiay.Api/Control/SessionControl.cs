using ShopGiay.Entity;
using ShopGiay.Infrastructure;

namespace ShopGiay.Control;

public sealed record SessionUser(long Id, string Name, string Email, string? Phone, string? Address, string Username, string[] Roles);
public sealed class SessionControl(Sql sql,AccountRepository accounts)
{
    public SessionUser? User { get; private set; }
    public string Area { get; private set; } = "customer";
    public string Cookie => $"sole_{Area}_session";
    // Dùng cookie HTTP-only theo miền gốc và bật Secure khi kết nối HTTPS.
    public static CookieOptions CookieOptions(HttpContext context) =>
        new() { HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Strict, Path = "/" };

    // Lấy người dùng đã xác thực và kiểm tra khu vực khi endpoint yêu cầu.
    public SessionUser Require(string? area = null)
    {
        if (User is null) throw new ApiException("Vui lòng đăng nhập để tiếp tục.",401);
        if (area is not null && Area != area) throw new ApiException("Tài khoản không có quyền thực hiện chức năng này.",403);
        return User;
    }

    // Chỉ trả phiên nhân sự thuộc khu vực quản lý hoặc kỹ thuật.
    public SessionUser RequireStaff()
    {
        var user = Require();
        return Area is "manager" or "admin" ? user : throw new ApiException("Tài khoản không có quyền thực hiện chức năng này.",403);
    }

    // Chọn cookie theo khu vực, kiểm tra bản băm còn hiệu lực rồi áp dụng quyền của bộ vai trò.
    public async Task Identify(HttpContext context)
    {
        var request = context.Request;
        var area = request.Headers["X-Sole-Area"].ToString();
        Area = area is "customer" or "manager" or "admin" ? area :
            request.Path.StartsWithSegments("/api/manage") ? "manager" :
            request.Path.StartsWithSegments("/api/admin") ? "admin" : "customer";
        var cookie = Cookie;
        var options = CookieOptions(context);
        if (request.Cookies.ContainsKey("sole_session")) context.Response.Cookies.Delete("sole_session", options);
        if (!request.Cookies.TryGetValue(cookie, out var token)) return;
        if (Password.ValidToken(token)) {
            var rows = await sql.Run(db => accounts.NhanDien(db,Password.TokenHash(token)),context.RequestAborted);
            var roles = rows.Select(x => x.MaVaiTro).ToArray();
            if (rows.FirstOrDefault() is { } account && VaiTro.KiemTraQuyen(Area,roles))
                User = new(account.TaiKhoanID,account.HoTen,account.Email,account.SoDienThoai,account.DiaChi,account.TenDangNhap,roles);
        }
        if (User is null) context.Response.Cookies.Delete(cookie,options);
    }
}
