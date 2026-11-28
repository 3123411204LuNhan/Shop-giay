namespace ShopGiay.Entity;

public sealed class NhatKyHeThong
{
    public long NhatKyID { get; init; }
    public long? NguoiThucHienID { get; init; }
    public required string HanhDong { get; init; }
    public required string DoiTuong { get; init; }
    public string? MaDoiTuong { get; init; }
    public required string NoiDung { get; init; }
    public required string KetQua { get; init; }
    public string? DiaChiIP { get; init; }
    public Guid? MaYeuCau { get; init; }
    public DateTime ThoiDiem { get; init; }
}
