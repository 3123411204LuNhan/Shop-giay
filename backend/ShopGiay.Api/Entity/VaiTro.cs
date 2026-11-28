namespace ShopGiay.Entity;

public sealed class VaiTro
{
    public int VaiTroID { get; init; }
    public required string MaVaiTro { get; init; }
    public required string TenVaiTro { get; init; }
    public string? MoTa { get; init; }
}
