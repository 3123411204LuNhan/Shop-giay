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
      'SELECT YeuCauDoiTraID id,KhachHangID owner FROM YeuCauDoiTra WHERE MaChongLap=?',
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
      'INSERT INTO ChiTietDoiTra(YeuCauDoiTraID,DonHangID,ChiTietDonHangID,SoLuongYeuCau) VALUES(?,?,?,?)',
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

  ownedRequest(values) {
    return query(
      'SELECT YeuCauDoiTraID id,MaYeuCau code,TrangThai status,LyDo reason,LyDoXuLy decision,DonHangID orderId FROM YeuCauDoiTra WHERE YeuCauDoiTraID=? AND KhachHangID=?',
      values,
    );
  }

  lines(values) {
    return query(
      'SELECT ct.TenSanPhamLucMua name,c.SoLuongYeuCau quantity,c.SoLuongDuyet approved,c.SoLuongDaNhan received,c.SoLuongBanLai sellable,c.TienHoanDuocDuyet refund FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID WHERE c.YeuCauDoiTraID=?',
      values,
    );
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
      'SELECT c.*,ct.BienTheID,ct.SoLuong bought,ct.ThanhTien paid FROM ChiTietDoiTra c JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=c.ChiTietDonHangID WHERE c.YeuCauDoiTraID=? FOR UPDATE',
      values,
      connection,
    );
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
      "INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID) VALUES(?,?,'NHAN_TRA',?,?)",
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
