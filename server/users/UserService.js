import { randomUUID } from 'node:crypto';
import { audit, hashPassword } from '../auth.js';
import { tx, fail, positive, text } from '../db.js';
import { User } from './User.js';
import { UserRepository } from './UserRepository.js';

function required(value, label) {
  const result = text(value);
  if (!result) fail('Vui lòng nhập ' + label + '.');
  return result;
}

// UserController gọi các phương thức dưới đây. UserService quyết định nghiệp vụ và ranh giới giao dịch.
export class UserService {
  constructor(repository = new UserRepository()) {
    this.repository = repository;
  }

  list() {
    return this.repository.list();
  }

  async loadLockedUser(id, connection) {
    const [account] = await this.repository.lockAccount(id, connection);
    if (!account) fail('Tài khoản không tồn tại.', 404);
    const roles = await this.repository.lockRoles(id, connection);
    return new User(account.TrangThai, roles.map(row => row.role));
  }

  // Tạo tài khoản, gán vai trò đã có và ghi người thực hiện trong cùng giao dịch.
  async create(actor, body) {
    const password = String(body.password || '');
    if (password.length < 8 || password.length > 128) fail('Mật khẩu cần từ 8 đến 128 ký tự.');
    if (!['CUSTOMER', 'SHOP_MANAGER'].includes(body.role)) fail('Chỉ được tạo tài khoản khách hàng hoặc quản lý shop.');
    const email = required(body.email, 'email');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) fail('Email không hợp lệ.');
    const name = required(body.name, 'họ tên');
    return tx(async connection => {
      const { insertId: id } = await this.repository.insert(email, hashPassword(password), name, connection);
      await this.repository.assignRole(id, actor.id, body.role, connection);
      await audit(actor, 'TAO_TAI_KHOAN', 'TaiKhoan', id, body.role, connection);
      return { id };
    });
  }

  // Khóa nhóm admin trước tài khoản đích để hai yêu cầu đồng thời không loại bỏ admin cuối cùng.
  async update(actor, rawId, body) {
    const id = positive(rawId);
    if (id === actor.id) fail('Không tự sửa vai trò hoặc khóa tài khoản đang dùng.');
    if (!['HOAT_DONG', 'BI_KHOA'].includes(body.status)) fail('Trạng thái hoặc vai trò không hợp lệ.');
    return tx(async connection => {
      const admins = await this.repository.lockActiveAdmins(connection);
      const user = await this.loadLockedUser(id, connection);
      if (!user.roles.length) fail('Không tìm thấy tài khoản có vai trò.', 404);
      const change = user.needsRoleChange(body.role);
      if (admins.length === 1 && admins[0].id === id &&
          (body.status !== 'HOAT_DONG' || (change && body.role !== 'SYSTEM_ADMIN'))) {
        fail('Không thể vô hiệu hóa quản trị viên cuối cùng.');
      }
      await this.repository.updateProfile(required(body.name, 'họ tên'), body.status, id, connection);
      if (change) await this.repository.changeRole(body.role, actor.id, id, connection);
      await this.repository.revokeSessions(id, connection);
      const detail = (body.role ?? user.roles.join(', ')) + ' / ' + body.status;
      await audit(actor, 'SUA_PHAN_QUYEN', 'TaiKhoan', id, detail, connection);
      return { ok: true };
    });
  }

  // Xóa mềm giữ ID trên đơn cũ, thu hồi phiên và các yêu cầu đặt lại mật khẩu.
  async remove(actor, rawId) {
    const id = positive(rawId);
    if (id === actor.id) fail('Bạn không thể xóa tài khoản đang đăng nhập.', 403);
    return tx(async connection => {
      const user = await this.loadLockedUser(id, connection);
      user.assertDeletable();
      await this.repository.anonymize('deleted-' + randomUUID(), hashPassword(randomUUID()), id, connection);
      await this.repository.revokeActiveSessions(id, connection);
      await this.repository.invalidateResetCodes(id, connection);
      await audit(actor, 'XOA_TAI_KHOAN', 'TaiKhoan', id, user.roles.join(', '), connection);
      return { ok: true };
    });
  }
}
