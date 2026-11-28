namespace ShopGiay.Infrastructure;

internal static class ReturnQueries
{
    internal const string ReviewableLine = """
        SELECT c.DonHangID FROM ChiTietDonHang c JOIN DonHang d ON d.DonHangID=c.DonHangID WHERE c.ChiTietDonHangID=? AND d.KhachHangID=? AND d.TrangThai='HOAN_THANH'
        """;

    internal const string InsertReview = """
        INSERT INTO DanhGia(DonHangID,ChiTietDonHangID,KhachHangID,SoSao,NoiDung) VALUES(?,?,?,?,?)
        """;

    internal const string ForCustomer = """
        SELECT YeuCauDoiTraID id,MaYeuCau code,LoaiYeuCau type,LyDo reason,TrangThai status,NgayTao date FROM YeuCauDoiTra WHERE KhachHangID=? ORDER BY YeuCauDoiTraID DESC
        """;

    internal const string LockCustomer = """
        SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE
        """;

    internal const string FindByKey = """
        SELECT YeuCauDoiTraID id,KhachHangID owner,MaYeuCau code,TrangThai status FROM YeuCauDoiTra WHERE MaChongLap=?
        """;

    internal const string LockEligibleLine = """
        SELECT ct.*,d.KhachHangID,d.NgayGiao FROM ChiTietDonHang ct JOIN DonHang d ON d.DonHangID=ct.DonHangID WHERE ct.ChiTietDonHangID=? AND d.KhachHangID=? AND d.TrangThai IN ('DA_GIAO','HOAN_THANH') FOR UPDATE
        """;

    internal const string ExchangeOptions = """
        SELECT b.BienTheID id,b.SKU sku,k.GiaTri size,m.TenMau color,t.SoLuongKhaDung stock
                FROM BienTheSanPham old JOIN BienTheSanPham b ON b.SanPhamID=old.SanPhamID
                JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN TonKho t ON t.BienTheID=b.BienTheID
                JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu h ON h.ThuongHieuID=s.ThuongHieuID
                JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID
                WHERE old.BienTheID=? AND b.BienTheID<>old.BienTheID AND b.DangKinhDoanh=1
                AND s.TrangThai='DANG_BAN' AND d.DangHoatDong=1 AND h.DangHoatDong=1
                AND COALESCE(b.GiaBanRieng,s.GiaBan)=?
                {slot0}
        """;

    internal const string RequestedQuantity = """
        SELECT COALESCE(SUM(ct.SoLuongYeuCau),0) n FROM ChiTietDoiTra ct JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=ct.YeuCauDoiTraID WHERE ct.ChiTietDonHangID=? AND y.TrangThai NOT IN ('TU_CHOI','DA_HUY')
        """;

    internal const string CreateRequest = """
        INSERT INTO YeuCauDoiTra(MaYeuCau,MaChongLap,DonHangID,KhachHangID,LoaiYeuCau,LyDo) VALUES(?,?,?,?,?,?)
        """;

    internal const string AddLine = """
        INSERT INTO ChiTietDoiTra(YeuCauDoiTraID,DonHangID,ChiTietDonHangID,SoLuongYeuCau,BienTheDoiSangID) VALUES(?,?,?,?,?)
        """;

    internal const string AttachImage = """
        INSERT INTO HinhAnhDoiTra(YeuCauDoiTraID,DuongDan) VALUES(?,?)
        """;

    internal const string RecordSubmission = """
        INSERT INTO LichSuDoiTra(YeuCauDoiTraID,TrangThaiMoi,NguoiThucHienID,NoiDung) VALUES(?,'CHO_DUYET',?,'Khách gửi yêu cầu')
        """;

    internal const string RequestDetails = """
        SELECT y.YeuCauDoiTraID id,y.MaYeuCau code,y.LoaiYeuCau type,y.TrangThai status,y.LyDo reason,
                y.LyDoXuLy decision,d.DonHangID orderId,d.MaDonHang orderCode,d2.MaDonHang replacementCode,
                a.HoTen customerName
                FROM YeuCauDoiTra y JOIN DonHang d ON d.DonHangID=y.DonHangID
                JOIN TaiKhoan a ON a.TaiKhoanID=y.KhachHangID
                LEFT JOIN DonHang d2 ON d2.DonHangID=y.DonHangDoiMoiID
                WHERE y.YeuCauDoiTraID=?{slot0}
        """;

    internal const string Lines = """
        SELECT c.ChiTietDoiTraID id,ct.TenSanPhamLucMua name,ct.KichCoLucMua size,ct.MauSacLucMua color,
                c.SoLuongYeuCau quantity,c.SoLuongDuyet approved,c.SoLuongDaNhan received,c.SoLuongBanLai sellable,
                c.TienHoanDuocDuyet refund,ks.GiaTri targetSize,ms.TenMau targetColor,k.SoLuongKhaDung targetStock
                FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID
                LEFT JOIN BienTheSanPham b ON b.BienTheID=c.BienTheDoiSangID
                LEFT JOIN KichCo ks ON ks.KichCoID=b.KichCoID LEFT JOIN MauSac ms ON ms.MauSacID=b.MauSacID
                LEFT JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE c.YeuCauDoiTraID=?
        """;

    internal const string RequestImage = """
        SELECT 1 FROM HinhAnhDoiTra h JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=h.YeuCauDoiTraID WHERE y.YeuCauDoiTraID=? AND h.DuongDan=?
        """;

    internal const string Images = """
        SELECT DuongDan url FROM HinhAnhDoiTra WHERE YeuCauDoiTraID=? ORDER BY HinhAnhID
        """;

    internal const string History = """
        SELECT TrangThaiMoi status,ThoiDiem date,NoiDung note FROM LichSuDoiTra WHERE YeuCauDoiTraID=? ORDER BY LichSuID
        """;

    internal const string Refunds = """
        SELECT SoTien amount,TrangThai status,NgayXacNhan confirmedAt FROM HoanTien WHERE YeuCauDoiTraID=?
        """;

    internal const string All = """
        SELECT y.YeuCauDoiTraID id,y.MaYeuCau code,y.LoaiYeuCau type,y.LyDo reason,y.TrangThai status,y.NgayTao date,t.HoTen name,ct.TenSanPhamLucMua product,c.SoLuongYeuCau quantity,c.SoLuongBanLai restocked,c.TienHoanDuocDuyet refund,(SELECT DuongDan FROM HinhAnhDoiTra h WHERE h.YeuCauDoiTraID=y.YeuCauDoiTraID LIMIT 1) image FROM YeuCauDoiTra y JOIN TaiKhoan t ON t.TaiKhoanID=y.KhachHangID JOIN ChiTietDoiTra c ON c.YeuCauDoiTraID=y.YeuCauDoiTraID JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID ORDER BY y.YeuCauDoiTraID DESC
        """;

    internal const string LockRequest = """
        SELECT * FROM YeuCauDoiTra WHERE YeuCauDoiTraID=? FOR UPDATE
        """;

    internal const string LockLines = """
        SELECT c.*,ct.BienTheID,ct.MaSanPhamLucMua,ct.TenSanPhamLucMua,ct.DanhMucLucMua,ct.ThuongHieuLucMua,ct.DonGia,ct.SoLuong SoLuongLucMua,ct.ThanhTien ThanhTienLucMua FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID WHERE c.YeuCauDoiTraID=? FOR UPDATE
        """;

    internal const string ApprovedReturnShares = """
        SELECT COALESCE(SUM(c.SoLuongDuyet),0) quantity,COALESCE(SUM(c.TienHoanDuocDuyet),0) amount
              FROM ChiTietDoiTra c JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=c.YeuCauDoiTraID
              WHERE c.ChiTietDonHangID=? AND c.YeuCauDoiTraID<>?
              AND y.LoaiYeuCau='TRA' AND y.TrangThai NOT IN ('DA_HUY','TU_CHOI')
        """;

    internal const string CreateReceipt = """
        INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID,TrangThai,NgayXacNhan,NguoiXacNhanID) VALUES(?,?,'NHAN_TRA',?,?,'DA_XAC_NHAN',UTC_TIMESTAMP(),?)
        """;

    internal const string AddReceiptLine = """
        INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,ChiTietDoiTraID) VALUES(?,?,?,?)
        """;

    internal const string PendingRefunds = """
        SELECT COUNT(*) n FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai<>'DA_XAC_NHAN'
        """;

    internal const string ApprovedTotal = """
        SELECT COALESCE(SUM(TienHoanDuocDuyet),0) owed FROM ChiTietDoiTra WHERE YeuCauDoiTraID=?
        """;

    internal const string ConfirmedRefunds = """
        SELECT COALESCE(SUM(SoTien),0) refunded FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai='DA_XAC_NHAN'
        """;

    internal const string RecordHistory = """
        INSERT INTO LichSuDoiTra(YeuCauDoiTraID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,NoiDung) VALUES(?,?,?,?,?)
        """;

    internal const string AllRefunds = """
        SELECT h.HoanTienID id,d.MaDonHang code,h.SoTien amount,h.TrangThai status,h.LyDo reason,h.NgayTao date FROM HoanTien h JOIN DonHang d ON d.DonHangID=h.DonHangID ORDER BY h.HoanTienID DESC
        """;

    internal const string LockRefund = """
        SELECT * FROM HoanTien WHERE HoanTienID=? FOR UPDATE
        """;

    internal const string RefundedForPayment = """
        SELECT COALESCE(SUM(SoTien),0) returned FROM HoanTien WHERE ThanhToanID=? AND TrangThai='DA_XAC_NHAN'
        """;

}
