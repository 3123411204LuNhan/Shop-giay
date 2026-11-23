import { ReturnService } from './ReturnService.js';

// Ánh xạ API đánh giá, yêu cầu trả hàng và hoàn tiền tới ReturnService.
export class ReturnController {
  constructor(service = new ReturnService()) {
    this.service = service;
  }

  register(router, { run, manager }) {
    router.post(
      '/reviews',
      run((r) => this.service.review(r)),
    );
    router.get(
      '/returns',
      run((r) => this.service.listMine(r)),
    );
    router.post(
      '/returns',
      run((r) => this.service.requestReturn(r)),
    );
    router.get(
      '/returns/:id',
      run((r) => this.service.detail(r)),
    );
    router.get(
      '/manage/returns',
      manager,
      run(() => this.service.listAll()),
    );
    router.post(
      '/manage/returns/:id/action',
      manager,
      run((r) => this.service.processReturn(r)),
    );
    router.get(
      '/manage/refunds',
      manager,
      run(() => this.service.refunds()),
    );
    router.post(
      '/manage/refunds/:id/confirm',
      manager,
      run((r) => this.service.confirmRefund(r)),
    );
    router.post(
      '/manage/refunds/:id/retry',
      manager,
      run((r) => this.service.retryRefund(r)),
    );
    router.post(
      '/manage/refunds/:id/failure',
      manager,
      run((r) => this.service.markRefundFailed(r)),
    );
  }
}
