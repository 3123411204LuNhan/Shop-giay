import { randomBytes, scryptSync, timingSafeEqual, createHash } from 'node:crypto';
import { query, fail } from './db.js';
export function hashPassword(password) { const salt = randomBytes(16).toString('hex'); return `${salt}:${scryptSync(password, salt, 64).toString('hex')}`; }
export function verifyPassword(password, stored) {
  try { const [salt, key] = stored.split(':'); const expected = Buffer.from(key, 'hex'); const actual = scryptSync(password, salt, 64); return expected.length === actual.length && timingSafeEqual(actual, expected); } catch { return false; }
}
export const tokenHash = token => createHash('sha256').update(token).digest();
export async function identify(req, res, next) {
  try {
    const token = req.cookies.sole_session;
    if (token) {
      const rows = await query(`SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.SoDienThoai phone,t.DiaChi address,t.TenDangNhap username
        FROM PhienDangNhap p JOIN TaiKhoan t ON t.TaiKhoanID=p.TaiKhoanID
        WHERE p.TokenHash=? AND p.ThuHoiLuc IS NULL AND p.HetHanLuc>UTC_TIMESTAMP() AND t.TrangThai='HOAT_DONG'`, [tokenHash(token)]);
      if (rows[0]) { req.user = rows[0]; req.user.roles = (await query('SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=?', [req.user.id])).map(r => r.role); }
    }
    next();
  } catch (e) { next(e); }
}
export function requireUser(req, res, next) { if (!req.user) return next(Object.assign(new Error('Vui lòng đăng nhập để tiếp tục.'), {status:401})); next(); }
export const role = allowed => (req, res, next) => { if (!req.user?.roles.some(r => allowed.includes(r))) return next(Object.assign(new Error('Tài khoản không có quyền thực hiện chức năng này.'), {status:403})); next(); };
export async function createSession(user, res) {
  const token = randomBytes(32).toString('hex');
  await query('INSERT INTO PhienDangNhap(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 7 DAY))', [user.id,tokenHash(token)]);
  res.cookie('sole_session', token, { httpOnly:true, sameSite:'strict', secure:process.env.NODE_ENV==='production', maxAge:7*86400000, path:'/' });
}
export async function audit(user, action, object, id, content, c) {
  return query("INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,MaDoiTuong,NoiDung,KetQua) VALUES(?,?,?,?,?,'THANH_CONG')", [user?.id||null,action,object,String(id),content],c);
}
