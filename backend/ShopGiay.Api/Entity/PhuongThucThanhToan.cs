namespace ShopGiay.Entity;

public sealed class PhuongThucThanhToan
{
    public int PhuongThucID { get; init; }
    public required string MaPhuongThuc { get; init; }
    public required string TenPhuongThuc { get; init; }
    public bool DangHoatDong { get; init; }
}
