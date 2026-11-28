using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Report; Service gọi lớp này trong cùng giao dịch.
public sealed class ReportRepository(Database db) : IReportRepository
{
    // Customers: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Customers()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.SoDienThoai phone,COUNT(d.DonHangID) orders,COALESCE(SUM(d.TongThanhToan),0) total FROM TaiKhoan t JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID AND v.MaVaiTro='CUSTOMER' LEFT JOIN DonHang d ON d.KhachHangID=t.TaiKhoanID WHERE t.NgayXoa IS NULL GROUP BY t.TaiKhoanID ORDER BY t.TaiKhoanID");
    }

    // Income: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Income(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COALESCE(SUM(SoTien),0) value FROM ThanhToan WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<?", values);
    }

    // Refunded: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Refunded(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COALESCE(SUM(SoTien),0) value FROM HoanTien WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<?", values);
    }

    // OrderCounts: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> OrderCounts(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TrangThai status,COUNT(*) count FROM DonHang WHERE NgayDat>=? AND NgayDat<? GROUP BY TrangThai", values);
    }

    // DailyIncome: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> DailyIncome(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT DATE(CONVERT_TZ(NgayXacNhan,'+00:00','+07:00')) day,SUM(SoTien) amount FROM ThanhToan WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<? GROUP BY DATE(CONVERT_TZ(NgayXacNhan,'+00:00','+07:00'))", values);
    }

    // BestSellers: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> BestSellers(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ct.TenSanPhamLucMua name,SUM(ct.SoLuong) quantity,SUM(ct.ThanhTien) amount FROM ChiTietDonHang ct JOIN DonHang d ON d.DonHangID=ct.DonHangID WHERE d.TrangThai='HOAN_THANH' AND d.NgayHoanThanh>=? AND d.NgayHoanThanh<? GROUP BY ct.TenSanPhamLucMua ORDER BY quantity DESC LIMIT 5", values);
    }

    // LowStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LowStock()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n FROM TonKho WHERE SoLuongKhaDung<=MucCanhBao");
    }

    // Logs: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Logs()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT n.NhatKyID id,t.HoTen name,n.HanhDong action,n.DoiTuong object,n.MaDoiTuong objectId,n.NoiDung content,n.KetQua result,n.ThoiDiem date FROM NhatKyHeThong n LEFT JOIN TaiKhoan t ON t.TaiKhoanID=n.NguoiThucHienID ORDER BY n.NhatKyID DESC LIMIT 200");
    }

    // DatabaseInfo: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> DatabaseInfo()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT DATABASE() name,VERSION() version");
    }

    // TableCount: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> TableCount()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n FROM information_schema.tables WHERE table_schema=DATABASE()");
    }

    // ActiveSessions: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ActiveSessions()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n FROM PhienDangNhap WHERE ThuHoiLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP()");
    }

    // AccountCount: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AccountCount()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n FROM TaiKhoan WHERE NgayXoa IS NULL");
    }
}
