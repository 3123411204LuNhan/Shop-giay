namespace ShopGiay.Entity;

public sealed class SanPham
{
    public long SanPhamID { get; init; }
    public required string MaSanPham { get; init; }
    public required string TenSanPham { get; init; }
    public string? MoTa { get; init; }
    public decimal GiaBan { get; init; }
    public int DanhMucID { get; init; }
    public int ThuongHieuID { get; init; }
    public required string TrangThai { get; init; }
    public long? NguoiTaoID { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
