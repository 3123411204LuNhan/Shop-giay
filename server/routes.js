import { AuthController } from './auth/AuthController.js';
import { CatalogController } from './catalog/CatalogController.js';
import { PromotionController } from './promotion/PromotionController.js';
import { CheckoutController } from './checkout/CheckoutController.js';
import { OrderController } from './order/OrderController.js';
import { ReturnController } from './return/ReturnController.js';
import { InventoryController } from './inventory/InventoryController.js';
import { ReportController } from './report/ReportController.js';
import { Router } from 'express';
import { identify, requireUser, role } from './auth.js';
import { canUseArea } from './session-cookie.js';
import { UserController } from './users/UserController.js';
export { expireUnpaidOrders } from './order/OrderService.js';
export const router = Router();
const manager = role(['SHOP_MANAGER']),
  admin = role(['SYSTEM_ADMIN']);
const run = (fn) => async (req, res, next) => {
  try {
    res.json(await fn(req, res));
  } catch (e) {
    next(e);
  }
};
const guards = { run, role, admin, manager };
const authController = new AuthController();
const catalogController = new CatalogController();
const promotionController = new PromotionController();
const checkoutController = new CheckoutController();
const orderController = new OrderController();
const returnController = new ReturnController();
const inventoryController = new InventoryController();
const reportController = new ReportController();

router.use(identify);
// A tab rendered for A must not mutate B's cart after a shared browser cookie changes.
router.use((r, s, next) => {
  const expected = r.get('x-sole-account');
  const publicAuth =
    r.path === '/me' ||
    ['/auth/login', '/auth/register', '/auth/forgot', '/auth/verify-otp', '/auth/reset'].includes(
      r.path,
    );
  if (expected && r.user && String(r.user.id) !== expected && !publicAuth) {
    return next(
      Object.assign(
        new Error(
          'Tài khoản trong trình duyệt đã thay đổi. Vui lòng kiểm tra lại giỏ hàng trước khi thao tác.',
        ),
        { status: 409, code: 'SESSION_CHANGED' },
      ),
    );
  }
  next();
});
// Các API công khai chạy trước bước bắt buộc đăng nhập.
authController.registerPublic(router, guards);
catalogController.registerPublic(router, guards);
promotionController.registerPublic(router, guards);
reportController.registerPublic(router, guards);
router.use(requireUser);
authController.registerStaff(router, guards);
router.use((r, s, next) => {
  if (r.path.startsWith('/manage/') || r.path.startsWith('/admin/')) return next();
  if (
    r.method === 'GET' &&
    /^\/orders\/\d+$/.test(r.path) &&
    r.sessionArea === 'manager' &&
    r.user.roles.includes('SHOP_MANAGER')
  )
    return next();
  if (r.sessionArea !== 'customer' || !canUseArea(r.user.roles, 'customer'))
    return next(
      Object.assign(new Error('Chức năng mua sắm chỉ dành cho tài khoản khách hàng.'), { status: 403 }),
    );
  next();
});
// Các controller bên dưới áp dụng kiểm tra vai trò cho từng nghiệp vụ.
authController.register(router, guards);
catalogController.register(router, guards);
promotionController.register(router, guards);
checkoutController.register(router, guards);
orderController.register(router, guards);
returnController.register(router, guards);
inventoryController.register(router, guards);
reportController.register(router, guards);
new UserController().register(router, admin, run);
