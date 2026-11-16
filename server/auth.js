import { randomBytes, scryptSync, timingSafeEqual, createHash } from 'node:crypto';
import { query, fail } from './db.js';
import { SESSION_AREAS, requestArea, sessionCookieOptions, clearSessionCookie, validSessionToken } from './session-cookie.js';
export function hashPassword(password) { const salt = randomBytes(16).toString('hex'); return `${salt}:${scryptSync(password, salt, 64).toString('hex')}`; }
export function verifyPassword(password, stored) {
  try { const [salt, key] = stored.split(':'); const expected = Buffer.from(key, 'hex'); const actual = scryptSync(password, salt, 64); return expected.length === actual.length && timingSafeEqual(actual, expected); } catch { return false; }
}
export const tokenHash = token => createHash('sha256').update(token).digest();
export async function identify(req, res, next) {
  try {
    req.sessionArea = requestArea(req);
    const token = req.cookies[SESSION_AREAS[req.sessionArea].cookie];
    res.set('Cache-Control', 'no-store');
    // The previous shared cookie cannot safely identify an independent area.
    if (req.cookies.sole_session) res.clearCookie('sole_session', sessionCookieOptions(req));
    if (token && !validSessionToken(token)) { clearSessionCookie(req, res); return next(); }
    if (token) {
      const rows = await query(`SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.SoDienThoai phone,t.DiaChi address,t.TenDangNhap username
        FROM PhienDangNhap p JOIN TaiKhoan t ON t.TaiKhoanID=p.TaiKhoanID
        WHERE p.TokenHash=? AND p.ThuHoiLuc IS NULL AND p.HetHanLuc>UTC_TIMESTAMP() AND t.TrangThai='HOAT_DONG'`, [tokenHash(token)]);
      if (rows[0]) {
        const roles = (await query('SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=?', [rows[0].id])).map(r => r.role);
        if (roles.includes(SESSION_AREAS[req.sessionArea].role)) req.user = {...rows[0], roles};
      }
      if (!req.user) clearSessionCookie(req, res);
    }
    next();
  } catch (e) { next(e); }
}
export function requireUser(req, res, next) { if (!req.user) return next(Object.assign(new Error('Vui lòng đăng nhập để tiếp tục.'), {status:401})); next(); }
export const role = allowed => (req, res, next) => { if (!req.user?.roles.some(r => allowed.includes(r))) return next(Object.assign(new Error('Tài khoản không có quyền thực hiện chức năng này.'), {status:403})); next(); };
export async function createSession(user, req, res, area) {
  const token = randomBytes(32).toString('hex');
  await query('INSERT INTO PhienDangNhap(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 7 DAY))', [user.id,tokenHash(token)]);
  res.cookie(SESSION_AREAS[area].cookie, token, { ...sessionCookieOptions(req), maxAge:7*86400000 });
}
export async function audit(user, action, object, id, content, c) {
  return query("INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,MaDoiTuong,NoiDung,KetQua) VALUES(?,?,?,?,?,'THANH_CONG')", [user?.id||null,action,object,String(id),content],c);
}
