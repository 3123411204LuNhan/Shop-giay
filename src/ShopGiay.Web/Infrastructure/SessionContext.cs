using System.Security.Cryptography;
using System.Text;

namespace ShopGiay.Web.Infrastructure;
// Scoped: mỗi HTTP request có tài khoản và khu vực độc lập.
public sealed class SessionContext(IHttpContextAccessor accessor, Database db)
{
    public HttpContext Http => accessor.HttpContext!;
    public Row? User { get; private set; }
    public long UserId => User?.L("id") ?? throw new ApiException("Vui lòng đăng nhập để tiếp tục.", 401);
    public string[] Roles { get; private set; } = [];
    public string Area { get; private set; } = "customer";

    public static readonly Dictionary<string, string> AreaRoles = new()
    {
        ["customer"] = "CUSTOMER",
        ["manager"] = "SHOP_MANAGER",
        ["admin"] = "SYSTEM_ADMIN"
    };
    public string CookieName => "sole_" + Area + "_session";
    public string Token => Http.Request.Cookies[CookieName] ?? "";

    // SHA-256 của token được lưu trong database; cookie giữ token ngẫu nhiên.
    public static byte[] Hash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
    // Tạo token phiên/OTP grant từ bộ sinh số ngẫu nhiên mật mã.
    public static string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    // Khu khách hàng không chấp nhận tài khoản nhân sự.
    public static bool CanUse(string[] roles, string area) => AreaRoles.TryGetValue(area, out var role) && roles.Contains(role) && (area != "customer" || !roles.Any(x => x is "SHOP_MANAGER" or "SYSTEM_ADMIN"));
    // Middleware → Identify → PhienDangNhap/TaiKhoan/VaiTro.
    public async Task Identify()
    {
        // Tạo biến path để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var path = Http.Request.Path.Value ?? "";
        // Tạo biến requested để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var requested = Http.Request.Headers["x-sole-area"].ToString();
        // Gán kết quả vào Area để bước xử lý tiếp theo dùng dữ liệu mới.
        Area = AreaRoles.ContainsKey(requested) ? requested : path.StartsWith("/api/manage/") ? "manager" : path.StartsWith("/api/admin/") ? "admin" : "customer";
        // Gán kết quả vào Http.Response.Headers.CacheControl để bước xử lý tiếp theo dùng dữ liệu mới.
        Http.Response.Headers.CacheControl = "no-store";
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (Http.Request.Cookies.ContainsKey("sole_session"))
            // Thực hiện bước Http.Response.Cookies.Delete trong SessionContext; tiếp tục theo luồng của hàm.
            Http.Response.Cookies.Delete("sole_session", Options());
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (Token.Length == 0)
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!System.Text.RegularExpressions.Regex.IsMatch(Token, "^[a-f0-9]{64}$"))
        {
            // Thực hiện bước Clear trong SessionContext; tiếp tục theo luồng của hàm.
            Clear();
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return;
        }

        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        var rows = await db.Query("SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.SoDienThoai phone,t.DiaChi address,t.TenDangNhap username FROM PhienDangNhap p JOIN TaiKhoan t ON t.TaiKhoanID=p.TaiKhoanID WHERE p.TokenHash=? AND p.ThuHoiLuc IS NULL AND p.HetHanLuc>UTC_TIMESTAMP() AND t.TrangThai='HOAT_DONG' AND t.NgayXoa IS NULL", [Hash(Token)]);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (rows.FirstOrDefault()is { } user)
        {
            // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            var roles = await db.Query("SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=?", [user.L("id")]);
            // Gán kết quả vào Roles để bước xử lý tiếp theo dùng dữ liệu mới.
            Roles = roles.Select(x => x.S("role")).ToArray();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (CanUse(Roles, Area))
            {
                // Gán kết quả vào User để bước xử lý tiếp theo dùng dữ liệu mới.
                User = user;
                // Gán kết quả vào User["roles"] để bước xử lý tiếp theo dùng dữ liệu mới.
                User["roles"] = Roles;
            }
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (User is null)
            // Thực hiện bước Clear trong SessionContext; tiếp tục theo luồng của hàm.
            Clear();
    }

    // API xác định vai trò trước khi gọi bất kỳ Service có quyền hạn.
    public void RequireRole(string role)
    {
        // Gán kết quả vào _ để bước xử lý tiếp theo dùng dữ liệu mới.
        _ = UserId;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!Roles.Contains(role))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Tài khoản không có quyền thực hiện chức năng này.", 403);
    }

    // Sau đăng nhập/đăng ký: lưu hash token và đặt cookie HttpOnly.
    public async Task Create(long userId)
    {
        // Tạo biến token để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var token = NewToken();
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("INSERT INTO PhienDangNhap(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 7 DAY))", [userId, Hash(token)]);
        // Tạo biến options để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var options = Options();
        // Gán kết quả vào options.MaxAge để bước xử lý tiếp theo dùng dữ liệu mới.
        options.MaxAge = TimeSpan.FromDays(7);
        // Thực hiện bước Http.Response.Cookies.Append trong SessionContext; tiếp tục theo luồng của hàm.
        Http.Response.Cookies.Append(CookieName, token, options);
    }

    // Cookie chỉ được gửi trong cùng site; Secure tự bật khi chạy HTTPS.
    private CookieOptions Options() => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        Secure = Http.Request.IsHttps,
        Path = "/"
    };
    // Thu hồi cookie đúng khu vực, không đăng xuất các khu vực khác.
    public void Clear() => Http.Response.Cookies.Delete(CookieName, Options());
}
