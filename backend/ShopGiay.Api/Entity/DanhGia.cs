namespace ShopGiay.Entity;

public sealed class DanhGia
{
    public long DanhGiaID { get; init; }
    public long DonHangID { get; init; }
    public long ChiTietDonHangID { get; init; }
    public long KhachHangID { get; init; }
    public byte SoSao { get; init; }
    public string? NoiDung { get; init; }
    public required string TrangThai { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
