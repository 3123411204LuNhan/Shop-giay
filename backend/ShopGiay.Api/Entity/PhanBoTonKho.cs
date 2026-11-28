namespace ShopGiay.Entity;

public sealed class PhanBoTonKho
{
    public long ChiTietDonHangID { get; init; }
    public int SoLuongGiu { get; init; }
    public required string TrangThai { get; init; }
    public DateTime NgayPhanBo { get; init; }
    public DateTime? HanGiuLuc { get; init; }
    public DateTime? NgayKetThuc { get; init; }
}
