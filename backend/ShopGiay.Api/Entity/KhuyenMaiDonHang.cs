namespace ShopGiay.Entity;

public sealed class KhuyenMaiDonHang
{
    public long DonHangID { get; init; }
    public long KhuyenMaiID { get; init; }
    public required string MaLucApDung { get; init; }
    public required string TenLucApDung { get; init; }
    public required string HinhThucLucApDung { get; init; }
    public decimal GiaTriLucApDung { get; init; }
    public decimal TienGiamThucTe { get; init; }
    public DateTime NgayApDung { get; init; }
}
