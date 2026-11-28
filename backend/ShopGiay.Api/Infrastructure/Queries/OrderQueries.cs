namespace ShopGiay.Infrastructure;

internal static class OrderQueries
{
    private const string ordersSql = """
        SELECT d.DonHangID id,d.MaDonHang code,d.KhachHangID customerId,d.HoTenNguoiNhan name,d.SoDienThoaiNhan phone,d.DiaChiNhan address,d.GhiChu note,d.TrangThai status,d.TongThanhToan total,d.TienHang subtotal,d.TienGiam discount,d.PhiVanChuyen shipping,d.NgayDat date,d.HanThanhToan paymentDeadline,d.DonViVanChuyen carrier,d.MaVanDon tracking,d.TrangThaiGiaoHang deliveryStatus,p.MaPhuongThuc method,
         COALESCE((SELECT SUM(SoTien) FROM HoanTien h WHERE h.DonHangID=d.DonHangID AND h.TrangThai='DA_XAC_NHAN'),0) refunded,COALESCE((SELECT SUM(SoTien) FROM ThanhToan t WHERE t.DonHangID=d.DonHangID AND t.TrangThai='DA_XAC_NHAN'),0) paid FROM DonHang d JOIN PhuongThucThanhToan p ON p.PhuongThucID=d.PhuongThucID
        """;

    internal const string FindById = ordersSql + """
         WHERE d.DonHangID=?
        """;

    internal const string Items = """
        SELECT ChiTietDonHangID id,BienTheID variantId,TenSanPhamLucMua name,KichCoLucMua size,MauSacLucMua color,SoLuong quantity,DonGia price,ThanhTien total FROM ChiTietDonHang WHERE DonHangID=?
        """;

    internal const string History = """
        SELECT TrangThaiMoi status,ThoiDiem date,GhiChu note FROM LichSuDonHang WHERE DonHangID=? ORDER BY LichSuID
        """;

    internal const string Refunds = """
        SELECT HoanTienID id,SoTien amount,TrangThai status,LyDo reason,NgayXacNhan confirmedAt FROM HoanTien WHERE DonHangID=?
        """;

    internal const string Payments = """
        SELECT ThanhToanID id,SoTien amount,TrangThai status,BangChung proof,NgayXacNhan confirmedAt FROM ThanhToan WHERE DonHangID=?
        """;

    internal const string LockOrder = """
        SELECT * FROM DonHang WHERE DonHangID=? FOR UPDATE
        """;

    internal const string Allocations = """
        SELECT p.*,ct.BienTheID FROM PhanBoTonKho p JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=p.ChiTietDonHangID WHERE ct.DonHangID=? AND p.TrangThai='DANG_GIU' ORDER BY ct.BienTheID FOR UPDATE
        """;

    internal const string ReleaseAllocation = """
        UPDATE PhanBoTonKho SET TrangThai='DA_GIAI_PHONG',NgayKetThuc=UTC_TIMESTAMP() WHERE ChiTietDonHangID=?
        """;

    internal const string CancelOrder = """
        UPDATE DonHang SET TrangThai='DA_HUY',NgayHuy=UTC_TIMESTAMP(),NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1 WHERE DonHangID=?
        """;

    internal const string RecordCancellation = """
        INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,'DA_HUY',?,?)
        """;

    internal const string CancelPendingPayments = """
        UPDATE ThanhToan SET TrangThai='DA_HUY' WHERE DonHangID=? AND TrangThai='CHO_XAC_NHAN'
        """;

    internal const string ExpiredOrders = """
        SELECT DonHangID id FROM DonHang WHERE HanThanhToan<=UTC_TIMESTAMP() AND TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_CHUAN_BI') ORDER BY DonHangID LIMIT 100
        """;

    internal const string ConfirmedTotal = """
        SELECT COALESCE(SUM(SoTien),0) paid FROM ThanhToan WHERE DonHangID=? AND TrangThai='DA_XAC_NHAN'
        """;

    internal const string ExpirePayment = """
        UPDATE ThanhToan SET TrangThai='HET_HAN' WHERE DonHangID=? AND TrangThai='DA_HUY'
        """;

    internal const string ForCustomer = ordersSql + """
         WHERE d.KhachHangID=? ORDER BY d.DonHangID DESC
        """;

    internal const string All = ordersSql + """
         ORDER BY d.DonHangID DESC
        """;

    internal const string PaymentMethod = """
        SELECT MaPhuongThuc code FROM PhuongThucThanhToan WHERE PhuongThucID=?
        """;

    internal const string MarkShipping = """
        UPDATE DonHang SET TrangThaiGiaoHang='DANG_GIAO' WHERE DonHangID=?
        """;

    internal const string MarkDelivered = """
        UPDATE DonHang SET TrangThaiGiaoHang='DA_GIAO' WHERE DonHangID=?
        """;

    internal const string HeldItems = """
        SELECT ct.*,p.SoLuongGiu FROM ChiTietDonHang ct JOIN PhanBoTonKho p ON p.ChiTietDonHangID=ct.ChiTietDonHangID WHERE ct.DonHangID=? AND p.TrangThai='DANG_GIU' ORDER BY ct.BienTheID FOR UPDATE
        """;

    internal const string ReleaseShipped = """
        UPDATE PhanBoTonKho SET TrangThai='DA_XUAT',NgayKetThuc=UTC_TIMESTAMP() WHERE ChiTietDonHangID=?
        """;

    internal const string UpdateStatus = """
        UPDATE DonHang SET TrangThai=?,NgayGiao=IF(?='DA_GIAO',UTC_TIMESTAMP(),NgayGiao),NgayHoanThanh=IF(?='HOAN_THANH',UTC_TIMESTAMP(),NgayHoanThanh),PhienBan=PhienBan+1 WHERE DonHangID=?
        """;

    internal const string AppendHistory = """
        INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,?,?)
        """;

    internal const string OrderForPayment = """
        SELECT DonHangID FROM ThanhToan WHERE ThanhToanID=?
        """;

    internal const string LockOrderWithMethod = """
        SELECT d.*,m.MaPhuongThuc FROM DonHang d JOIN PhuongThucThanhToan m ON m.PhuongThucID=d.PhuongThucID WHERE d.DonHangID=? FOR UPDATE
        """;

    internal const string LockPayment = """
        SELECT * FROM ThanhToan WHERE ThanhToanID=? FOR UPDATE
        """;

    internal const string ConfirmPayment = """
        UPDATE ThanhToan SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?
        """;

    internal const string ClearDeadline = """
        UPDATE DonHang SET HanThanhToan=NULL WHERE DonHangID=?
        """;

    internal const string UpdateDelivery = """
        UPDATE DonHang SET TrangThaiGiaoHang=?,DonViVanChuyen=COALESCE(?,DonViVanChuyen),MaVanDon=COALESCE(?,MaVanDon),PhienBan=PhienBan+1 WHERE DonHangID=?
        """;

    internal const string RecordDelivery = """
        INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,?,?)
        """;

    internal const string LockItems = """
        SELECT * FROM ChiTietDonHang WHERE DonHangID=? ORDER BY BienTheID
        """;

    internal const string CreateReturnReceipt = """
        INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID,TrangThai,NgayXacNhan,NguoiXacNhanID,DonHangID) VALUES(?,?,'NHAN_TRA',?,?,'DA_XAC_NHAN',UTC_TIMESTAMP(),?,?)
        """;

    internal const string AddReceiptLine = """
        INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,GhiChu) VALUES(?,?,?,?)
        """;

    internal const string CancelReturnedOrder = """
        UPDATE DonHang SET TrangThai='DA_HUY',TrangThaiGiaoHang='DA_NHAN_HOAN',NgayHuy=UTC_TIMESTAMP(),NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1 WHERE DonHangID=?
        """;

    internal const string RecordReturn = """
        INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,'DANG_GIAO','DA_HUY',?,?)
        """;

    internal const string LockLateOrder = """
        SELECT d.*,p.MaPhuongThuc method FROM DonHang d JOIN PhuongThucThanhToan p ON p.PhuongThucID=d.PhuongThucID WHERE d.DonHangID=? FOR UPDATE
        """;

    internal const string LockLatestPayment = """
        SELECT * FROM ThanhToan WHERE DonHangID=? ORDER BY ThanhToanID LIMIT 1 FOR UPDATE
        """;

    internal const string ConfirmLatePayment = """
        UPDATE ThanhToan SET TrangThai='DA_XAC_NHAN',SoTien=?,NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?
        """;
}
