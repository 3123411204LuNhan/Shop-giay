namespace ShopGiay.Entity;

public sealed class PhanBoTonKho
{
    public long ChiTietDonHangID { get; init; }
    public long BienTheID { get; init; }
    public int SoLuongGiu { get; init; }
    public string TrangThai { get; private set; } = "DANG_GIU";
    public DateTime NgayPhanBo { get; init; }
    public DateTime? HanGiuLuc { get; init; }
    public DateTime? NgayKetThuc { get; private set; }

    // Kết thúc giữ hàng khi đơn bị hủy và trả số lượng cần giải phóng.
    public int GiaiPhong(DateTime thoiDiem)
    {
        KetThuc("DA_GIAI_PHONG",thoiDiem);
        return SoLuongGiu;
    }

    // Kết thúc giữ hàng khi xuất giao và trả số lượng cần trừ khỏi kho.
    public int XuatHang(DateTime thoiDiem)
    {
        KetThuc("DA_XUAT",thoiDiem);
        return SoLuongGiu;
    }

    // Chỉ ghi trạng thái kết thúc nếu dòng còn giữ hàng.
    private void KetThuc(string trangThai,DateTime thoiDiem)
    {
        if (TrangThai != "DANG_GIU") throw new DomainRuleException("Dòng hàng không còn giữ tồn.");
        (TrangThai,NgayKetThuc) = (trangThai,thoiDiem);
    }
}
