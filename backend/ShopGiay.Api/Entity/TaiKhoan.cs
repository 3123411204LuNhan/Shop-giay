namespace ShopGiay.Entity;

public sealed class TaiKhoan
{
    public long TaiKhoanID { get; init; }
    public required string TenDangNhap { get; init; }
    public string? SoDienThoai { get; init; }
    public required string Email { get; init; }
    public required string MatKhauHash { get; init; }
    public required string HoTen { get; init; }
    public DateOnly? NgaySinh { get; init; }
    public string? GioiTinh { get; init; }
    public string? DiaChi { get; init; }
    public string? AnhDaiDien { get; init; }
    public required string TrangThai { get; init; }
    public bool EmailDaXacMinh { get; init; }
    public int SoLanDangNhapSai { get; init; }
    public DateTime? KhoaDenLuc { get; init; }
    public DateTime? LanDangNhapCuoi { get; init; }
    public DateTime? NgayDoiMatKhau { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
