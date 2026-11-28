namespace ShopGiay.Entity;

public sealed class GioHang
{
    public long GioHangID { get; init; }
    public long? TaiKhoanID { get; init; }
    public byte[]? MaPhienHash { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
    public DateTime? HetHanLuc { get; init; }
    private List<ChiTietGioHang> chiTiet = [];

    // Nhận các dòng hiện có của giỏ trước khi áp dụng thay đổi.
    public void NapChiTiet(List<ChiTietGioHang> cacDong) => chiTiet = cacDong;

    // Tìm dòng theo biến thể, tạo nếu chưa có rồi đặt số lượng cuối cùng.
    public ChiTietGioHang DatSoLuong(long bienTheId,int soLuong)
    {
        var dong = chiTiet.Find(x => x.BienTheID == bienTheId);
        if (dong is null) {
            dong = new ChiTietGioHang { GioHangID = GioHangID, BienTheID = bienTheId };
            dong.CapNhatSoLuong(soLuong);
            chiTiet.Add(dong);
        } else dong.CapNhatSoLuong(soLuong);
        return dong;
    }

    // Xóa dòng của biến thể và báo có cần lưu thao tác xóa hay không.
    public bool XoaSanPham(long bienTheId) => chiTiet.RemoveAll(x => x.BienTheID == bienTheId) > 0;
}
