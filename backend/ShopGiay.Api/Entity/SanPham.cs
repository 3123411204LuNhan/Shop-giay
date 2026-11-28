namespace ShopGiay.Entity;

public sealed class SanPham
{
    public long SanPhamID { get; init; }
    public string MaSanPham { get; private set; } = "";
    public string TenSanPham { get; private set; } = "";
    public string? MoTa { get; private set; }
    public decimal GiaBan { get; private set; }
    public int DanhMucID { get; private set; }
    public int ThuongHieuID { get; private set; }
    public string TrangThai { get; private set; } = "AN";
    public long? NguoiTaoID { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }

    // Áp dụng nội dung, giá và phân loại sau khi Control chuẩn hóa dữ liệu nhập.
    public void CapNhatThongTin(string ma,string ten,string moTa,decimal gia,int danhMuc,int thuongHieu) =>
        (MaSanPham,TenSanPham,MoTa,GiaBan,DanhMucID,ThuongHieuID) = (ma,ten,moTa,gia,danhMuc,thuongHieu);

    // Kiểm tra giới hạn giá rồi làm tròn theo đơn vị VNĐ.
    public static decimal ChuanHoaGia(decimal gia)
    {
        if (gia <= 0 || gia > 10000000000m) throw new DomainRuleException("Giá tiền không hợp lệ.");
        return Math.Round(gia,MidpointRounding.AwayFromZero);
    }

    // Chuyển trạng thái kinh doanh và chuẩn hóa tên trạng thái nháp của dữ liệu cũ.
    public void DatTrangThai(string trangThai) => TrangThai = trangThai switch {
        "NHAP" => "AN",
        "AN" or "DANG_BAN" or "NGUNG_BAN" => trangThai,
        _ => throw new DomainRuleException("Trạng thái sản phẩm không hợp lệ.")
    };
}
