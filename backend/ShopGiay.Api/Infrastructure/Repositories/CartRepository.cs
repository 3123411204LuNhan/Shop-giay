using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public sealed class CartRepository
{
    // Dùng khóa duy nhất theo tài khoản để tạo hoặc lấy giỏ trong giao dịch hiện tại.
    public async Task<GioHang> LayHoacTao(SqlSession db,long account)
    {
        await db.Execute("INSERT INTO GioHang(TaiKhoanID) VALUES(?) ON DUPLICATE KEY UPDATE TaiKhoanID=VALUES(TaiKhoanID)",account);
        return await db.One<GioHang>("SELECT * FROM GioHang WHERE TaiKhoanID=? FOR UPDATE",account)
            ?? throw new InvalidOperationException("Không đọc được giỏ của tài khoản.");
    }

    // Đọc dữ liệu hiển thị, gồm giá hiện tại và ảnh ưu tiên cho từng biến thể.
    public Task<List<dynamic>> DocHienThi(SqlSession db,long id) =>
        db.Rows("""
            SELECT ct.BienTheID variantId,ct.SoLuong quantity,s.TenSanPham name,COALESCE(b.GiaBanRieng,s.GiaBan) price,
                   kc.GiaTri size,m.TenMau color,k.SoLuongKhaDung stock,
                   (SELECT h.DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID
                    ORDER BY h.LaAnhChinh DESC,h.ThuTu,h.HinhAnhID LIMIT 1) image
            FROM ChiTietGioHang ct JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID
            JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo kc ON kc.KichCoID=b.KichCoID
            JOIN MauSac m ON m.MauSacID=b.MauSacID JOIN TonKho k ON k.BienTheID=b.BienTheID
            WHERE ct.GioHangID=?
            """,id);

    // Đọc số dư của biến thể đang bán để Entity kiểm tra số lượng yêu cầu.
    public Task<TonKho?> TonDangBan(SqlSession db,long variant) =>
        db.One<TonKho>("""
            SELECT k.SoLuongTon ton,k.SoLuongDaPhanBo daPhanBo FROM TonKho k
            JOIN BienTheSanPham b ON b.BienTheID=k.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID
            WHERE b.BienTheID=? AND b.DangKinhDoanh=1 AND s.TrangThai='DANG_BAN'
            """,variant);

    // Đọc các dòng thuộc đúng giỏ rồi nạp chúng vào đối tượng giỏ.
    public async Task NapChiTiet(SqlSession db,GioHang gioHang) =>
        gioHang.NapChiTiet(await db.Rows<ChiTietGioHang>("SELECT * FROM ChiTietGioHang WHERE GioHangID=? FOR UPDATE",gioHang.GioHangID));

    // Lưu dòng mới hoặc đặt lại số lượng của dòng đang có theo khóa giỏ và biến thể.
    public Task LuuDong(SqlSession db,ChiTietGioHang dong) =>
        db.Execute("""
            INSERT INTO ChiTietGioHang(GioHangID,BienTheID,SoLuong) VALUES(?,?,?)
            ON DUPLICATE KEY UPDATE SoLuong=VALUES(SoLuong),NgayCapNhat=UTC_TIMESTAMP()
            """,dong.GioHangID,dong.BienTheID,dong.SoLuong);

    // Xóa dòng đã được đối tượng giỏ loại khỏi danh sách.
    public Task XoaDong(SqlSession db,long cart,long variant) =>
        db.Execute("DELETE FROM ChiTietGioHang WHERE GioHangID=? AND BienTheID=?",cart,variant);

    // Dọn các dòng của giỏ sau khi tạo đơn thành công trong cùng giao dịch.
    public Task LamTrong(SqlSession db,long cart) =>
        db.Execute("DELETE FROM ChiTietGioHang WHERE GioHangID=?",cart);
}
