namespace ShopGiay.Entity;

public sealed class ChiTietDoiTra
{
    public long ChiTietDoiTraID { get; init; }
    public long YeuCauDoiTraID { get; init; }
    public long DonHangID { get; init; }
    public long ChiTietDonHangID { get; init; }
    public int SoLuongYeuCau { get; init; }
    public int SoLuongDuyet { get; init; }
    public int SoLuongDaNhan { get; init; }
    public int SoLuongBanLai { get; init; }
    public decimal TienHoanDuocDuyet { get; init; }
    public long? BienTheDoiSangID { get; init; }
    public string? TinhTrangHang { get; init; }
    public long? NguoiNhanHangID { get; init; }
    public DateTime? NgayNhanHang { get; init; }
}
