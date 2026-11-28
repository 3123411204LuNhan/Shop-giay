namespace ShopGiay.Entity;

public sealed class LichSuKhuyenMai
{
    public long LichSuID { get; init; }
    public long KhuyenMaiID { get; init; }
    public long NguoiThucHienID { get; init; }
    public required string HanhDong { get; init; }
    public required string NoiDung { get; init; }
    public DateTime ThoiDiem { get; init; }
}
