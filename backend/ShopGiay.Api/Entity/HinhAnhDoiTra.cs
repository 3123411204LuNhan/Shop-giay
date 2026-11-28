namespace ShopGiay.Entity;

public sealed class HinhAnhDoiTra
{
    public long HinhAnhID { get; init; }
    public long YeuCauDoiTraID { get; init; }
    public required string DuongDan { get; init; }
    public string? MoTa { get; init; }
    public DateTime NgayTao { get; init; }
}
