import { query } from '../db.js';

// UserService truyền kết nối để mọi bước ghi cùng nằm trong một giao dịch.
export class UserRepository {
  list() {
    return query(
      "SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.TenDangNhap username,t.SoDienThoai phone,t.TrangThai status,t.NgayTao date,GROUP_CONCAT(v.MaVaiTro) roles FROM TaiKhoan t LEFT JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID LEFT JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE t.NgayXoa IS NULL GROUP BY t.TaiKhoanID ORDER BY t.TaiKhoanID",
      []
    );
  }

  insert(email, passwordHash, name, connection) {
    return query(
      "INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen) VALUES(?,?,?,?)",
      [email, email, passwordHash, name], connection
    );
  }

  assignRole(id, actorId, role, connection) {
    return query(
      "INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID,NguoiGanID) SELECT ?,VaiTroID,? FROM VaiTro WHERE MaVaiTro=?",
      [id, actorId, role], connection
    );
  }

  lockActiveAdmins(connection) {
    return query(
      "SELECT t.TaiKhoanID id FROM TaiKhoan t JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE v.MaVaiTro='SYSTEM_ADMIN' AND t.TrangThai='HOAT_DONG' ORDER BY t.TaiKhoanID FOR UPDATE",
      [], connection
    );
  }

  lockRoles(id, connection) {
    return query(
      "SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=? FOR UPDATE",
      [id], connection
    );
  }

  updateProfile(name, status, id, connection) {
    return query(
      "UPDATE TaiKhoan SET HoTen=?,TrangThai=?,NgayCapNhat=UTC_TIMESTAMP() WHERE TaiKhoanID=?",
      [name, status, id], connection
    );
  }

  changeRole(role, actorId, id, connection) {
    return query(
      "UPDATE TaiKhoanVaiTro SET VaiTroID=(SELECT VaiTroID FROM VaiTro WHERE MaVaiTro=?),NguoiGanID=?,NgayGan=UTC_TIMESTAMP() WHERE TaiKhoanID=?",
      [role, actorId, id], connection
    );
  }

  revokeSessions(id, connection) {
    return query(
      "UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=?",
      [id], connection
    );
  }

  lockAccount(id, connection) {
    return query(
      "SELECT TrangThai FROM TaiKhoan WHERE TaiKhoanID=? AND NgayXoa IS NULL FOR UPDATE",
      [id], connection
    );
  }

  anonymize(alias, passwordHash, id, connection) {
    return query(
      "UPDATE TaiKhoan SET NgayXoa=UTC_TIMESTAMP(),NgayCapNhat=UTC_TIMESTAMP(),TenDangNhap=?,Email=?,MatKhauHash=?,SoDienThoai=NULL,DiaChi=NULL,NgaySinh=NULL,GioiTinh=NULL,AnhDaiDien=NULL WHERE TaiKhoanID=?",
      [alias, alias, passwordHash, id], connection
    );
  }

  revokeActiveSessions(id, connection) {
    return query(
      "UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND ThuHoiLuc IS NULL",
      [id], connection
    );
  }

  invalidateResetCodes(id, connection) {
    return query(
      "UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL",
      [id], connection
    );
  }
}
