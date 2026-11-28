namespace ShopGiay.Entity;

public sealed class TonKho
{
    public long BienTheID { get; init; }
    public int SoLuongTon { get; private set; }
    public int SoLuongDaPhanBo { get; private set; }
    public int SoLuongKhaDung => SoLuongTon - SoLuongDaPhanBo;
    public int MucCanhBao { get; init; }
    public DateTime NgayCapNhat { get; init; }

    // Khôi phục số dư tồn và phần đã giữ từ dữ liệu kho.
    public TonKho(int ton, int daPhanBo)
    {
        if (ton < 0 || daPhanBo < 0 || daPhanBo > ton)
            throw new ArgumentOutOfRangeException(nameof(daPhanBo));
        SoLuongTon = ton;
        SoLuongDaPhanBo = daPhanBo;
    }

    // So sánh số lượng yêu cầu với phần tồn chưa phân bổ.
    public bool ConDuHang(int soLuong) => soLuong > 0 && soLuong <= SoLuongKhaDung;

    // Nhập thêm hàng sau khi kiểm tra số lượng dương.
    public void TangTon(int soLuong)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(soLuong);
        SoLuongTon = checked(SoLuongTon + soLuong);
    }

    // Chỉ trừ phần tồn vật lý còn khả dụng.
    public void GiamTon(int soLuong)
    {
        if (!ConDuHang(soLuong)) throw new InvalidOperationException("Không đủ tồn khả dụng.");
        SoLuongTon -= soLuong;
    }

    // Giữ một phần hàng còn khả dụng cho đơn.
    public void PhanBo(int soLuong)
    {
        if (!ConDuHang(soLuong)) throw new InvalidOperationException("Không đủ tồn khả dụng.");
        SoLuongDaPhanBo += soLuong;
    }

    // Giảm lượng đang giữ mà không thay đổi tồn vật lý.
    public void GiaiPhong(int soLuong)
    {
        if (soLuong <= 0 || soLuong > SoLuongDaPhanBo) throw new ArgumentOutOfRangeException(nameof(soLuong));
        SoLuongDaPhanBo -= soLuong;
    }

    // Kiểm tra số dư sau cùng, sau đó giải phóng, đổi tồn vật lý và giữ thêm theo thứ tự.
    // Khi xuất hàng phải giải phóng lượng giữ trước để giảm tồn mà không tính trùng.
    public bool ApDungBienDong(int thayDoiTon,int thayDoiPhanBo)
    {
        var ton = (long)SoLuongTon + thayDoiTon;
        var phanBo = (long)SoLuongDaPhanBo + thayDoiPhanBo;
        if (phanBo < 0 || ton > int.MaxValue || ton < phanBo) return false;
        if (thayDoiPhanBo < 0) GiaiPhong(-thayDoiPhanBo);
        if (thayDoiTon > 0) TangTon(thayDoiTon);
        if (thayDoiTon < 0) GiamTon(-thayDoiTon);
        if (thayDoiPhanBo > 0) PhanBo(thayDoiPhanBo);
        return true;
    }
}
