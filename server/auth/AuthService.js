import { required } from '../input.js';
import { AuthRepository } from './AuthRepository.js';
import { tx, fail, text } from '../db.js';
import { hashPassword, verifyPassword, createSession, tokenHash, audit } from '../auth.js';
import { SESSION_AREAS, loginArea, clearSessionCookie, validSessionToken } from '../session-cookie.js';
import {
  requestPasswordReset,
  verifyResetCode,
  resetPassword,
  saveNewPassword,
} from '../password-reset.js';
const repository = new AuthRepository();

// AuthController chuyển yêu cầu tới các phương thức xử lý phiên đăng nhập, hồ sơ và mật khẩu.
export class AuthService {
  async me(r) {
    return { user: r.user || null };
  }

  async login(r, s) {
    const users = await repository.findLogin([text(r.body.login), text(r.body.login)]);
    const u = users[0];
    if (
      !u ||
      String(r.body.password || '').length > 128 ||
      !verifyPassword(String(r.body.password || ''), u.hash)
    ) {
      if (u) await repository.recordLoginFailure([u.id]);
      fail('Email hoặc mật khẩu không đúng.', 401);
    }
    if (u.status !== 'HOAT_DONG' || (u.locked && new Date(u.locked) > new Date()))
      fail('Tài khoản đang bị khóa.', 403);
    const roles = (await repository.roles([u.id])).map((x) => x.role);
    const area = loginArea(roles, r.sessionArea);
    if (!area) fail('Tài khoản không có quyền đăng nhập khu vực này.', 403);
    await createSession(u, r, s, area);
    await repository.recordLogin([u.id]);
    return { ok: true, area };
  }

  async register(r, s) {
    if (r.sessionArea !== 'customer')
      fail('Vui lòng đăng ký tài khoản khách hàng tại khu vực mua sắm.', 403);
    const name = required(r.body.name, 'họ tên'),
      email = required(r.body.email, 'email').toLowerCase(),
      password = String(r.body.password || '');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) || password.length < 8 || password.length > 128)
      fail('Email chưa hợp lệ hoặc mật khẩu chưa đủ 8 ký tự.');
    const id = await tx(async (c) => {
      const a = await repository.createCustomer(
        [email, email, hashPassword(password), name, text(r.body.phone, 20) || null],
        c,
      );
      await repository.assignCustomerRole([a.insertId], c);
      return a.insertId;
    });
    await createSession({ id }, r, s, 'customer');
    return { ok: true, area: 'customer' };
  }

  async logout(r, s) {
    const token = r.cookies[SESSION_AREAS[r.sessionArea].cookie];
    if (validSessionToken(token)) await repository.revokeSession([tokenHash(token)]);
    clearSessionCookie(r, s);
    return { ok: true };
  }

  async requestReset(r) {
    return requestPasswordReset(r.body.email);
  }

  async verifyOtp(r) {
    return verifyResetCode(r.body);
  }

  async resetPassword(r) {
    return resetPassword(r.body);
  }

  async changeStaffPassword(r, s) {
    const { password } = r.body;
    if (typeof password !== 'string' || password.length < 8 || password.length > 128)
      fail('Mật khẩu cần từ 8 đến 128 ký tự.');
    const id = r.user.id;
    await tx(async (c) => {
      const [account] = await repository.lockActiveUser([id], c);
      const token = r.cookies[SESSION_AREAS[r.sessionArea].cookie];
      const [session] = await repository.lockSession([id, tokenHash(token)], c);
      if (!account || !session) fail('Vui lòng đăng nhập để tiếp tục.', 401);
      await saveNewPassword(c, id, password);
      await audit(r.user, 'DOI_MAT_KHAU', 'TaiKhoan', id, 'Nhân sự tự đổi mật khẩu', c);
    });
    clearSessionCookie(r, s);
    return { ok: true };
  }

  async updateProfile(r) {
    await repository.updateProfile([
      required(r.body.name, 'họ tên'),
      text(r.body.phone, 20) || null,
      text(r.body.address, 500) || null,
      r.user.id,
    ]);
    return { ok: true };
  }
}
