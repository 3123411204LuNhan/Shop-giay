namespace ShopGiay.Entity;

public sealed class ChiTietDoiTra
{
    public long ChiTietDoiTraID { get; init; }
    public long YeuCauDoiTraID { get; init; }
    public long DonHangID { get; init; }
    public long ChiTietDonHangID { get; init; }
    public int SoLuongYeuCau { get; init; }
    public int SoLuongDuyet { get; private set; }
    public int SoLuongDaNhan { get; private set; }
    public int SoLuongBanLai { get; private set; }
    public decimal TienHoanDuocDuyet { get; private set; }
    public long? BienTheDoiSangID { get; init; }
    public string? TinhTrangHang { get; private set; }
    public long? NguoiNhanHangID { get; private set; }
    public DateTime? NgayNhanHang { get; private set; }
    public long BienTheID { get; init; }
    public string MaSanPhamLucMua { get; init; } = "";
    public string TenSanPhamLucMua { get; init; } = "";
    public string DanhMucLucMua { get; init; } = "";
    public string ThuongHieuLucMua { get; init; } = "";
    public decimal DonGia { get; init; }
    public int SoLuongLucMua { get; init; }
    public decimal ThanhTienLucMua { get; init; }

    // Duyệt toàn bộ số lượng yêu cầu và lưu số tiền hoàn đã phân bổ.
    public void Duyet(decimal tienHoan) => (SoLuongDuyet,TienHoanDuocDuyet) = (SoLuongYeuCau,tienHoan);

    // Ghi nhận đủ lượng được duyệt và chỉ định phần có thể nhập lại để bán.
    public void GhiNhanNhanHang(int banLai,long actor,string tinhTrang,DateTime thoiDiem)
    {
        if (banLai < 0 || banLai > SoLuongDuyet)
            throw new DomainRuleException("Số đôi bán lại phải từ 0 đến số đã nhận.");
        (SoLuongDaNhan,SoLuongBanLai,NguoiNhanHangID,TinhTrangHang,NgayNhanHang) =
            (SoLuongDuyet,banLai,actor,tinhTrang,thoiDiem);
    }
}
