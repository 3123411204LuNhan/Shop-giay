namespace ShopGiay.Entity;

public sealed class LichSuDonHang
{
    public long LichSuID { get; init; }
    public long DonHangID { get; init; }
    public string? TrangThaiCu { get; init; }
    public required string TrangThaiMoi { get; init; }
    public long? NguoiThucHienID { get; init; }
    public DateTime ThoiDiem { get; init; }
    public string? GhiChu { get; init; }
}
