import { required, code } from '../input.js';
import { CheckoutRepository } from './CheckoutRepository.js';
import { randomUUID, createHash } from 'node:crypto';
import { tx, fail, positive, text } from '../db.js';
import { storeConfig } from '../store-config.js';
import { stockLog } from '../operations.js';
const repository = new CheckoutRepository();

async function cartId(id, c) {
  await repository.ensureCart([id], c);
  return (await repository.cartId([id], c))[0].id;
}

async function checkoutSnapshot(r, c) {
  const cart = await cartId(r.user.id, c);
  const items = await repository.checkoutItems([cart], c);
  if (!items.length) fail('Giỏ hàng đang trống.');
  const [{ n: cartCount }] = await repository.countItems([cart], c);
  if (cartCount !== items.length) fail('Giỏ có sản phẩm đã ngừng kinh doanh.');
  let total = 0;
  for (const item of items) {
    if (!item.DangKinhDoanh || item.TrangThai !== 'DANG_BAN') fail('Có sản phẩm ngừng bán.');
    const [k] = await repository.lockStock([item.BienTheID], c);
    if (!k || k.stock < item.SoLuong) fail(item.TenSanPham + ' không đủ hàng.');
    total += item.price * item.SoLuong;
  }
  let promo = null,
    discount = 0;
  let eligible = items;
  if (r.body.promo) {
    [promo] = await repository.lockPromotion([text(r.body.promo, 50)], c);
    if (!promo || total < promo.GiaTriDonToiThieu)
      fail('Mã ưu đãi không hợp lệ hoặc đơn chưa đạt điều kiện.');
    const [used] = await repository.promotionUsage([r.user.id, promo.KhuyenMaiID], c);
    if (
      (promo.GioiHanLuotDung && used.n >= promo.GioiHanLuotDung) ||
      (promo.GioiHanMoiKhach && used.own >= promo.GioiHanMoiKhach)
    )
      fail('Mã ưu đãi đã hết lượt sử dụng.');
    if (promo.PhamVi !== 'TOAN_BO') {
      const map = {
        SAN_PHAM: ['KhuyenMaiSanPham', 'SanPhamID'],
        DANH_MUC: ['KhuyenMaiDanhMuc', 'DanhMucID'],
        THUONG_HIEU: ['KhuyenMaiThuongHieu', 'ThuongHieuID'],
      }[promo.PhamVi];
      const ids = await repository.eligibleIds(map, [promo.KhuyenMaiID], c);
      eligible = items.filter((x) => ids.some((y) => y.id === x[map[1]]));
    }
    const base = eligible.reduce((s, x) => s + x.price * x.SoLuong, 0);
    const reduction =
      promo.HinhThuc === 'PHAN_TRAM'
        ? Math.min(Math.round((base * promo.GiaTri) / 100), promo.GiamToiDa ?? Infinity)
        : promo.GiaTri;
    discount = Math.min(base, reduction);
    if (!discount) fail('Mã ưu đãi không áp dụng cho sản phẩm trong giỏ.');
  }
  if (!['COD', 'BANK_TRANSFER'].includes(r.body.method || 'COD'))
    fail('Hiện chỉ hỗ trợ COD và chuyển khoản đối soát thủ công.');
  const method = (await repository.paymentMethod([r.body.method || 'COD'], c))[0];
  if (!method) fail('Phương thức thanh toán không hợp lệ.');
  const shipping = total >= 1500000 ? 0 : 30000;

  const fingerprint = createHash('sha256')
    .update(
      JSON.stringify({
        items: items.map((x) => [x.BienTheID, x.SoLuong, x.price]),
        promo: promo?.KhuyenMaiID,
        discount,
        shipping,
        method: method.id,
      }),
    )
    .digest('hex');
  return { cart, items, total, promo, discount, eligible, method, shipping, fingerprint };
}

// CheckoutController chuyển yêu cầu tới các phương thức xử lý giỏ hàng, báo giá và tạo đơn.
export class CheckoutService {
  async cart(r) {
    const id = await cartId(r.user.id);
    return await repository.cartItems([id]);
  }

  async updateCart(r) {
    const variant = positive(r.body.variantId),
      qty = Number(r.body.quantity);
    if (!Number.isInteger(qty) || qty < 0 || qty > 100) fail('Số lượng từ 0 đến 100.');
    await tx(async (c) => {
      const id = await cartId(r.user.id, c);
      if (!qty) {
        await repository.removeItem([id, variant], c);
        return;
      }
      const rows = await repository.availableVariant([variant], c);
      if (!rows[0] || rows[0].stock < qty) fail('Biến thể không đủ tồn kho.');
      await repository.setQuantity([id, variant, qty], c);
    });
    return { ok: true };
  }

  async quote(r) {
    return await tx(async (c) => {
      const snapshot = await checkoutSnapshot(r, c);
      return {
        items: snapshot.items.map((x) => ({
          variantId: x.BienTheID,
          name: x.TenSanPham,
          size: x.size,
          color: x.color,
          quantity: x.SoLuong,
          price: x.price,
        })),
        subtotal: snapshot.total,
        discount: snapshot.discount,
        shipping: snapshot.shipping,
        total: snapshot.total - snapshot.discount + snapshot.shipping,
        fingerprint: snapshot.fingerprint,
      };
    });
  }

  async placeOrder(r) {
    const key = required(r.body.requestId, 'mã yêu cầu');
    if (!/^[\da-f-]{36}$/i.test(key)) fail('Mã yêu cầu không hợp lệ.');
    return await tx(async (c) => {
      // Serialize checkout retries and promotion usage for one consistent cart/order snapshot.
      await repository.lockCustomer([r.user.id], c);
      const existing = await repository.findOrderByKey([key], c);
      if (existing[0]) {
        if (existing[0].owner !== r.user.id) fail('Mã yêu cầu đã được sử dụng.', 409);
        const [old] = await repository.orderTotal([existing[0].id], c);
        return { id: existing[0].id, total: old.total };
      }
      const { cart, items, total, promo, discount, eligible, method, shipping, fingerprint } =
        await checkoutSnapshot(r, c);
      if (r.body.fingerprint && r.body.fingerprint !== fingerprint)
        fail('Giỏ hàng, giá hoặc ưu đãi đã thay đổi. Vui lòng xem lại tổng tiền trước khi đặt.', 409);
      const result = await repository.createOrder(
        [
          code('SS'),
          key,
          r.user.id,
          required(r.body.name, 'người nhận'),
          required(r.body.phone, 'số điện thoại'),
          required(r.body.address, 'địa chỉ'),
          text(r.body.note),
          method.id,
          total,
          discount,
          shipping,
        ],
        c,
      );
      const id = result.insertId;
      if ((r.body.method || 'COD') === 'BANK_TRANSFER')
        await repository.setDeadline([storeConfig.holdMinutes, id], c);
      let remaining = discount;
      let remainingBase = eligible.reduce((s, x) => s + x.price * x.SoLuong, 0);
      for (const item of items) {
        let reduction = 0;
        if (eligible.includes(item) && remainingBase) {
          reduction = Math.min(
            remaining,
            Math.round((remaining * item.price * item.SoLuong) / remainingBase),
          );
          remaining -= reduction;
          remainingBase -= item.price * item.SoLuong;
        }
        const line = await repository.addOrderLine(
          [
            id,
            item.BienTheID,
            item.MaSanPham,
            item.SKU,
            item.TenSanPham,
            item.size,
            item.color,
            item.TenDanhMuc,
            item.TenThuongHieu,
            item.SoLuong,
            item.price,
            reduction,
          ],
          c,
        );
        await repository.allocateStock([line.insertId, item.SoLuong], c);
        await stockLog(
          c,
          r.user,
          item.BienTheID,
          'GIU_HANG',
          0,
          item.SoLuong,
          line.insertId,
          null,
          'Phân bổ khi đặt hàng',
        );
      }
      if (promo)
        await repository.recordPromotion(
          [
            id,
            promo.KhuyenMaiID,
            promo.MaKhuyenMai,
            promo.TenKhuyenMai,
            promo.HinhThuc,
            promo.GiaTri,
            discount,
          ],
          c,
        );
      await repository.recordHistory([id, r.user.id], c);
      await repository.createPayment([id, method.id, randomUUID(), total - discount + shipping], c);
      await repository.emptyCart([cart], c);
      return { id, total: total - discount + shipping };
    });
  }
}
