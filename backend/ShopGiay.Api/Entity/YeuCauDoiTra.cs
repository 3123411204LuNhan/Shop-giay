namespace ShopGiay.Entity;

public sealed class YeuCauDoiTra
{
    public long YeuCauDoiTraID { get; init; }
    public required string MaYeuCau { get; init; }
    public Guid MaChongLap { get; init; }
    public long DonHangID { get; init; }
    public long KhachHangID { get; init; }
    public required string LoaiYeuCau { get; init; }
    public required string LyDo { get; init; }
    public required string TrangThai { get; init; }
    public long? NguoiDuyetID { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime? NgayDuyet { get; init; }
    public string? LyDoXuLy { get; init; }
    public DateTime? NgayHoanTat { get; init; }
    public long? DonHangDoiMoiID { get; init; }
}
