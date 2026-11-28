using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Inventory; Service gọi lớp này trong cùng giao dịch.
public sealed class InventoryRepository(Database db) : IInventoryRepository
{
    // Receipts: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Receipts()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"SELECT p.PhieuKhoID id,p.MaPhieu code,p.LoaiPhieu type,p.TrangThai status,p.NhaCungCap supplier,p.MaChungTuThamChieu reference,p.LyDo reason,p.NgayTao date,p.NgayXacNhan confirmedAt,t.HoTen creator,COUNT(c.ChiTietPhieuKhoID) lineCount,COALESCE(SUM(c.SoLuongThayDoi*COALESCE(c.DonGiaNhap,0)),0) cost FROM PhieuKho p JOIN TaiKhoan t ON t.TaiKhoanID=p.NguoiThucHienID LEFT JOIN ChiTietPhieuKho c ON c.PhieuKhoID=p.PhieuKhoID GROUP BY p.PhieuKhoID ORDER BY p.PhieuKhoID DESC LIMIT 500");
    }

    // Receipt: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Receipt(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT PhieuKhoID id,MaPhieu code,LoaiPhieu type,TrangThai status,NhaCungCap supplier,MaChungTuThamChieu reference,LyDo reason,NgayTao date,NgayXacNhan confirmedAt FROM PhieuKho WHERE PhieuKhoID=?", values);
    }

    // Lines: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Lines(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT c.BienTheID variantId,c.SoLuongThayDoi quantity,c.DonGiaNhap cost,b.SKU sku,s.TenSanPham name,k.GiaTri size,m.TenMau color FROM ChiTietPhieuKho c JOIN BienTheSanPham b ON b.BienTheID=c.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID WHERE c.PhieuKhoID=?", values);
    }

    // LockActor: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockActor(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE", values, connection);
    }

    // FindByKey: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> FindByKey(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT PhieuKhoID id,NguoiThucHienID owner FROM PhieuKho WHERE MaChongLap=?", values, connection);
    }

    // LockReceipt: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockReceipt(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM PhieuKho WHERE PhieuKhoID=? FOR UPDATE", values, connection);
    }

    // LockLines: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockLines(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM ChiTietPhieuKho WHERE PhieuKhoID=? ORDER BY BienTheID", values, connection);
    }

    // ConfirmReceipt: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ConfirmReceipt(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE PhieuKho SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=? WHERE PhieuKhoID=?", values, connection);
    }

    // CancelReceipt: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CancelReceipt(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE PhieuKho SET TrangThai='DA_HUY' WHERE PhieuKhoID=?", values, connection);
    }

    // SetThreshold: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> SetThreshold(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE TonKho SET MucCanhBao=? WHERE BienTheID=?", values, connection);
    }

    // History: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> History(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"SELECT b.BienDongID id,b.LoaiBienDong type,b.ThayDoiTon quantity,b.ThayDoiPhanBo reservedChange,b.TonSau stock,b.PhanBoSau reserved,b.LyDo reason,b.ThoiDiem date,v.SKU sku,t.HoTen actor FROM BienDongKho b JOIN BienTheSanPham v ON v.BienTheID=b.BienTheID LEFT JOIN TaiKhoan t ON t.TaiKhoanID=b.NguoiThucHienID {(values.Length > 0 ? "WHERE b.BienTheID=?" : "")} ORDER BY b.BienDongID DESC LIMIT 300", values);
    }
}
