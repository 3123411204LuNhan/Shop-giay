import { query } from '../db.js';

// ReturnService dùng các truy vấn này; connection giữ các bước ghi trong cùng giao dịch.
export class ReturnRepository {
  reviewableLine(values) {
    return query(
      "SELECT c.DonHangID FROM ChiTietDonHang c JOIN DonHang d ON d.DonHangID=c.DonHangID WHERE c.ChiTietDonHangID=? AND d.KhachHangID=? AND d.TrangThai='HOAN_THANH'",
      values,
    );
  }

  insertReview(values) {
    return query(
      'INSERT INTO DanhGia(DonHangID,ChiTietDonHangID,KhachHangID,SoSao,NoiDung) VALUES(?,?,?,?,?)',
      values,
    );
  }

  forCustomer(values) {
    return query(
      'SELECT YeuCauDoiTraID id,MaYeuCau code,LoaiYeuCau type,LyDo reason,TrangThai status,NgayTao date FROM YeuCauDoiTra WHERE KhachHangID=? ORDER BY YeuCauDoiTraID DESC',
      values,
    );
  }

  lockCustomer(values, connection) {
    return query('SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE', values, connection);
  }

  findByKey(values, connection) {
    return query(
      'SELECT YeuCauDoiTraID id,KhachHangID owner,MaYeuCau code,TrangThai status FROM YeuCauDoiTra WHERE MaChongLap=?',
      values,
      connection,
    );
  }

  lockEligibleLine(values, connection) {
    return query(
      "SELECT ct.*,d.KhachHangID,d.NgayGiao FROM ChiTietDonHang ct JOIN DonHang d ON d.DonHangID=ct.DonHangID WHERE ct.ChiTietDonHangID=? AND d.KhachHangID=? AND d.TrangThai IN ('DA_GIAO','HOAN_THANH') FOR UPDATE",
      values,
      connection,
    );
  }

  exchangeOptions(originalId,price,targetId,connection) {
    return query(
      `SELECT b.BienTheID id,b.SKU sku,k.GiaTri size,m.TenMau color,t.SoLuongKhaDung stock
        FROM BienTheSanPham old JOIN BienTheSanPham b ON b.SanPhamID=old.SanPhamID
        JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN TonKho t ON t.BienTheID=b.BienTheID
        JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu h ON h.ThuongHieuID=s.ThuongHieuID
        JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID
        WHERE old.BienTheID=? AND b.BienTheID<>old.BienTheID AND b.DangKinhDoanh=1
        AND s.TrangThai='DANG_BAN' AND d.DangHoatDong=1 AND h.DangHoatDong=1
        AND COALESCE(b.GiaBanRieng,s.GiaBan)=?
        ${targetId==null?'AND t.SoLuongKhaDung>0 ORDER BY k.ThuTu,b.BienTheID':'AND b.BienTheID=? FOR UPDATE'}`,
      targetId==null?[originalId,price]:[originalId,price,targetId],connection,
    );
  }

  linkReplacement(values,connection) {
    return query('UPDATE YeuCauDoiTra SET DonHangDoiMoiID=? WHERE YeuCauDoiTraID=?',values,connection);
  }

  requestedQuantity(values, connection) {
    return query(
      "SELECT COALESCE(SUM(ct.SoLuongYeuCau),0) n FROM ChiTietDoiTra ct JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=ct.YeuCauDoiTraID WHERE ct.ChiTietDonHangID=? AND y.TrangThai NOT IN ('TU_CHOI','DA_HUY')",
      values,
      connection,
    );
  }

  createRequest(values, connection) {
    return query(
      'INSERT INTO YeuCauDoiTra(MaYeuCau,MaChongLap,DonHangID,KhachHangID,LoaiYeuCau,LyDo) VALUES(?,?,?,?,?,?)',
      values,
      connection,
    );
  }

  addLine(values, connection) {
    return query(
      'INSERT INTO ChiTietDoiTra(YeuCauDoiTraID,DonHangID,ChiTietDonHangID,SoLuongYeuCau,BienTheDoiSangID) VALUES(?,?,?,?,?)',
      values,
      connection,
    );
  }

  attachImage(values, connection) {
    return query('INSERT INTO HinhAnhDoiTra(YeuCauDoiTraID,DuongDan) VALUES(?,?)', values, connection);
  }

  recordSubmission(values, connection) {
    return query(
      "INSERT INTO LichSuDoiTra(YeuCauDoiTraID,TrangThaiMoi,NguoiThucHienID,NoiDung) VALUES(?,'CHO_DUYET',?,'Khách gửi yêu cầu')",
      values,
      connection,
    );
  }

  requestDetails(id, customerId) {
    return query(
      `SELECT y.YeuCauDoiTraID id,y.MaYeuCau code,y.LoaiYeuCau type,y.TrangThai status,y.LyDo reason,
        y.LyDoXuLy decision,d.DonHangID orderId,d.MaDonHang orderCode,d2.MaDonHang replacementCode,
        a.HoTen customerName
        FROM YeuCauDoiTra y JOIN DonHang d ON d.DonHangID=y.DonHangID
        JOIN TaiKhoan a ON a.TaiKhoanID=y.KhachHangID
        LEFT JOIN DonHang d2 ON d2.DonHangID=y.DonHangDoiMoiID
        WHERE y.YeuCauDoiTraID=?${customerId==null?'':' AND y.KhachHangID=?'}`,
      customerId==null?[id]:[id,customerId],
    );
  }

  lines(values) {
    return query(
      `SELECT c.ChiTietDoiTraID id,ct.TenSanPhamLucMua name,ct.KichCoLucMua size,ct.MauSacLucMua color,
        c.SoLuongYeuCau quantity,c.SoLuongDuyet approved,c.SoLuongDaNhan received,c.SoLuongBanLai sellable,
        c.TienHoanDuocDuyet refund,ks.GiaTri targetSize,ms.TenMau targetColor,k.SoLuongKhaDung targetStock
        FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID
        LEFT JOIN BienTheSanPham b ON b.BienTheID=c.BienTheDoiSangID
        LEFT JOIN KichCo ks ON ks.KichCoID=b.KichCoID LEFT JOIN MauSac ms ON ms.MauSacID=b.MauSacID
        LEFT JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE c.YeuCauDoiTraID=?`,
      values,
    );
  }

  requestImage(id,url,owner) {
    return query(
      'SELECT 1 FROM HinhAnhDoiTra h JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=h.YeuCauDoiTraID WHERE y.YeuCauDoiTraID=? AND h.DuongDan=?'+(owner==null?'':' AND y.KhachHangID=?'),
      owner==null?[id,url]:[id,url,owner],
    );
  }

  images(values) {
    return query('SELECT DuongDan url FROM HinhAnhDoiTra WHERE YeuCauDoiTraID=? ORDER BY HinhAnhID',values);
  }

  history(values) {
    return query(
      'SELECT TrangThaiMoi status,ThoiDiem date,NoiDung note FROM LichSuDoiTra WHERE YeuCauDoiTraID=? ORDER BY LichSuID',
      values,
    );
  }

  refunds(values) {
    return query(
      'SELECT SoTien amount,TrangThai status,NgayXacNhan confirmedAt FROM HoanTien WHERE YeuCauDoiTraID=?',
      values,
    );
  }

  all() {
    return query(
      'SELECT y.YeuCauDoiTraID id,y.MaYeuCau code,y.LoaiYeuCau type,y.LyDo reason,y.TrangThai status,y.NgayTao date,t.HoTen name,ct.TenSanPhamLucMua product,c.SoLuongYeuCau quantity,c.SoLuongBanLai restocked,c.TienHoanDuocDuyet refund,(SELECT DuongDan FROM HinhAnhDoiTra h WHERE h.YeuCauDoiTraID=y.YeuCauDoiTraID LIMIT 1) image FROM YeuCauDoiTra y JOIN TaiKhoan t ON t.TaiKhoanID=y.KhachHangID JOIN ChiTietDoiTra c ON c.YeuCauDoiTraID=y.YeuCauDoiTraID JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID ORDER BY y.YeuCauDoiTraID DESC',
    );
  }

  lockRequest(values, connection) {
    return query('SELECT * FROM YeuCauDoiTra WHERE YeuCauDoiTraID=? FOR UPDATE', values, connection);
  }

  lockLines(values, connection) {
    return query(
      'SELECT c.*,ct.BienTheID,ct.MaSanPhamLucMua,ct.TenSanPhamLucMua,ct.DanhMucLucMua,ct.ThuongHieuLucMua,ct.DonGia,ct.SoLuong bought,ct.ThanhTien paid FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID WHERE c.YeuCauDoiTraID=? FOR UPDATE',
      values,
      connection,
    );
  }

  approvedReturnShares(values,connection) {
    return query(`SELECT COALESCE(SUM(c.SoLuongDuyet),0) quantity,COALESCE(SUM(c.TienHoanDuocDuyet),0) amount
      FROM ChiTietDoiTra c JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=c.YeuCauDoiTraID
      WHERE c.ChiTietDonHangID=? AND c.YeuCauDoiTraID<>?
      AND y.LoaiYeuCau='TRA' AND y.TrangThai NOT IN ('DA_HUY','TU_CHOI')`,values,connection);
  }

  approveLine(values, connection) {
    return query(
      'UPDATE ChiTietDoiTra SET SoLuongDuyet=SoLuongYeuCau,TienHoanDuocDuyet=? WHERE ChiTietDoiTraID=?',
      values,
      connection,
    );
  }

  recordDecision(values, connection) {
    return query(
      'UPDATE YeuCauDoiTra SET NguoiDuyetID=?,NgayDuyet=UTC_TIMESTAMP(),LyDoXuLy=? WHERE YeuCauDoiTraID=?',
      values,
      connection,
    );
  }

  receiveLine(values, connection) {
    return query(
      'UPDATE ChiTietDoiTra SET SoLuongDaNhan=SoLuongDuyet,SoLuongBanLai=?,NguoiNhanHangID=?,NgayNhanHang=UTC_TIMESTAMP(),TinhTrangHang=? WHERE ChiTietDoiTraID=?',
      values,
      connection,
    );
  }

  createReceipt(values, connection) {
    return query(
      "INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID,TrangThai,NgayXacNhan,NguoiXacNhanID) VALUES(?,?,'NHAN_TRA',?,?,'DA_XAC_NHAN',UTC_TIMESTAMP(),?)",
      values,
      connection,
    );
  }

  addReceiptLine(values, connection) {
    return query(
      'INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,ChiTietDoiTraID) VALUES(?,?,?,?)',
      values,
      connection,
    );
  }

  pendingRefunds(values, connection) {
    return query(
      "SELECT COUNT(*) n FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai<>'DA_XAC_NHAN'",
      values,
      connection,
    );
  }

  approvedTotal(values, connection) {
    return query(
      'SELECT COALESCE(SUM(TienHoanDuocDuyet),0) owed FROM ChiTietDoiTra WHERE YeuCauDoiTraID=?',
      values,
      connection,
    );
  }

  confirmedRefunds(values, connection) {
    return query(
      "SELECT COALESCE(SUM(SoTien),0) refunded FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai='DA_XAC_NHAN'",
      values,
      connection,
    );
  }

  markComplete(values, connection) {
    return query(
      'UPDATE YeuCauDoiTra SET NgayHoanTat=UTC_TIMESTAMP() WHERE YeuCauDoiTraID=?',
      values,
      connection,
    );
  }

  setStatus(values, connection) {
    return query('UPDATE YeuCauDoiTra SET TrangThai=? WHERE YeuCauDoiTraID=?', values, connection);
  }

  recordHistory(values, connection) {
    return query(
      'INSERT INTO LichSuDoiTra(YeuCauDoiTraID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,NoiDung) VALUES(?,?,?,?,?)',
      values,
      connection,
    );
  }

  allRefunds() {
    return query(
      'SELECT h.HoanTienID id,d.MaDonHang code,h.SoTien amount,h.TrangThai status,h.LyDo reason,h.NgayTao date FROM HoanTien h JOIN DonHang d ON d.DonHangID=h.DonHangID ORDER BY h.HoanTienID DESC',
    );
  }

  lockRefund(values, connection) {
    return query('SELECT * FROM HoanTien WHERE HoanTienID=? FOR UPDATE', values, connection);
  }

  lockPayment(values, connection) {
    return query('SELECT * FROM ThanhToan WHERE ThanhToanID=? FOR UPDATE', values, connection);
  }

  refundedForPayment(values, connection) {
    return query(
      "SELECT COALESCE(SUM(SoTien),0) returned FROM HoanTien WHERE ThanhToanID=? AND TrangThai='DA_XAC_NHAN'",
      values,
      connection,
    );
  }

  confirmRefund(values, connection) {
    return query(
      "UPDATE HoanTien SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXuLyID=?,BangChung=? WHERE HoanTienID=?",
      values,
      connection,
    );
  }

  retryRefund(values, connection) {
    return query("UPDATE HoanTien SET TrangThai='CHO_XU_LY' WHERE HoanTienID=?", values, connection);
  }

  failRefund(values, connection) {
    return query("UPDATE HoanTien SET TrangThai='THAT_BAI' WHERE HoanTienID=?", values, connection);
  }
}
