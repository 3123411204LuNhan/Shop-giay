namespace ShopGiay.Entity;

public sealed class ChiTietPhieuKho
{
    public long ChiTietPhieuKhoID { get; init; }
    public long PhieuKhoID { get; init; }
    public long BienTheID { get; init; }
    public int SoLuongThayDoi { get; init; }
    public decimal? DonGiaNhap { get; init; }
    public long? ChiTietDoiTraID { get; init; }
    public string? GhiChu { get; init; }
}
