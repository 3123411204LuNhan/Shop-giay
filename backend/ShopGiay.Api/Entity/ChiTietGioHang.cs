namespace ShopGiay.Entity;

public sealed class ChiTietGioHang
{
    public long GioHangID { get; init; }
    public long BienTheID { get; init; }
    public int SoLuong { get; init; }
    public DateTime NgayThem { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
