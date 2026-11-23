import { query } from '../db.js';

const ordersSql = `SELECT d.DonHangID id,d.MaDonHang code,d.KhachHangID customerId,d.HoTenNguoiNhan name,d.SoDienThoaiNhan phone,d.DiaChiNhan address,d.GhiChu note,d.TrangThai status,d.TongThanhToan total,d.TienHang subtotal,d.TienGiam discount,d.PhiVanChuyen shipping,d.NgayDat date,d.HanThanhToan paymentDeadline,d.DonViVanChuyen carrier,d.MaVanDon tracking,d.TrangThaiGiaoHang deliveryStatus,p.MaPhuongThuc method,
 COALESCE((SELECT SUM(SoTien) FROM HoanTien h WHERE h.DonHangID=d.DonHangID AND h.TrangThai='DA_XAC_NHAN'),0) refunded,COALESCE((SELECT SUM(SoTien) FROM ThanhToan t WHERE t.DonHangID=d.DonHangID AND t.TrangThai='DA_XAC_NHAN'),0) paid FROM DonHang d JOIN PhuongThucThanhToan p ON p.PhuongThucID=d.PhuongThucID`;
// OrderService dùng các truy vấn này; connection giữ các bước ghi trong cùng giao dịch.
export class OrderRepository {
  findById(values) {
    return query(ordersSql + ' WHERE d.DonHangID=?', values);
  }

  items(values) {
    return query(
      'SELECT ChiTietDonHangID id,BienTheID variantId,TenSanPhamLucMua name,KichCoLucMua size,MauSacLucMua color,SoLuong quantity,DonGia price,ThanhTien total FROM ChiTietDonHang WHERE DonHangID=?',
      values,
    );
  }

  history(values) {
    return query(
      'SELECT TrangThaiMoi status,ThoiDiem date,GhiChu note FROM LichSuDonHang WHERE DonHangID=? ORDER BY LichSuID',
      values,
    );
  }

  refunds(values) {
    return query(
      'SELECT HoanTienID id,SoTien amount,TrangThai status,LyDo reason,NgayXacNhan confirmedAt FROM HoanTien WHERE DonHangID=?',
      values,
    );
  }

  payments(values) {
    return query(
      'SELECT ThanhToanID id,SoTien amount,TrangThai status,BangChung proof,NgayXacNhan confirmedAt FROM ThanhToan WHERE DonHangID=?',
      values,
    );
  }

  lockOrder(values, connection) {
    return query('SELECT * FROM DonHang WHERE DonHangID=? FOR UPDATE', values, connection);
  }

  allocations(values, connection) {
    return query(
      "SELECT p.*,ct.BienTheID FROM PhanBoTonKho p JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=p.ChiTietDonHangID WHERE ct.DonHangID=? AND p.TrangThai='DANG_GIU' ORDER BY ct.BienTheID FOR UPDATE",
      values,
      connection,
    );
  }

  releaseAllocation(values, connection) {
    return query(
      "UPDATE PhanBoTonKho SET TrangThai='DA_GIAI_PHONG',NgayKetThuc=UTC_TIMESTAMP() WHERE ChiTietDonHangID=?",
      values,
      connection,
    );
  }

  cancelOrder(values, connection) {
    return query(
      "UPDATE DonHang SET TrangThai='DA_HUY',NgayHuy=UTC_TIMESTAMP(),NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1 WHERE DonHangID=?",
      values,
      connection,
    );
  }

  recordCancellation(values, connection) {
    return query(
      "INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,'DA_HUY',?,?)",
      values,
      connection,
    );
  }

  cancelPendingPayments(values, connection) {
    return query(
      "UPDATE ThanhToan SET TrangThai='DA_HUY' WHERE DonHangID=? AND TrangThai='CHO_XAC_NHAN'",
      values,
      connection,
    );
  }

  expiredOrders() {
    return query(
      "SELECT DonHangID id FROM DonHang WHERE HanThanhToan<=UTC_TIMESTAMP() AND TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_CHUAN_BI') ORDER BY DonHangID LIMIT 100",
    );
  }

  confirmedTotal(values, connection) {
    return query(
      "SELECT COALESCE(SUM(SoTien),0) paid FROM ThanhToan WHERE DonHangID=? AND TrangThai='DA_XAC_NHAN'",
      values,
      connection,
    );
  }

  expirePayment(values, connection) {
    return query(
      "UPDATE ThanhToan SET TrangThai='HET_HAN' WHERE DonHangID=? AND TrangThai='DA_HUY'",
      values,
      connection,
    );
  }

  forCustomer(values) {
    return query(ordersSql + ' WHERE d.KhachHangID=? ORDER BY d.DonHangID DESC', values);
  }

  all() {
    return query(ordersSql + ' ORDER BY d.DonHangID DESC');
  }

  paymentMethod(values, connection) {
    return query(
      'SELECT MaPhuongThuc code FROM PhuongThucThanhToan WHERE PhuongThucID=?',
      values,
      connection,
    );
  }

  markShipping(values, connection) {
    return query(
      "UPDATE DonHang SET TrangThaiGiaoHang='DANG_GIAO' WHERE DonHangID=?",
      values,
      connection,
    );
  }

  markDelivered(values, connection) {
    return query("UPDATE DonHang SET TrangThaiGiaoHang='DA_GIAO' WHERE DonHangID=?", values, connection);
  }

  heldItems(values, connection) {
    return query(
      "SELECT ct.*,p.SoLuongGiu FROM ChiTietDonHang ct JOIN PhanBoTonKho p ON p.ChiTietDonHangID=ct.ChiTietDonHangID WHERE ct.DonHangID=? AND p.TrangThai='DANG_GIU' ORDER BY ct.BienTheID FOR UPDATE",
      values,
      connection,
    );
  }

  releaseShipped(values, connection) {
    return query(
      "UPDATE PhanBoTonKho SET TrangThai='DA_XUAT',NgayKetThuc=UTC_TIMESTAMP() WHERE ChiTietDonHangID=?",
      values,
      connection,
    );
  }

  updateStatus(values, connection) {
    return query(
      "UPDATE DonHang SET TrangThai=?,NgayGiao=IF(?='DA_GIAO',UTC_TIMESTAMP(),NgayGiao),NgayHoanThanh=IF(?='HOAN_THANH',UTC_TIMESTAMP(),NgayHoanThanh),PhienBan=PhienBan+1 WHERE DonHangID=?",
      values,
      connection,
    );
  }

  appendHistory(values, connection) {
    return query(
      'INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,?,?)',
      values,
      connection,
    );
  }

  orderForPayment(values, connection) {
    return query('SELECT DonHangID FROM ThanhToan WHERE ThanhToanID=?', values, connection);
  }

  lockOrderWithMethod(values, connection) {
    return query(
      'SELECT d.*,m.MaPhuongThuc FROM DonHang d JOIN PhuongThucThanhToan m ON m.PhuongThucID=d.PhuongThucID WHERE d.DonHangID=? FOR UPDATE',
      values,
      connection,
    );
  }

  lockPayment(values, connection) {
    return query('SELECT * FROM ThanhToan WHERE ThanhToanID=? FOR UPDATE', values, connection);
  }

  confirmPayment(values, connection) {
    return query(
      "UPDATE ThanhToan SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?",
      values,
      connection,
    );
  }

  clearDeadline(values, connection) {
    return query('UPDATE DonHang SET HanThanhToan=NULL WHERE DonHangID=?', values, connection);
  }

  updateDelivery(values, connection) {
    return query(
      'UPDATE DonHang SET TrangThaiGiaoHang=?,DonViVanChuyen=COALESCE(?,DonViVanChuyen),MaVanDon=COALESCE(?,MaVanDon),PhienBan=PhienBan+1 WHERE DonHangID=?',
      values,
      connection,
    );
  }

  recordDelivery(values, connection) {
    return query(
      'INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,?,?)',
      values,
      connection,
    );
  }

  lockItems(values, connection) {
    return query(
      'SELECT * FROM ChiTietDonHang WHERE DonHangID=? ORDER BY BienTheID',
      values,
      connection,
    );
  }

  createReturnReceipt(values, connection) {
    return query(
      "INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID,TrangThai,NgayXacNhan,NguoiXacNhanID,DonHangID) VALUES(?,?,'NHAN_TRA',?,?,'DA_XAC_NHAN',UTC_TIMESTAMP(),?,?)",
      values,
      connection,
    );
  }

  addReceiptLine(values, connection) {
    return query(
      'INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,GhiChu) VALUES(?,?,?,?)',
      values,
      connection,
    );
  }

  cancelReturnedOrder(values, connection) {
    return query(
      "UPDATE DonHang SET TrangThai='DA_HUY',TrangThaiGiaoHang='DA_NHAN_HOAN',NgayHuy=UTC_TIMESTAMP(),NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1 WHERE DonHangID=?",
      values,
      connection,
    );
  }

  recordReturn(values, connection) {
    return query(
      "INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,'DANG_GIAO','DA_HUY',?,?)",
      values,
      connection,
    );
  }

  lockLateOrder(values, connection) {
    return query(
      'SELECT d.*,p.MaPhuongThuc method FROM DonHang d JOIN PhuongThucThanhToan p ON p.PhuongThucID=d.PhuongThucID WHERE d.DonHangID=? FOR UPDATE',
      values,
      connection,
    );
  }

  lockLatestPayment(values, connection) {
    return query(
      'SELECT * FROM ThanhToan WHERE DonHangID=? ORDER BY ThanhToanID LIMIT 1 FOR UPDATE',
      values,
      connection,
    );
  }

  confirmLatePayment(values, connection) {
    return query(
      "UPDATE ThanhToan SET TrangThai='DA_XAC_NHAN',SoTien=?,NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?",
      values,
      connection,
    );
  }
}
