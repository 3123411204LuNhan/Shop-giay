namespace ShopGiay.Entity;

public sealed class LichSuDoiTra
{
    public long LichSuID { get; init; }
    public long YeuCauDoiTraID { get; init; }
    public string? TrangThaiCu { get; init; }
    public required string TrangThaiMoi { get; init; }
    public long NguoiThucHienID { get; init; }
    public required string NoiDung { get; init; }
    public DateTime ThoiDiem { get; init; }
}
