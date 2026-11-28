namespace ShopGiay.Entity;

public sealed class HoanTien
{
    public long HoanTienID { get; init; }
    public long DonHangID { get; init; }
    public long ThanhToanID { get; init; }
    public long? YeuCauDoiTraID { get; init; }
    public Guid MaYeuCau { get; init; }
    public required string LyDo { get; init; }
    public decimal SoTien { get; init; }
    public required string TrangThai { get; init; }
    public string? KenhHoanTien { get; init; }
    public string? MaGiaoDichHoan { get; init; }
    public string? BangChung { get; init; }
    public long? NguoiXuLyID { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime? NgayXacNhan { get; init; }
}
