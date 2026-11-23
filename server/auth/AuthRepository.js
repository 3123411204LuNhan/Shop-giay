import { query } from '../db.js';

// AuthService dùng các truy vấn này; connection giữ các bước ghi trong cùng giao dịch.
export class AuthRepository {
  findLogin(values) {
    return query(
      'SELECT TaiKhoanID id,MatKhauHash hash,TrangThai status,KhoaDenLuc locked FROM TaiKhoan WHERE NgayXoa IS NULL AND (Email=? OR TenDangNhap=?)',
      values,
    );
  }

  recordLoginFailure(values) {
    return query(
      'UPDATE TaiKhoan SET SoLanDangNhapSai=SoLanDangNhapSai+1,KhoaDenLuc=IF(SoLanDangNhapSai>=5,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 15 MINUTE),KhoaDenLuc) WHERE TaiKhoanID=?',
      values,
    );
  }

  roles(values) {
    return query(
      'SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=?',
      values,
    );
  }

  recordLogin(values) {
    return query(
      'UPDATE TaiKhoan SET LanDangNhapCuoi=UTC_TIMESTAMP(),SoLanDangNhapSai=0 WHERE TaiKhoanID=?',
      values,
    );
  }

  createCustomer(values, connection) {
    return query(
      'INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai) VALUES(?,?,?,?,?)',
      values,
      connection,
    );
  }

  assignCustomerRole(values, connection) {
    return query(
      "INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID) SELECT ?,VaiTroID FROM VaiTro WHERE MaVaiTro='CUSTOMER'",
      values,
      connection,
    );
  }

  revokeSession(values) {
    return query('UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TokenHash=?', values);
  }

  lockActiveUser(values, connection) {
    return query(
      "SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? AND TrangThai='HOAT_DONG' AND NgayXoa IS NULL FOR UPDATE",
      values,
      connection,
    );
  }

  lockSession(values, connection) {
    return query(
      'SELECT PhienID FROM PhienDangNhap WHERE TaiKhoanID=? AND TokenHash=? AND ThuHoiLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP() FOR UPDATE',
      values,
      connection,
    );
  }

  updateProfile(values) {
    return query(
      'UPDATE TaiKhoan SET HoTen=?,SoDienThoai=?,DiaChi=?,NgayCapNhat=UTC_TIMESTAMP() WHERE TaiKhoanID=?',
      values,
    );
  }
}
