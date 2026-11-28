namespace ShopGiay.Entity;

public sealed class HinhAnhSanPham
{
    public long HinhAnhID { get; init; }
    public long SanPhamID { get; init; }
    public long? BienTheID { get; init; }
    public required string DuongDan { get; init; }
    public string? MoTaAnh { get; init; }
    public bool LaAnhChinh { get; init; }
    public int ThuTu { get; init; }
}
