namespace ShopGiay.Web.Services;

// Kết quả đăng nhập: sai thông tin hoặc sai khu vực là kết quả HTTP, không phải exception làm debugger dừng.
public sealed record LoginResult(int StatusCode, object Body)
{
    // AuthService → AuthController: giữ hợp đồng JSON lỗi mà React đang đọc.
    public static LoginResult Rejected(string message, int statusCode) => new(statusCode, new { message });

    // Chỉ tạo kết quả thành công sau khi phiên đúng khu vực đã được lưu.
    public static LoginResult Accepted(string area) => new(200, new { ok = true, area });
}
