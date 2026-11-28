namespace ShopGiay.Entity;

public sealed class ThuongHieu
{
    public int ThuongHieuID { get; init; }
    public required string TenThuongHieu { get; init; }
    public string? MoTa { get; init; }
    public string? HinhAnh { get; init; }
    public bool DangHoatDong { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
