using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using ShopGiay.Infrastructure;

namespace ShopGiay.Control;

public sealed record SessionUser(long Id, string Name, string Email, string? Phone, string? Address, string Username, string[] Roles);
public sealed class SessionControl(MySqlDataSource database)
{
    public SessionUser? User { get; private set; }
    public string Area { get; private set; } = "customer";
    public string Cookie => $"sole_{Area}_session";
    public static CookieOptions CookieOptions(HttpContext context) =>
        new() { HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Strict, Path = "/" };

    public static bool CanUse(string area,IEnumerable<string> roles) => area switch {
        "manager" => roles.Contains("SHOP_MANAGER"),
        "admin" => roles.Contains("SYSTEM_ADMIN"),
        _ => roles.Contains("CUSTOMER") && !roles.Contains("SHOP_MANAGER") && !roles.Contains("SYSTEM_ADMIN")
    };

    public SessionUser Require(string? area = null)
    {
        if (User is null) throw new ApiException("Vui lòng đăng nhập để tiếp tục.",401);
        if (area is not null && Area != area) throw new ApiException("Tài khoản không có quyền thực hiện chức năng này.",403);
        return User;
    }

    public SessionUser RequireStaff()
    {
        var user = Require();
        return Area is "manager" or "admin" ? user : throw new ApiException("Tài khoản không có quyền thực hiện chức năng này.",403);
    }

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
        if (token.Length == 64 && token.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f')) {
            await using var db = await database.OpenConnectionAsync(context.RequestAborted);
            await using var cmd = new MySqlCommand("""
                SELECT t.TaiKhoanID,t.HoTen,t.Email,t.SoDienThoai,t.DiaChi,t.TenDangNhap,v.MaVaiTro
                FROM PhienDangNhap p JOIN TaiKhoan t ON t.TaiKhoanID=p.TaiKhoanID
                JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID
                WHERE p.TokenHash=@hash AND p.ThuHoiLuc IS NULL AND p.HetHanLuc>UTC_TIMESTAMP()
                AND t.TrangThai='HOAT_DONG' AND t.NgayXoa IS NULL
                """, db);
            cmd.Parameters.AddWithValue("hash", SHA256.HashData(Encoding.ASCII.GetBytes(token)));
            await using var reader = await cmd.ExecuteReaderAsync(context.RequestAborted);
            var roles = new List<string>();
            SessionUser? account = null;
            while (await reader.ReadAsync(context.RequestAborted)) {
                account ??= new(reader.GetInt64(0),reader.GetString(1),reader.GetString(2),
                    reader.IsDBNull(3)?null:reader.GetString(3),reader.IsDBNull(4)?null:reader.GetString(4),reader.GetString(5),[]);
                roles.Add(reader.GetString(6));
            }
            if (account is not null && CanUse(Area,roles)) User = account with { Roles = roles.ToArray() };
        }
        if (User is null) context.Response.Cookies.Delete(cookie,options);
    }
}
