using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Return; Service gọi lớp này trong cùng giao dịch.
public sealed class ReturnRepository(Database db) : IReturnRepository
{
    // ReviewableLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ReviewableLine(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT c.DonHangID FROM ChiTietDonHang c JOIN DonHang d ON d.DonHangID=c.DonHangID WHERE c.ChiTietDonHangID=? AND d.KhachHangID=? AND d.TrangThai='HOAN_THANH'", values);
    }

    // InsertReview: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> InsertReview(object? [] values)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO DanhGia(DonHangID,ChiTietDonHangID,KhachHangID,SoSao,NoiDung) VALUES(?,?,?,?,?)", values);
    }

    // ForCustomer: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ForCustomer(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT YeuCauDoiTraID id,MaYeuCau code,LoaiYeuCau type,LyDo reason,TrangThai status,NgayTao date FROM YeuCauDoiTra WHERE KhachHangID=? ORDER BY YeuCauDoiTraID DESC", values);
    }

    // LockCustomer: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockCustomer(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE", values, connection);
    }

    // FindByKey: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> FindByKey(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT YeuCauDoiTraID id,KhachHangID owner FROM YeuCauDoiTra WHERE MaChongLap=?", values, connection);
    }

    // LockEligibleLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockEligibleLine(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ct.*,d.KhachHangID,d.NgayGiao FROM ChiTietDonHang ct JOIN DonHang d ON d.DonHangID=ct.DonHangID WHERE ct.ChiTietDonHangID=? AND d.KhachHangID=? AND d.TrangThai IN ('DA_GIAO','HOAN_THANH') FOR UPDATE", values, connection);
    }

    // RequestedQuantity: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RequestedQuantity(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COALESCE(SUM(ct.SoLuongYeuCau),0) n FROM ChiTietDoiTra ct JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=ct.YeuCauDoiTraID WHERE ct.ChiTietDonHangID=? AND y.TrangThai NOT IN ('TU_CHOI','DA_HUY')", values, connection);
    }

    // CreateRequest: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreateRequest(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO YeuCauDoiTra(MaYeuCau,MaChongLap,DonHangID,KhachHangID,LoaiYeuCau,LyDo) VALUES(?,?,?,?,?,?)", values, connection);
    }

    // AddLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AddLine(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO ChiTietDoiTra(YeuCauDoiTraID,DonHangID,ChiTietDonHangID,SoLuongYeuCau) VALUES(?,?,?,?)", values, connection);
    }

    // AttachImage: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AttachImage(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO HinhAnhDoiTra(YeuCauDoiTraID,DuongDan) VALUES(?,?)", values, connection);
    }

    // RecordSubmission: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordSubmission(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO LichSuDoiTra(YeuCauDoiTraID,TrangThaiMoi,NguoiThucHienID,NoiDung) VALUES(?,'CHO_DUYET',?,'Khách gửi yêu cầu')", values, connection);
    }

    // OwnedRequest: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> OwnedRequest(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT YeuCauDoiTraID id,MaYeuCau code,TrangThai status,LyDo reason,LyDoXuLy decision,DonHangID orderId FROM YeuCauDoiTra WHERE YeuCauDoiTraID=? AND KhachHangID=?", values);
    }

    // Lines: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Lines(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ct.TenSanPhamLucMua name,c.SoLuongYeuCau quantity,c.SoLuongDuyet approved,c.SoLuongDaNhan received,c.SoLuongBanLai sellable,c.TienHoanDuocDuyet refund FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID WHERE c.YeuCauDoiTraID=?", values);
    }

    // History: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> History(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TrangThaiMoi status,ThoiDiem date,NoiDung note FROM LichSuDoiTra WHERE YeuCauDoiTraID=? ORDER BY LichSuID", values);
    }

    // Refunds: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Refunds(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT SoTien amount,TrangThai status,NgayXacNhan confirmedAt FROM HoanTien WHERE YeuCauDoiTraID=?", values);
    }

    // All: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> All()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT y.YeuCauDoiTraID id,y.MaYeuCau code,y.LoaiYeuCau type,y.LyDo reason,y.TrangThai status,y.NgayTao date,t.HoTen name,ct.TenSanPhamLucMua product,c.SoLuongYeuCau quantity,c.SoLuongBanLai restocked,c.TienHoanDuocDuyet refund,(SELECT DuongDan FROM HinhAnhDoiTra h WHERE h.YeuCauDoiTraID=y.YeuCauDoiTraID LIMIT 1) image FROM YeuCauDoiTra y JOIN TaiKhoan t ON t.TaiKhoanID=y.KhachHangID JOIN ChiTietDoiTra c ON c.YeuCauDoiTraID=y.YeuCauDoiTraID JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID ORDER BY y.YeuCauDoiTraID DESC");
    }

    // LockRequest: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockRequest(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM YeuCauDoiTra WHERE YeuCauDoiTraID=? FOR UPDATE", values, connection);
    }

    // LockLines: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockLines(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT c.*,ct.BienTheID,ct.SoLuong bought,ct.ThanhTien paid FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID WHERE c.YeuCauDoiTraID=? FOR UPDATE", values, connection);
    }

    // ApproveLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ApproveLine(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE ChiTietDoiTra SET SoLuongDuyet=SoLuongYeuCau,TienHoanDuocDuyet=? WHERE ChiTietDoiTraID=?", values, connection);
    }

    // RecordDecision: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordDecision(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE YeuCauDoiTra SET NguoiDuyetID=?,NgayDuyet=UTC_TIMESTAMP(),LyDoXuLy=? WHERE YeuCauDoiTraID=?", values, connection);
    }

    // ReceiveLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ReceiveLine(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE ChiTietDoiTra SET SoLuongDaNhan=SoLuongDuyet,SoLuongBanLai=?,NguoiNhanHangID=?,NgayNhanHang=UTC_TIMESTAMP(),TinhTrangHang=? WHERE ChiTietDoiTraID=?", values, connection);
    }

    // CreateReceipt: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreateReceipt(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID) VALUES(?,?,'NHAN_TRA',?,?)", values, connection);
    }

    // AddReceiptLine: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AddReceiptLine(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,ChiTietDoiTraID) VALUES(?,?,?,?)", values, connection);
    }

    // PendingRefunds: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> PendingRefunds(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai<>'DA_XAC_NHAN'", values, connection);
    }

    // ApprovedTotal: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ApprovedTotal(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COALESCE(SUM(TienHoanDuocDuyet),0) owed FROM ChiTietDoiTra WHERE YeuCauDoiTraID=?", values, connection);
    }

    // ConfirmedRefunds: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ConfirmedRefunds(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COALESCE(SUM(SoTien),0) refunded FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai='DA_XAC_NHAN'", values, connection);
    }

    // MarkComplete: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> MarkComplete(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE YeuCauDoiTra SET NgayHoanTat=UTC_TIMESTAMP() WHERE YeuCauDoiTraID=?", values, connection);
    }

    // SetStatus: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> SetStatus(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE YeuCauDoiTra SET TrangThai=? WHERE YeuCauDoiTraID=?", values, connection);
    }

    // RecordHistory: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordHistory(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO LichSuDoiTra(YeuCauDoiTraID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,NoiDung) VALUES(?,?,?,?,?)", values, connection);
    }

    // AllRefunds: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AllRefunds()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT h.HoanTienID id,d.MaDonHang code,h.SoTien amount,h.TrangThai status,h.LyDo reason,h.NgayTao date FROM HoanTien h JOIN DonHang d ON d.DonHangID=h.DonHangID ORDER BY h.HoanTienID DESC");
    }

    // LockRefund: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockRefund(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM HoanTien WHERE HoanTienID=? FOR UPDATE", values, connection);
    }

    // LockPayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockPayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM ThanhToan WHERE ThanhToanID=? FOR UPDATE", values, connection);
    }

    // RefundedForPayment: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RefundedForPayment(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COALESCE(SUM(SoTien),0) returned FROM HoanTien WHERE ThanhToanID=? AND TrangThai='DA_XAC_NHAN'", values, connection);
    }

    // ConfirmRefund: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ConfirmRefund(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE HoanTien SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXuLyID=?,BangChung=? WHERE HoanTienID=?", values, connection);
    }

    // RetryRefund: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RetryRefund(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE HoanTien SET TrangThai='CHO_XU_LY' WHERE HoanTienID=?", values, connection);
    }

    // FailRefund: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> FailRefund(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE HoanTien SET TrangThai='THAT_BAI' WHERE HoanTienID=?", values, connection);
    }
}
