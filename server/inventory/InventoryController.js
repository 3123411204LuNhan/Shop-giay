import { InventoryService } from './InventoryService.js';

// Ánh xạ API phiếu kho, ngưỡng cảnh báo và lịch sử tồn tới InventoryService.
export class InventoryController {
  constructor(service = new InventoryService()) {
    this.service = service;
  }

  register(router, { run, manager }) {
    router.get('/manage/stock',manager,run(r=>this.service.stock(r)));
    router.post(
      '/manage/stock',
      manager,
      run(() => this.service.rejectDirectAdjustment()),
    );
    router.get(
      '/manage/receipts',
      manager,
      run(() => this.service.receipts()),
    );
    router.get(
      '/manage/receipts/:id',
      manager,
      run((r) => this.service.receipt(r)),
    );
    router.post(
      '/manage/receipts',
      manager,
      run((r) => this.service.createReceipt(r)),
    );
    router.put(
      '/manage/receipts/:id',
      manager,
      run((r) => this.service.updateReceipt(r)),
    );
    router.post(
      '/manage/receipts/:id/confirm',
      manager,
      run((r) => this.service.confirmReceipt(r)),
    );
    router.post(
      '/manage/receipts/:id/cancel',
      manager,
      run((r) => this.service.cancelReceipt(r)),
    );
    router.put(
      '/manage/stock/:id/warning',
      manager,
      run((r) => this.service.setWarning(r)),
    );
    router.get(
      '/manage/stock-history',
      manager,
      run((r) => this.service.stockHistory(r)),
    );
  }
}
