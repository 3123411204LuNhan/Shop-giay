namespace ShopGiay.Entity;

public sealed class BienDongKho
{
    public long BienDongID { get; init; }
    public long BienTheID { get; init; }
    public required string LoaiBienDong { get; init; }
    public int ThayDoiTon { get; init; }
    public int ThayDoiPhanBo { get; init; }
    public int TonSau { get; init; }
    public int PhanBoSau { get; init; }
    public long? ChiTietPhieuKhoID { get; init; }
    public long? ChiTietDonHangID { get; init; }
    public long? NguoiThucHienID { get; init; }
    public Guid MaThaoTac { get; init; }
    public required string LyDo { get; init; }
    public DateTime ThoiDiem { get; init; }
}
