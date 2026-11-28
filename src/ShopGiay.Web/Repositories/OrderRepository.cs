using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Order; Service gọi lớp này trong cùng giao dịch.
public sealed class OrderRepository(Database db) : IOrderRepository
{
    private const string ordersSql = $@"SELECT d.DonHangID id,d.MaDonHang code,d.KhachHangID customerId,d.HoTenNguoiNhan name,d.SoDienThoaiNhan phone,d.DiaChiNhan address,d.GhiChu note,d.TrangThai status,d.TongThanhToan total,d.TienHang subtotal,d.TienGiam discount,d.PhiVanChuyen shipping,d.NgayDat date,d.HanThanhToan paymentDeadline,d.DonViVanChuyen carrier,d.MaVanDon tracking,d.TrangThaiGiaoHang deliveryStatus,p.MaPhuongThuc method,
 COALESCE((SELECT SUM(SoTien) FROM HoanTien h WHERE h.DonHangID=d.DonHangID AND h.TrangThai='DA_XAC_NHAN'),0) refunded,COALESCE((SELECT SUM(SoTien) FROM ThanhToan t WHERE t.DonHangID=d.DonHangID AND t.TrangThai='DA_XAC_NHAN'),0) paid FROM DonHang d JOIN PhuongThucThanhToan p ON p.PhuongThucID=d.PhuongThucID";
    // FindById: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> FindById(object? [] values)
    {
        // Database.Query → thực thi câu lệnh trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(ordersSql + @" WHERE d.DonHangID=?", values);
    }

    // Items: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Items(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ChiTietDonHangID id,BienTheID variantId,TenSanPhamLucMua name,KichCoLucMua size,MauSacLucMua color,SoLuong quantity,DonGia price,ThanhTien total FROM ChiTietDonHang WHERE DonHangID=?", values);
    }

    // History: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> History(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TrangThaiMoi status,ThoiDiem date,GhiChu note FROM LichSuDonHang WHERE DonHangID=? ORDER BY LichSuID", values);
    }

    // Refunds: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Refunds(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT HoanTienID id,SoTien amount,TrangThai status,LyDo reason,NgayXacNhan confirmedAt FROM HoanTien WHERE DonHangID=?", values);
    }

    // Payments: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Payments(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ThanhToanID id,SoTien amount,TrangThai status,BangChung proof,NgayXacNhan confirmedAt FROM ThanhToan WHERE DonHangID=?", values);
    }

    // LockOrder: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockOrder(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM DonHang WHERE DonHangID=? FOR UPDATE", values, connection);
    }

    // Allocations: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Allocations(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT p.*,ct.BienTheID FROM PhanBoTonKho p JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=p.ChiTietDonHangID WHERE ct.DonHangID=? AND p.TrangThai='DANG_GIU' ORDER BY ct.BienTheID FOR UPDATE", values, connection);
    }

    // ReleaseAllocation: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ReleaseAllocation(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE PhanBoTonKho SET TrangThai='DA_GIAI_PHONG',NgayKetThuc=UTC_TIMESTAMP() WHERE ChiTietDonHangID=?", values, connection);
    }

    // CancelOrder: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CancelOrder(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET TrangThai='DA_HUY',NgayHuy=UTC_TIMESTAMP(),NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1 WHERE DonHangID=?", values, connection);
    }

    // RecordCancellation: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordCancellation(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,'DA_HUY',?,?)", values, connection);
    }

    // CancelPendingPayments: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CancelPendingPayments(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE ThanhToan SET TrangThai='DA_HUY' WHERE DonHangID=? AND TrangThai='CHO_XAC_NHAN'", values, connection);
    }

    // ExpiredOrders: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ExpiredOrders()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT DonHangID id FROM DonHang WHERE HanThanhToan<=UTC_TIMESTAMP() AND TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_CHUAN_BI') ORDER BY DonHangID LIMIT 100");
    }

    // ConfirmedTotal: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ConfirmedTotal(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COALESCE(SUM(SoTien),0) paid FROM ThanhToan WHERE DonHangID=? AND TrangThai='DA_XAC_NHAN'", values, connection);
    }

    // ExpirePayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ExpirePayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE ThanhToan SET TrangThai='HET_HAN' WHERE DonHangID=? AND TrangThai='DA_HUY'", values, connection);
    }

    // ForCustomer: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ForCustomer(object? [] values)
    {
        // Database.Query → thực thi câu lệnh trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(ordersSql + @" WHERE d.KhachHangID=? ORDER BY d.DonHangID DESC", values);
    }

    // All: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> All()
    {
        // Database.Query → thực thi câu lệnh trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(ordersSql + @" ORDER BY d.DonHangID DESC");
    }

    // PaymentMethod: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> PaymentMethod(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT MaPhuongThuc code FROM PhuongThucThanhToan WHERE PhuongThucID=?", values, connection);
    }

    // MarkShipping: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> MarkShipping(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET TrangThaiGiaoHang='DANG_GIAO' WHERE DonHangID=?", values, connection);
    }

    // MarkDelivered: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> MarkDelivered(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET TrangThaiGiaoHang='DA_GIAO' WHERE DonHangID=?", values, connection);
    }

    // HeldItems: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> HeldItems(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ct.*,p.SoLuongGiu FROM ChiTietDonHang ct JOIN PhanBoTonKho p ON p.ChiTietDonHangID=ct.ChiTietDonHangID WHERE ct.DonHangID=? AND p.TrangThai='DANG_GIU' ORDER BY ct.BienTheID FOR UPDATE", values, connection);
    }

    // ReleaseShipped: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ReleaseShipped(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE PhanBoTonKho SET TrangThai='DA_XUAT',NgayKetThuc=UTC_TIMESTAMP() WHERE ChiTietDonHangID=?", values, connection);
    }

    // UpdateStatus: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateStatus(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET TrangThai=?,NgayGiao=IF(?='DA_GIAO',UTC_TIMESTAMP(),NgayGiao),NgayHoanThanh=IF(?='HOAN_THANH',UTC_TIMESTAMP(),NgayHoanThanh),PhienBan=PhienBan+1 WHERE DonHangID=?", values, connection);
    }

    // AppendHistory: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AppendHistory(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,?,?)", values, connection);
    }

    // OrderForPayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> OrderForPayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT DonHangID FROM ThanhToan WHERE ThanhToanID=?", values, connection);
    }

    // LockOrderWithMethod: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockOrderWithMethod(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT d.*,m.MaPhuongThuc FROM DonHang d JOIN PhuongThucThanhToan m ON m.PhuongThucID=d.PhuongThucID WHERE d.DonHangID=? FOR UPDATE", values, connection);
    }

    // LockPayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockPayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM ThanhToan WHERE ThanhToanID=? FOR UPDATE", values, connection);
    }

    // ConfirmPayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ConfirmPayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE ThanhToan SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?", values, connection);
    }

    // ClearDeadline: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ClearDeadline(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET HanThanhToan=NULL WHERE DonHangID=?", values, connection);
    }

    // UpdateDelivery: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateDelivery(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET TrangThaiGiaoHang=?,DonViVanChuyen=COALESCE(?,DonViVanChuyen),MaVanDon=COALESCE(?,MaVanDon),PhienBan=PhienBan+1 WHERE DonHangID=?", values, connection);
    }

    // RecordDelivery: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordDelivery(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,?,?)", values, connection);
    }

    // LockItems: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockItems(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM ChiTietDonHang WHERE DonHangID=? ORDER BY BienTheID", values, connection);
    }

    // CreateReturnReceipt: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreateReturnReceipt(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID,TrangThai,NgayXacNhan,NguoiXacNhanID,DonHangID) VALUES(?,?,'NHAN_TRA',?,?,'DA_XAC_NHAN',UTC_TIMESTAMP(),?,?)", values, connection);
    }

    // AddReceiptLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AddReceiptLine(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,GhiChu) VALUES(?,?,?,?)", values, connection);
    }

    // CancelReturnedOrder: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CancelReturnedOrder(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DonHang SET TrangThai='DA_HUY',TrangThaiGiaoHang='DA_NHAN_HOAN',NgayHuy=UTC_TIMESTAMP(),NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1 WHERE DonHangID=?", values, connection);
    }

    // RecordReturn: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordReturn(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,'DANG_GIAO','DA_HUY',?,?)", values, connection);
    }

    // LockLateOrder: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockLateOrder(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT d.*,p.MaPhuongThuc method FROM DonHang d JOIN PhuongThucThanhToan p ON p.PhuongThucID=d.PhuongThucID WHERE d.DonHangID=? FOR UPDATE", values, connection);
    }

    // LockLatestPayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockLatestPayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM ThanhToan WHERE DonHangID=? ORDER BY ThanhToanID LIMIT 1 FOR UPDATE", values, connection);
    }

    // ConfirmLatePayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ConfirmLatePayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE ThanhToan SET TrangThai='DA_XAC_NHAN',SoTien=?,NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?", values, connection);
    }
}
