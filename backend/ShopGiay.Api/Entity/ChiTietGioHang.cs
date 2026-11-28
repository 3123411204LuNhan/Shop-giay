namespace ShopGiay.Entity;

public sealed class ChiTietGioHang
{
    public long GioHangID { get; init; }
    public long BienTheID { get; init; }
    public int SoLuong { get; private set; }
    public DateTime NgayThem { get; init; }
    public DateTime NgayCapNhat { get; init; }

    // Kiểm tra số lượng dương rồi thay số lượng của dòng giỏ.
    public void CapNhatSoLuong(int soLuong)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soLuong);
        SoLuong = soLuong;
    }
}
