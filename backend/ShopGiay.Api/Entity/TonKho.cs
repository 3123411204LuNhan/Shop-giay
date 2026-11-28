namespace ShopGiay.Entity;

public sealed class TonKho
{
    public long BienTheID { get; init; }
    public int SoLuongTon { get; private set; }
    public int SoLuongDaPhanBo { get; private set; }
    public int SoLuongKhaDung => SoLuongTon - SoLuongDaPhanBo;
    public int MucCanhBao { get; init; }
    public DateTime NgayCapNhat { get; init; }

    public TonKho(int ton, int daPhanBo)
    {
        if (ton < 0 || daPhanBo < 0 || daPhanBo > ton)
            throw new ArgumentOutOfRangeException(nameof(daPhanBo));
        SoLuongTon = ton;
        SoLuongDaPhanBo = daPhanBo;
    }

    public bool ConDuHang(int soLuong) => soLuong > 0 && soLuong <= SoLuongKhaDung;

    public void TangTon(int soLuong)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soLuong);
        SoLuongTon = checked(SoLuongTon + soLuong);
    }

    public void GiamTon(int soLuong)
    {
        if (!ConDuHang(soLuong)) throw new InvalidOperationException("Không đủ tồn khả dụng.");
        SoLuongTon -= soLuong;
    }

    public void PhanBo(int soLuong)
    {
        if (!ConDuHang(soLuong)) throw new InvalidOperationException("Không đủ tồn khả dụng.");
        SoLuongDaPhanBo += soLuong;
    }

    public void GiaiPhong(int soLuong)
    {
        if (soLuong <= 0 || soLuong > SoLuongDaPhanBo) throw new ArgumentOutOfRangeException(nameof(soLuong));
        SoLuongDaPhanBo -= soLuong;
    }
}
