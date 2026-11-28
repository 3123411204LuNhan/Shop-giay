using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Checkout; Service gọi lớp này trong cùng giao dịch.
public sealed class CheckoutRepository(Database db) : ICheckoutRepository
{
    // EnsureCart: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> EnsureCart(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO GioHang(TaiKhoanID) VALUES(?) ON DUPLICATE KEY UPDATE TaiKhoanID=VALUES(TaiKhoanID)", values, connection);
    }

    // CartId: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CartId(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT GioHangID id FROM GioHang WHERE TaiKhoanID=?", values, connection);
    }

    // CheckoutItems: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CheckoutItems(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"SELECT ct.*,b.SKU,b.SanPhamID,b.DangKinhDoanh,COALESCE(b.GiaBanRieng,s.GiaBan) price,s.MaSanPham,s.TenSanPham,s.TrangThai,s.DanhMucID,s.ThuongHieuID,d.TenDanhMuc,t.TenThuongHieu,kc.GiaTri size,m.TenMau color FROM ChiTietGioHang ct JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID WHERE ct.GioHangID=? AND d.DangHoatDong=1 AND t.DangHoatDong=1 ORDER BY b.BienTheID", values, connection);
    }

    // CountItems: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CountItems(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n FROM ChiTietGioHang WHERE GioHangID=?", values, connection);
    }

    // LockStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockStock(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT SoLuongKhaDung stock FROM TonKho WHERE BienTheID=? FOR UPDATE", values, connection);
    }

    // LockPromotion: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockPromotion(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM KhuyenMai WHERE MaKhuyenMai=? AND TrangThai='DANG_AP_DUNG' AND BatDauLuc<=UTC_TIMESTAMP() AND KetThucLuc>UTC_TIMESTAMP() FOR UPDATE", values, connection);
    }

    // PromotionUsage: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> PromotionUsage(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n,SUM(d.KhachHangID=?) own FROM KhuyenMaiDonHang k JOIN DonHang d ON d.DonHangID=k.DonHangID WHERE k.KhuyenMaiID=? AND d.TrangThai<>'DA_HUY'", values, connection);
    }

    // EligibleIds: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> EligibleIds(string[] map, object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"SELECT {map[1]} id FROM {map[0]} WHERE KhuyenMaiID=?", values, connection);
    }

    // PaymentMethod: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> PaymentMethod(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT PhuongThucID id FROM PhuongThucThanhToan WHERE MaPhuongThuc=? AND DangHoatDong=1", values, connection);
    }

    // CartItems: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CartItems(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"SELECT ct.BienTheID variantId,ct.SoLuong quantity,s.TenSanPham name,COALESCE(b.GiaBanRieng,s.GiaBan) price,kc.GiaTri size,m.TenMau color,k.SoLuongKhaDung stock,
     (SELECT DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1) image FROM ChiTietGioHang ct JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE ct.GioHangID=?", values);
    }

    // RemoveItem: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RemoveItem(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE FROM ChiTietGioHang WHERE GioHangID=? AND BienTheID=?", values, connection);
    }

    // AvailableVariant: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AvailableVariant(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT k.SoLuongKhaDung stock FROM TonKho k JOIN BienTheSanPham b ON b.BienTheID=k.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID WHERE b.BienTheID=? AND b.DangKinhDoanh=1 AND s.TrangThai='DANG_BAN'", values, connection);
    }

    // SetQuantity: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> SetQuantity(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO ChiTietGioHang(GioHangID,BienTheID,SoLuong) VALUES(?,?,?) ON DUPLICATE KEY UPDATE SoLuong=VALUES(SoLuong),NgayCapNhat=UTC_TIMESTAMP()", values, connection);
    }

    // LockCustomer: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockCustomer(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE", values, connection);
    }

    // FindOrderByKey: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> FindOrderByKey(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT DonHangID id,KhachHangID owner FROM DonHang WHERE MaYeuCauTao=?", values, connection);
    }

    // OrderTotal: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> OrderTotal(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TongThanhToan total FROM DonHang WHERE DonHangID=?", values, connection);
    }

    // CreateOrder: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreateOrder(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO DonHang(MaDonHang,MaYeuCauTao,KhachHangID,HoTenNguoiNhan,SoDienThoaiNhan,DiaChiNhan,GhiChu,PhuongThucID,TienHang,TienGiam,PhiVanChuyen) VALUES(?,?,?,?,?,?,?,?,?,?,?)", values, connection);
    }

    // SetDeadline: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> SetDeadline(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET HanThanhToan=DATE_ADD(UTC_TIMESTAMP(),INTERVAL ? MINUTE) WHERE DonHangID=?", values, connection);
    }

    // AddOrderLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AddOrderLine(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO ChiTietDonHang(DonHangID,BienTheID,MaSanPhamLucMua,SKULucMua,TenSanPhamLucMua,KichCoLucMua,MauSacLucMua,DanhMucLucMua,ThuongHieuLucMua,SoLuong,DonGia,TienGiamPhanBo) VALUES(?,?,?,?,?,?,?,?,?,?,?,?)", values, connection);
    }

    // AllocateStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AllocateStock(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO PhanBoTonKho(ChiTietDonHangID,SoLuongGiu) VALUES(?,?)", values, connection);
    }

    // RecordPromotion: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordPromotion(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO KhuyenMaiDonHang(DonHangID,KhuyenMaiID,MaLucApDung,TenLucApDung,HinhThucLucApDung,GiaTriLucApDung,TienGiamThucTe) VALUES(?,?,?,?,?,?,?)", values, connection);
    }

    // RecordHistory: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordHistory(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO LichSuDonHang(DonHangID,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,'CHO_XAC_NHAN',?,'Khách tạo đơn')", values, connection);
    }

    // CreatePayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreatePayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO ThanhToan(DonHangID,PhuongThucID,MaYeuCau,SoTien) VALUES(?,?,?,?)", values, connection);
    }

    // EmptyCart: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> EmptyCart(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE FROM ChiTietGioHang WHERE GioHangID=?", values, connection);
    }
}
