namespace ShopGiay.Entity;

public sealed class DanhMuc
{
    public int DanhMucID { get; init; }
    public required string TenDanhMuc { get; init; }
    public string? MoTa { get; init; }
    public string? HinhAnh { get; init; }
    public bool DangHoatDong { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
