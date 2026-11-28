using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public sealed class ReturnRepository
{
    // Tạo hồ sơ yêu cầu với mã chống lặp và trạng thái ban đầu của Entity.
    public Task<long> Them(SqlSession db,YeuCauDoiTra request) =>
        db.Insert(ReturnQueries.CreateRequest,request.MaYeuCau,request.MaChongLap,request.DonHangID,
            request.KhachHangID,request.LoaiYeuCau,request.LyDo);

    // Khóa yêu cầu để các bước duyệt, nhận và hoàn tất không chồng nhau.
    public Task<YeuCauDoiTra?> Khoa(SqlSession db,long id) =>
        db.One<YeuCauDoiTra>(ReturnQueries.LockRequest,id);

    // Đọc khóa các dòng trả cùng giá và mô tả chụp từ đơn gốc.
    public Task<List<ChiTietDoiTra>> KhoaDong(SqlSession db,long id) =>
        db.Rows<ChiTietDoiTra>(ReturnQueries.LockLines,id);

    // Lưu trạng thái, quyết định xét duyệt, đơn đổi và mốc hoàn tất.
    public Task Luu(SqlSession db,YeuCauDoiTra request) =>
        db.Execute("""
            UPDATE YeuCauDoiTra SET TrangThai=?,NguoiDuyetID=?,NgayDuyet=?,LyDoXuLy=?,NgayHoanTat=?,DonHangDoiMoiID=?
            WHERE YeuCauDoiTraID=?
            """,request.TrangThai,request.NguoiDuyetID,request.NgayDuyet,request.LyDoXuLy,request.NgayHoanTat,
            request.DonHangDoiMoiID,request.YeuCauDoiTraID);

    // Lưu số lượng và tiền đã duyệt của dòng đổi trả.
    public Task LuuDuyet(SqlSession db,ChiTietDoiTra line) =>
        db.Execute("UPDATE ChiTietDoiTra SET SoLuongDuyet=?,TienHoanDuocDuyet=? WHERE ChiTietDoiTraID=?",
            line.SoLuongDuyet,line.TienHoanDuocDuyet,line.ChiTietDoiTraID);

    // Lưu lượng nhận, lượng bán lại, tình trạng và thông tin người nhận.
    public Task LuuNhanHang(SqlSession db,ChiTietDoiTra line) =>
        db.Execute("""
            UPDATE ChiTietDoiTra SET SoLuongDaNhan=?,SoLuongBanLai=?,NguoiNhanHangID=?,NgayNhanHang=?,TinhTrangHang=?
            WHERE ChiTietDoiTraID=?
            """,line.SoLuongDaNhan,line.SoLuongBanLai,line.NguoiNhanHangID,line.NgayNhanHang,line.TinhTrangHang,line.ChiTietDoiTraID);

    // Khóa khoản hoàn để đối soát và chuyển trạng thái.
    public Task<HoanTien?> KhoaKhoanHoan(SqlSession db,long id) =>
        db.One<HoanTien>(ReturnQueries.LockRefund,id);

    // Lưu trạng thái khoản hoàn và thông tin xác nhận nếu đã hoàn tiền.
    public Task LuuKhoanHoan(SqlSession db,HoanTien refund) =>
        db.Execute("UPDATE HoanTien SET TrangThai=?,NgayXacNhan=?,NguoiXuLyID=?,BangChung=? WHERE HoanTienID=?",
            refund.TrangThai,refund.NgayXacNhan,refund.NguoiXuLyID,refund.BangChung,refund.HoanTienID);

    // Lưu đánh giá của khách cho đúng dòng đơn đã hoàn thành.
    public Task ThemDanhGia(SqlSession db,DanhGia danhGia) =>
        db.Execute(ReturnQueries.InsertReview,danhGia.DonHangID,danhGia.ChiTietDonHangID,danhGia.KhachHangID,danhGia.SoSao,danhGia.NoiDung);
}
