import { OrderService } from './OrderService.js';

// Ánh xạ API tra cứu đơn, hủy đơn, giao hàng và đối soát tới OrderService.
export class OrderController {
  constructor(service = new OrderService()) {
    this.service = service;
  }

  register(router, { run, manager }) {
    router.get(
      '/orders',
      run((r) => this.service.listMine(r)),
    );
    router.get(
      '/orders/:id',
      run((r) => this.service.details(r)),
    );
    router.post(
      '/orders/:id/cancel',
      run((r) => this.service.cancel(r)),
    );
    router.get(
      '/manage/orders',
      manager,
      run(() => this.service.listAll()),
    );
    router.post(
      '/manage/orders/:id/status',
      manager,
      run((r) => this.service.advance(r)),
    );
    router.post(
      '/manage/payments/:id/confirm',
      manager,
      run((r) => this.service.confirmPayment(r)),
    );
    router.get(
      '/manage/customers/:id/orders',
      manager,
      run((r) => this.service.customerHistory(r)),
    );
    router.post(
      '/manage/orders/:id/delivery',
      manager,
      run((r) => this.service.updateDelivery(r)),
    );
    router.post(
      '/manage/orders/:id/receive-delivery-return',
      manager,
      run((r) => this.service.receiveDeliveryReturn(r)),
    );
    router.post(
      '/manage/orders/:id/late-payment',
      manager,
      run((r) => this.service.recordLatePayment(r)),
    );
  }
}
