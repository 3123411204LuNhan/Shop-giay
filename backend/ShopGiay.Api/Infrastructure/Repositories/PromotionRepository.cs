using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public sealed class PromotionRepository
{
    // Khóa chương trình theo ID để sửa lịch, đổi trạng thái hoặc xóa.
    public Task<KhuyenMai?> Khoa(SqlSession db,long id) =>
        db.One<KhuyenMai>("SELECT * FROM KhuyenMai WHERE KhuyenMaiID=? FOR UPDATE",id);

    // Đọc và khóa mã ưu đãi để kiểm tra điều kiện và lượt dùng trong giao dịch đặt đơn.
    public Task<KhuyenMai?> KhoaTheoMa(SqlSession db,string code) =>
        db.One<KhuyenMai>("SELECT * FROM KhuyenMai WHERE MaKhuyenMai=? FOR UPDATE",code);

    // Tạo chương trình cùng thông tin giảm, phạm vi và lịch đã thiết lập.
    public Task<long> Them(SqlSession db,KhuyenMai promo) =>
        db.Insert("""
            INSERT INTO KhuyenMai(MaKhuyenMai,TenKhuyenMai,HinhThuc,GiaTri,GiaTriDonToiThieu,GiamToiDa,PhamVi,BatDauLuc,KetThucLuc,NguoiTaoID,TrangThai)
            VALUES(?,?,?,?,?,?,?,?,?,?,?)
            """,promo.MaKhuyenMai,promo.TenKhuyenMai,promo.HinhThuc,promo.GiaTri,promo.GiaTriDonToiThieu,promo.GiamToiDa,
            promo.PhamVi,promo.BatDauLuc,promo.KetThucLuc,promo.NguoiTaoID,promo.TrangThai);

    // Lưu lịch và điều kiện từ Entity trong cùng giao dịch với lịch sử thay đổi.
    public Task Luu(SqlSession db,KhuyenMai promo) =>
        db.Execute("""
            UPDATE KhuyenMai SET TenKhuyenMai=?,HinhThuc=?,GiaTri=?,GiaTriDonToiThieu=?,GiamToiDa=?,
              TrangThai=?,BatDauLuc=?,KetThucLuc=?,NgayCapNhat=UTC_TIMESTAMP() WHERE KhuyenMaiID=?
            """,promo.TenKhuyenMai,promo.HinhThuc,promo.GiaTri,promo.GiaTriDonToiThieu,promo.GiamToiDa,promo.TrangThai,
            promo.BatDauLuc,promo.KetThucLuc,promo.KhuyenMaiID);
}
