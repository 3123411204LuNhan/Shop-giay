namespace ShopGiay.Entity;

public sealed class TaiKhoanVaiTro
{
    public long TaiKhoanID { get; init; }
    public int VaiTroID { get; init; }
    public long? NguoiGanID { get; init; }
    public DateTime NgayGan { get; init; }
}
