namespace ShopGiay.Entity;

public sealed class VaiTro
{
    public int VaiTroID { get; init; }
    public required string MaVaiTro { get; init; }
    public required string TenVaiTro { get; init; }
    public string? MoTa { get; init; }

    // Kiểm tra bộ vai trò theo khu vực. Tài khoản nhân sự không dùng phiên khách.
    public static bool KiemTraQuyen(string khuVuc,IEnumerable<string> roles) => khuVuc switch {
        "manager" => roles.Contains("SHOP_MANAGER"),
        "admin" => roles.Contains("SYSTEM_ADMIN"),
        _ => roles.Contains("CUSTOMER") && !roles.Contains("SHOP_MANAGER") && !roles.Contains("SYSTEM_ADMIN")
    };
}
