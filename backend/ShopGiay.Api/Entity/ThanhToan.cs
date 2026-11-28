namespace ShopGiay.Entity;

public sealed class ThanhToan
{
    public long ThanhToanID { get; init; }
    public long DonHangID { get; init; }
    public int PhuongThucID { get; init; }
    public Guid MaYeuCau { get; init; }
    public string? CongThanhToan { get; init; }
    public string? MaGiaoDich { get; init; }
    public decimal SoTien { get; init; }
    public required string TrangThai { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime? NgayXacNhan { get; init; }
    public long? NguoiXacNhanID { get; init; }
    public string? BangChung { get; init; }
    public string? ThongBaoLoi { get; init; }
}
