namespace ShopGiay.Entity;

public sealed class MauSac
{
    public int MauSacID { get; init; }
    public required string TenMau { get; init; }
    public string? MaHex { get; init; }
    public bool DangHoatDong { get; init; }
}
