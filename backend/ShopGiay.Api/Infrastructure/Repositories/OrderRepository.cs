using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public sealed class OrderRepository
{
    // Khóa bản ghi đơn và ánh xạ dữ liệu MySQL thành Entity.
    public Task<DonHang?> TaiDeCapNhat(SqlSession db,long id) =>
        db.One<DonHang>(OrderQueries.LockOrder,id);
    // Ghi trạng thái và thông tin hủy từ Entity trong giao dịch của Control.
    public Task LuuHuyDon(SqlSession db,DonHang donHang) =>
        db.Execute(OrderQueries.CancelOrder,donHang.TrangThai,donHang.NgayHuy,
            donHang.NguoiHuyID,donHang.LyDoHuy,donHang.DonHangID);

    // Tạo đơn với các khoản tiền đã chốt và thời hạn thu dựa trên đồng hồ MySQL.
    public Task<long> ThemDon(SqlSession db,DonHang don,int? phutChoThu) =>
        db.Insert("""
            INSERT INTO DonHang(MaDonHang,MaYeuCauTao,KhachHangID,HoTenNguoiNhan,SoDienThoaiNhan,DiaChiNhan,GhiChu,
                PhuongThucID,TienHang,TienGiam,PhiVanChuyen,HanThanhToan)
            VALUES(?,?,?,?,?,?,?,?,?,?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL ? MINUTE))
            """,don.MaDonHang,don.MaYeuCauTao,don.KhachHangID,don.HoTenNguoiNhan,don.SoDienThoaiNhan,don.DiaChiNhan,
            don.GhiChu,don.PhuongThucID,don.TienHang,don.TienGiam,don.PhiVanChuyen,phutChoThu);

    // Lưu thông tin, số lượng và giá tại thời điểm mua để đối soát về sau.
    public Task<long> ThemDong(SqlSession db,ChiTietDonHang line) =>
        db.Insert("""
            INSERT INTO ChiTietDonHang(DonHangID,BienTheID,MaSanPhamLucMua,SKULucMua,TenSanPhamLucMua,KichCoLucMua,
                MauSacLucMua,DanhMucLucMua,ThuongHieuLucMua,SoLuong,DonGia,TienGiamPhanBo)
            VALUES(?,?,?,?,?,?,?,?,?,?,?,?)
            """,line.DonHangID,line.BienTheID,line.MaSanPhamLucMua,line.SKULucMua,line.TenSanPhamLucMua,
            line.KichCoLucMua,line.MauSacLucMua,line.DanhMucLucMua,line.ThuongHieuLucMua,line.SoLuong,line.DonGia,line.TienGiamPhanBo);

    // Tạo bản ghi giữ tồn cho dòng đơn mới trong giao dịch đặt hàng.
    public Task ThemPhanBo(SqlSession db,PhanBoTonKho phanBo) =>
        db.Execute("INSERT INTO PhanBoTonKho(ChiTietDonHangID,SoLuongGiu,TrangThai) VALUES(?,?,?)",
            phanBo.ChiTietDonHangID,phanBo.SoLuongGiu,phanBo.TrangThai);

    // Khóa các dòng đang giữ theo thứ tự biến thể để hủy hoặc xuất kho.
    public Task<List<PhanBoTonKho>> KhoaPhanBoDangGiu(SqlSession db,long id) =>
        db.Rows<PhanBoTonKho>(OrderQueries.Allocations,id);

    // Lưu trạng thái kết thúc của dòng giữ hàng đã xử lý.
    public Task LuuPhanBo(SqlSession db,PhanBoTonKho phanBo) =>
        db.Execute("UPDATE PhanBoTonKho SET TrangThai=?,NgayKetThuc=? WHERE ChiTietDonHangID=?",
            phanBo.TrangThai,phanBo.NgayKetThuc,phanBo.ChiTietDonHangID);

    // Lưu bước xử lý và các mốc giao, hoàn tất từ Entity.
    public Task LuuTienDo(SqlSession db,DonHang don) =>
        db.Execute("UPDATE DonHang SET TrangThai=?,NgayGiao=?,NgayHoanThanh=?,PhienBan=PhienBan+1 WHERE DonHangID=?",
            don.TrangThai,don.NgayGiao,don.NgayHoanThanh,don.DonHangID);

    // Lưu trạng thái vận chuyển và chỉ thay thông tin theo dõi khi được gửi kèm.
    public Task LuuVanChuyen(SqlSession db,DonHang don,string? donVi,string? maVanDon) =>
        db.Execute(OrderQueries.UpdateDelivery,don.TrangThaiGiaoHang,donVi,maVanDon,don.DonHangID);

    // Lưu thông tin đơn bị hủy vì đã nhận lại kiện giao thất bại.
    public Task LuuHoanGiao(SqlSession db,DonHang don) =>
        db.Execute("""
            UPDATE DonHang SET TrangThai=?,TrangThaiGiaoHang=?,NgayHuy=?,NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1
            WHERE DonHangID=?
            """,don.TrangThai,don.TrangThaiGiaoHang,don.NgayHuy,don.NguoiHuyID,don.LyDoHuy,don.DonHangID);

    // Tạo khoản chờ thu theo tổng tiền của đơn mới.
    public Task ThemThanhToan(SqlSession db,ThanhToan pay) =>
        db.Execute("INSERT INTO ThanhToan(DonHangID,PhuongThucID,MaYeuCau,SoTien) VALUES(?,?,?,?)",
            pay.DonHangID,pay.PhuongThucID,pay.MaYeuCau,pay.SoTien);

    // Khóa đơn cùng mã phương thức để đối chiếu điều kiện COD hoặc chuyển khoản.
    public Task<DonHang?> KhoaDeThuTien(SqlSession db,long id) =>
        db.One<DonHang>(OrderQueries.LockOrderWithMethod,id);

    // Khóa đơn đã hủy cùng phương thức để kiểm tra tiền về sau thời hạn.
    public Task<DonHang?> KhoaDeDoiSoatTre(SqlSession db,long id) =>
        db.One<DonHang>(OrderQueries.LockLateOrder,id);

    // Khóa khoản chờ thu theo mã giao dịch.
    public Task<ThanhToan?> KhoaThanhToan(SqlSession db,long id) =>
        db.One<ThanhToan>(OrderQueries.LockPayment,id);

    // Khóa khoản thu gần nhất của đơn để đối soát tiền đến muộn.
    public Task<ThanhToan?> KhoaThanhToanCuoi(SqlSession db,long id) =>
        db.One<ThanhToan>(OrderQueries.LockLatestPayment,id);

    // Lưu trạng thái thu, số tiền và bằng chứng đã ghi trên Entity.
    public Task LuuXacNhanThu(SqlSession db,ThanhToan pay) =>
        db.Execute("""
            UPDATE ThanhToan SET TrangThai=?,SoTien=?,NgayXacNhan=?,NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?
            """,pay.TrangThai,pay.SoTien,pay.NgayXacNhan,pay.NguoiXacNhanID,pay.BangChung,pay.ThanhToanID);
}
