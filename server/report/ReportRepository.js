import { query } from '../db.js';

// ReportService dùng các truy vấn này; connection giữ các bước ghi trong cùng giao dịch.
export class ReportRepository {
  customers() {
    return query(
      "SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.SoDienThoai phone,COUNT(d.DonHangID) orders,COALESCE(SUM(d.TongThanhToan),0) total FROM TaiKhoan t JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID AND v.MaVaiTro='CUSTOMER' LEFT JOIN DonHang d ON d.KhachHangID=t.TaiKhoanID WHERE t.NgayXoa IS NULL GROUP BY t.TaiKhoanID ORDER BY t.TaiKhoanID",
    );
  }

  income(values) {
    return query(
      "SELECT COALESCE(SUM(SoTien),0) value FROM ThanhToan WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<?",
      values,
    );
  }

  refunded(values) {
    return query(
      "SELECT COALESCE(SUM(SoTien),0) value FROM HoanTien WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<?",
      values,
    );
  }

  orderCounts(values) {
    return query(
      'SELECT TrangThai status,COUNT(*) count FROM DonHang WHERE NgayDat>=? AND NgayDat<? GROUP BY TrangThai',
      values,
    );
  }

  dailyIncome(values) {
    return query(
      "SELECT DATE(CONVERT_TZ(NgayXacNhan,'+00:00','+07:00')) day,SUM(SoTien) amount FROM ThanhToan WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<? GROUP BY DATE(CONVERT_TZ(NgayXacNhan,'+00:00','+07:00'))",
      values,
    );
  }

  bestSellers(values) {
    return query(
      "SELECT ct.TenSanPhamLucMua name,SUM(ct.SoLuong) quantity,SUM(ct.ThanhTien) amount FROM ChiTietDonHang ct JOIN DonHang d ON d.DonHangID=ct.DonHangID WHERE d.TrangThai='HOAN_THANH' AND d.NgayHoanThanh>=? AND d.NgayHoanThanh<? GROUP BY ct.TenSanPhamLucMua ORDER BY quantity DESC LIMIT 5",
      values,
    );
  }

  lowStock() {
    return query('SELECT COUNT(*) n FROM TonKho WHERE SoLuongKhaDung<=MucCanhBao');
  }

  logs() {
    return query(
      'SELECT n.NhatKyID id,t.HoTen name,n.HanhDong action,n.DoiTuong object,n.MaDoiTuong objectId,n.NoiDung content,n.KetQua result,n.ThoiDiem date FROM NhatKyHeThong n LEFT JOIN TaiKhoan t ON t.TaiKhoanID=n.NguoiThucHienID ORDER BY n.NhatKyID DESC LIMIT 200',
    );
  }

  databaseInfo() {
    return query('SELECT DATABASE() name,VERSION() version');
  }

  tableCount() {
    return query('SELECT COUNT(*) n FROM information_schema.tables WHERE table_schema=DATABASE()');
  }

  activeSessions() {
    return query(
      'SELECT COUNT(*) n FROM PhienDangNhap WHERE ThuHoiLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP()',
    );
  }

  accountCount() {
    return query('SELECT COUNT(*) n FROM TaiKhoan WHERE NgayXoa IS NULL');
  }
}
