namespace ShopGiay.Entity;

public sealed class PhienDangNhap
{
    public Guid PhienID { get; init; }
    public long TaiKhoanID { get; init; }
    public required byte[] TokenHash { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime HetHanLuc { get; init; }
    public DateTime? ThuHoiLuc { get; init; }
    public string? DiaChiIP { get; init; }
    public string? UserAgent { get; init; }
}
