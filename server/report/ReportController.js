import { ReportService } from './ReportService.js';

// Ánh xạ API cấu hình công khai, khách hàng, doanh thu và giám sát hệ thống tới ReportService.
export class ReportController {
  constructor(service = new ReportService()) {
    this.service = service;
  }

  registerPublic(router, { run }) {
    router.get(
      '/store-settings',
      run(() => this.service.settings()),
    );
  }

  register(router, { run, manager, admin }) {
    router.get(
      '/manage/customers',
      manager,
      run(() => this.service.customers()),
    );
    router.get(
      '/manage/reports',
      manager,
      run((r) => this.service.sales(r)),
    );
    router.get(
      '/admin/logs',
      admin,
      run(() => this.service.logs()),
    );
    router.get(
      '/admin/system',
      admin,
      run(() => this.service.system()),
    );
  }
}
