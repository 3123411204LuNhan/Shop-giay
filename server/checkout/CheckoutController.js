import { CheckoutService } from './CheckoutService.js';

// Ánh xạ API giỏ hàng, báo giá và tạo đơn tới CheckoutService.
export class CheckoutController {
  constructor(service = new CheckoutService()) {
    this.service = service;
  }

  register(router, { run }) {
    router.get(
      '/cart',
      run((r) => this.service.cart(r)),
    );
    router.put(
      '/cart',
      run((r) => this.service.updateCart(r)),
    );
    router.post(
      '/checkout/quote',
      run((r) => this.service.quote(r)),
    );
    router.post(
      '/orders',
      run((r) => this.service.placeOrder(r)),
    );
  }
}
