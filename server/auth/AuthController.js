import { AuthService } from './AuthService.js';

// Ánh xạ API phiên đăng nhập, hồ sơ và mật khẩu tới AuthService.
export class AuthController {
  constructor(service = new AuthService()) {
    this.service = service;
  }

  registerPublic(router, { run }) {
    router.get(
      '/me',
      run((r) => this.service.me(r)),
    );
    router.post(
      '/auth/login',
      run((r, s) => this.service.login(r, s)),
    );
    router.post(
      '/auth/register',
      run((r, s) => this.service.register(r, s)),
    );
    router.post(
      '/auth/logout',
      run((r, s) => this.service.logout(r, s)),
    );
    router.post(
      '/auth/forgot',
      run((r) => this.service.requestReset(r)),
    );
    router.post(
      '/auth/verify-otp',
      run((r) => this.service.verifyOtp(r)),
    );
    router.post(
      '/auth/reset',
      run((r) => this.service.resetPassword(r)),
    );
  }

  registerStaff(router, { run, role }) {
    router.post(
      '/staff/password',
      role(['SHOP_MANAGER', 'SYSTEM_ADMIN']),
      run((r, s) => this.service.changeStaffPassword(r, s)),
    );
  }

  register(router, { run }) {
    router.put(
      '/profile',
      run((r) => this.service.updateProfile(r)),
    );
  }
}
