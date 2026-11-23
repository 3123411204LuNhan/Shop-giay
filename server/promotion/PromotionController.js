import { PromotionService } from './PromotionService.js';

// Ánh xạ API công bố, tạo, sửa và xóa khuyến mãi tới PromotionService.
export class PromotionController {
  constructor(service = new PromotionService()) {
    this.service = service;
  }

  registerPublic(router, { run }) {
    router.get(
      '/promotions',
      run(() => this.service.published()),
    );
  }

  register(router, { run, manager }) {
    router.get(
      '/manage/promotions',
      manager,
      run(() => this.service.list()),
    );
    router.post(
      '/manage/promotions',
      manager,
      run((r) => this.service.create(r)),
    );
    router.put(
      '/manage/promotions/:id',
      manager,
      run((r) => this.service.update(r)),
    );
    router.delete(
      '/manage/promotions/:id',
      manager,
      run((r) => this.service.deleteDraft(r)),
    );
  }
}
