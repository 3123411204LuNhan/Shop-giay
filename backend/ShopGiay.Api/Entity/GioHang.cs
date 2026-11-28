namespace ShopGiay.Entity;

public sealed class GioHang
{
    public long GioHangID { get; init; }
    public long? TaiKhoanID { get; init; }
    public byte[]? MaPhienHash { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
    public DateTime? HetHanLuc { get; init; }
}
