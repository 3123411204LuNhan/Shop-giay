import { query } from '../db.js';

// CheckoutService dùng các truy vấn này; connection giữ các bước ghi trong cùng giao dịch.
export class CheckoutRepository {
  ensureCart(values, connection) {
    return query(
      'INSERT INTO GioHang(TaiKhoanID) VALUES(?) ON DUPLICATE KEY UPDATE TaiKhoanID=VALUES(TaiKhoanID)',
      values,
      connection,
    );
  }

  cartId(values, connection) {
    return query('SELECT GioHangID id FROM GioHang WHERE TaiKhoanID=?', values, connection);
  }

  checkoutItems(values, connection) {
    return query(
      `SELECT ct.*,b.SKU,b.SanPhamID,b.DangKinhDoanh,COALESCE(b.GiaBanRieng,s.GiaBan) price,s.MaSanPham,s.TenSanPham,s.TrangThai,s.DanhMucID,s.ThuongHieuID,d.TenDanhMuc,t.TenThuongHieu,kc.GiaTri size,m.TenMau color FROM ChiTietGioHang ct JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID WHERE ct.GioHangID=? AND d.DangHoatDong=1 AND t.DangHoatDong=1 ORDER BY b.BienTheID`,
      values,
      connection,
    );
  }

  countItems(values, connection) {
    return query('SELECT COUNT(*) n FROM ChiTietGioHang WHERE GioHangID=?', values, connection);
  }

  lockStock(values, connection) {
    return query(
      'SELECT SoLuongKhaDung stock FROM TonKho WHERE BienTheID=? FOR UPDATE',
      values,
      connection,
    );
  }

  lockPromotion(values, connection) {
    return query(
      "SELECT * FROM KhuyenMai WHERE MaKhuyenMai=? AND TrangThai='DANG_AP_DUNG' AND BatDauLuc<=UTC_TIMESTAMP() AND KetThucLuc>UTC_TIMESTAMP() FOR UPDATE",
      values,
      connection,
    );
  }

  promotionUsage(values, connection) {
    return query(
      "SELECT COUNT(*) n,SUM(d.KhachHangID=?) own FROM KhuyenMaiDonHang k JOIN DonHang d ON d.DonHangID=k.DonHangID WHERE k.KhuyenMaiID=? AND d.TrangThai<>'DA_HUY'",
      values,
      connection,
    );
  }

  eligibleIds(map, values, connection) {
    return query(`SELECT ${map[1]} id FROM ${map[0]} WHERE KhuyenMaiID=?`, values, connection);
  }

  paymentMethod(values, connection) {
    return query(
      'SELECT PhuongThucID id FROM PhuongThucThanhToan WHERE MaPhuongThuc=? AND DangHoatDong=1',
      values,
      connection,
    );
  }

  cartItems(values) {
    return query(
      `SELECT ct.BienTheID variantId,ct.SoLuong quantity,s.TenSanPham name,COALESCE(b.GiaBanRieng,s.GiaBan) price,kc.GiaTri size,m.TenMau color,k.SoLuongKhaDung stock,
     (SELECT DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1) image FROM ChiTietGioHang ct JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE ct.GioHangID=?`,
      values,
    );
  }

  removeItem(values, connection) {
    return query('DELETE FROM ChiTietGioHang WHERE GioHangID=? AND BienTheID=?', values, connection);
  }

  availableVariant(values, connection) {
    return query(
      "SELECT k.SoLuongKhaDung stock FROM TonKho k JOIN BienTheSanPham b ON b.BienTheID=k.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID WHERE b.BienTheID=? AND b.DangKinhDoanh=1 AND s.TrangThai='DANG_BAN'",
      values,
      connection,
    );
  }

  setQuantity(values, connection) {
    return query(
      'INSERT INTO ChiTietGioHang(GioHangID,BienTheID,SoLuong) VALUES(?,?,?) ON DUPLICATE KEY UPDATE SoLuong=VALUES(SoLuong),NgayCapNhat=UTC_TIMESTAMP()',
      values,
      connection,
    );
  }

  lockCustomer(values, connection) {
    return query('SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE', values, connection);
  }

  findOrderByKey(values, connection) {
    return query(
      'SELECT DonHangID id,KhachHangID owner FROM DonHang WHERE MaYeuCauTao=?',
      values,
      connection,
    );
  }

  orderTotal(values, connection) {
    return query('SELECT TongThanhToan total FROM DonHang WHERE DonHangID=?', values, connection);
  }

  createOrder(values, connection) {
    return query(
      'INSERT INTO DonHang(MaDonHang,MaYeuCauTao,KhachHangID,HoTenNguoiNhan,SoDienThoaiNhan,DiaChiNhan,GhiChu,PhuongThucID,TienHang,TienGiam,PhiVanChuyen) VALUES(?,?,?,?,?,?,?,?,?,?,?)',
      values,
      connection,
    );
  }

  setDeadline(values, connection) {
    return query(
      'UPDATE DonHang SET HanThanhToan=DATE_ADD(UTC_TIMESTAMP(),INTERVAL ? MINUTE) WHERE DonHangID=?',
      values,
      connection,
    );
  }

  addOrderLine(values, connection) {
    return query(
      'INSERT INTO ChiTietDonHang(DonHangID,BienTheID,MaSanPhamLucMua,SKULucMua,TenSanPhamLucMua,KichCoLucMua,MauSacLucMua,DanhMucLucMua,ThuongHieuLucMua,SoLuong,DonGia,TienGiamPhanBo) VALUES(?,?,?,?,?,?,?,?,?,?,?,?)',
      values,
      connection,
    );
  }

  allocateStock(values, connection) {
    return query(
      'INSERT INTO PhanBoTonKho(ChiTietDonHangID,SoLuongGiu) VALUES(?,?)',
      values,
      connection,
    );
  }

  recordPromotion(values, connection) {
    return query(
      'INSERT INTO KhuyenMaiDonHang(DonHangID,KhuyenMaiID,MaLucApDung,TenLucApDung,HinhThucLucApDung,GiaTriLucApDung,TienGiamThucTe) VALUES(?,?,?,?,?,?,?)',
      values,
      connection,
    );
  }

  recordHistory(values, connection) {
    return query(
      "INSERT INTO LichSuDonHang(DonHangID,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,'CHO_XAC_NHAN',?,'Khách tạo đơn')",
      values,
      connection,
    );
  }

  createPayment(values, connection) {
    return query(
      'INSERT INTO ThanhToan(DonHangID,PhuongThucID,MaYeuCau,SoTien) VALUES(?,?,?,?)',
      values,
      connection,
    );
  }

  emptyCart(values, connection) {
    return query('DELETE FROM ChiTietGioHang WHERE GioHangID=?', values, connection);
  }
}
