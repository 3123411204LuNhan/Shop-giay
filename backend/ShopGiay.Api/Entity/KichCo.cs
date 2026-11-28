namespace ShopGiay.Entity;

public sealed class KichCo
{
    public int KichCoID { get; init; }
    public required string HeSize { get; init; }
    public required string GiaTri { get; init; }
    public int ThuTu { get; init; }
    public bool DangHoatDong { get; init; }
}
