import { fail } from '../db.js';

// User biểu diễn tài khoản đã đọc và khóa qua UserRepository, gồm trạng thái và các vai trò hiện tại.
export class User {
  constructor(status, roles) {
    this.status = status;
    this.roles = roles;
  }

  // UserService.update hỏi User xem vai trò mới có hợp lệ và có cần ghi lại hay không.
  needsRoleChange(requested) {
    const current = this.roles.length === 1 ? this.roles[0] : null;
    if (requested === undefined || requested === current) return false;
    const staff = ['SYSTEM_ADMIN', 'SHOP_MANAGER'];
    if (!staff.includes(current) || !staff.includes(requested)) {
      fail('Chỉ được đổi vai trò giữa quản lý shop và nhân viên kỹ thuật.', 403);
    }
    return true;
  }

  // UserService.remove chỉ cho xóa khách hoặc quản lý đã khóa.
  assertDeletable() {
    if (!this.roles.length || this.roles.some(role => !['CUSTOMER', 'SHOP_MANAGER'].includes(role))) {
      fail('Chỉ được xóa tài khoản khách hàng hoặc quản lý shop.', 403);
    }
    if (this.status !== 'BI_KHOA') fail('Hãy khóa tài khoản trước khi xóa.', 409);
  }
}
