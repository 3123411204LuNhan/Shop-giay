namespace ShopGiay.Entity;

public sealed class PhieuKho
{
    public long PhieuKhoID { get; init; }
    public required string MaPhieu { get; init; }
    public Guid MaChongLap { get; init; }
    public required string LoaiPhieu { get; init; }
    public string? MaChungTuThamChieu { get; init; }
    public required string LyDo { get; init; }
    public long NguoiThucHienID { get; init; }
    public DateTime NgayTao { get; init; }
}
