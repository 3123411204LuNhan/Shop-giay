namespace ShopGiay.Entity;

public sealed class DatLaiMatKhau
{
    public long YeuCauID { get; init; }
    public long TaiKhoanID { get; init; }
    public required byte[] TokenHash { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime HetHanLuc { get; init; }
    public DateTime? DaSuDungLuc { get; init; }
    public int SoLanThu { get; init; }
}
