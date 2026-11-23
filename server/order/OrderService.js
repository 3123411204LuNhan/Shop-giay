import { required } from '../input.js';
import { OrderRepository } from './OrderRepository.js';
import { randomUUID } from 'node:crypto';
import { tx, fail, positive, text } from '../db.js';
import { audit } from '../auth.js';
import { publicStoreConfig } from '../store-config.js';
import { stockLog, reserveRefunds } from '../operations.js';
import { operationCode } from '../operations.js';
const repository = new OrderRepository();

async function orderDetails(id) {
  const order = (await repository.findById([id]))[0];
  if (!order) fail('Không tìm thấy đơn.', 404);
  order.items = await repository.items([id]);
  order.history = await repository.history([id]);
  order.refunds = await repository.refunds([id]);
  order.settings = publicStoreConfig();
  order.payments = await repository.payments([id]);
  return order;
}

async function cancelOrder(c, user, id, reason, owner) {
  const [o] = await repository.lockOrder([id], c);
  if (!o || (owner && o.KhachHangID !== user.id)) fail('Không tìm thấy đơn.', 404);
  if (o.TrangThai === 'DA_HUY') return;
  if (!['CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DANG_CHUAN_BI'].includes(o.TrangThai))
    fail('Đơn đang giao hoặc đã hoàn tất, không thể hủy.');
  const lines = await repository.allocations([id], c);
  for (const l of lines) {
    await stockLog(
      c,
      user,
      l.BienTheID,
      'GIAI_PHONG',
      0,
      -l.SoLuongGiu,
      l.ChiTietDonHangID,
      null,
      reason,
    );
    await repository.releaseAllocation([l.ChiTietDonHangID], c);
  }
  await repository.cancelOrder([user.id, reason, id], c);
  await repository.recordCancellation([id, o.TrangThai, user.id, reason], c);
  await repository.cancelPendingPayments([id], c);
  await reserveRefunds(c, user, id, reason);
  await audit(user, 'HUY_DON', 'DonHang', id, reason, c);
}
let expiring = false;
export async function expireUnpaidOrders() {
  if (expiring) return 0;
  expiring = true;
  let count = 0;
  try {
    const ids = await repository.expiredOrders();
    for (const row of ids)
      await tx(async (c) => {
        const [o] = await repository.lockOrder([row.id], c);
        if (
          !o ||
          !['CHO_XAC_NHAN', 'DA_XAC_NHAN', 'DANG_CHUAN_BI'].includes(o.TrangThai) ||
          !o.HanThanhToan ||
          new Date(o.HanThanhToan) > new Date()
        )
          return;
        const [{ paid }] = await repository.confirmedTotal([row.id], c);
        if (paid > 0) return;
        await cancelOrder(c, { id: null }, row.id, 'Hệ thống: hết thời hạn chờ thanh toán', false);
        await repository.expirePayment([row.id], c);
        count++;
      });
    return count;
  } finally {
    expiring = false;
  }
}

// OrderController chuyển yêu cầu tới các phương thức xử lý tra cứu đơn, hủy đơn, giao hàng và đối soát.
export class OrderService {
  async listMine(r) {
    return await repository.forCustomer([r.user.id]);
  }

  async details(r) {
    const o = await orderDetails(positive(r.params.id));
    if (o.customerId !== r.user.id && !r.user.roles.includes('SHOP_MANAGER'))
      fail('Không có quyền xem đơn.', 403);
    return o;
  }

  async cancel(r) {
    await tx((c) =>
      cancelOrder(c, r.user, positive(r.params.id), text(r.body.reason) || 'Khách hủy đơn', true),
    );
    return { ok: true };
  }

  async listAll() {
    return await repository.all();
  }

  async advance(r) {
    await tx(async (c) => {
      const id = positive(r.params.id);
      if (r.body.status === 'DA_HUY')
        return cancelOrder(c, r.user, id, required(r.body.reason, 'lý do hủy'), false);
      const [o] = await repository.lockOrder([id], c);
      if (!o) fail('Không có đơn.', 404);
      const sequence = [
        'CHO_XAC_NHAN',
        'DA_XAC_NHAN',
        'DANG_CHUAN_BI',
        'DANG_GIAO',
        'DA_GIAO',
        'HOAN_THANH',
      ];
      if (sequence[sequence.indexOf(o.TrangThai) + 1] !== r.body.status)
        fail('Trạng thái chuyển tiếp không hợp lệ.');
      const [method] = await repository.paymentMethod([o.PhuongThucID], c);
      if (method.code !== 'COD') {
        const [{ paid }] = await repository.confirmedTotal([id], c);
        if (paid < o.TongThanhToan)
          fail('Cần đối soát thu đủ tiền chuyển khoản trước khi xác nhận/xử lý đơn.');
      }
      if (r.body.status === 'DA_GIAO' && ['GIAO_THAT_BAI', 'CHO_HOAN_VE'].includes(o.TrangThaiGiaoHang))
        fail('Cần xử lý giao thất bại trước khi xác nhận đã giao.');
      if (r.body.status === 'DANG_GIAO') await repository.markShipping([id], c);
      if (r.body.status === 'DA_GIAO') await repository.markDelivered([id], c);
      if (r.body.status === 'DANG_GIAO') {
        const lines = await repository.heldItems([id], c);
        for (const l of lines) {
          await stockLog(
            c,
            r.user,
            l.BienTheID,
            'BAN_HANG',
            -l.SoLuongGiu,
            -l.SoLuongGiu,
            l.ChiTietDonHangID,
            null,
            'Xuất giao đơn',
          );
          await repository.releaseShipped([l.ChiTietDonHangID], c);
        }
      }
      if (r.body.status === 'HOAN_THANH') {
        const [{ paid }] = await repository.confirmedTotal([id], c);
        if (paid < o.TongThanhToan) fail('Cần xác nhận thu đủ tiền trước khi hoàn thành đơn.');
      }
      await repository.updateStatus([r.body.status, r.body.status, r.body.status, id], c);
      await repository.appendHistory(
        [id, o.TrangThai, r.body.status, r.user.id, text(r.body.reason)],
        c,
      );
      await audit(r.user, 'CHUYEN_TRANG_THAI', 'DonHang', id, r.body.status, c);
    });
    return { ok: true };
  }

  async confirmPayment(r) {
    await tx(async (c) => {
      const [lookup] = await repository.orderForPayment([positive(r.params.id)], c);
      if (!lookup) fail('Không có giao dịch.');
      const [d] = await repository.lockOrderWithMethod([lookup.DonHangID], c);
      const [p] = await repository.lockPayment([positive(r.params.id)], c);
      if (p.TrangThai === 'DA_XAC_NHAN') return;
      if (
        d.TrangThai === 'DA_HUY' ||
        p.TrangThai !== 'CHO_XAC_NHAN' ||
        (d.MaPhuongThuc === 'COD' && !['DA_GIAO', 'HOAN_THANH'].includes(d.TrangThai))
      )
        fail('Chỉ xác nhận COD sau khi giao thành công; đơn hủy cần đối soát tiền đến muộn.');
      if (d.HanThanhToan && new Date(d.HanThanhToan) <= new Date())
        fail('Đã hết hạn thanh toán. Vui lòng tải lại đơn và xử lý tiền đến muộn nếu đã thực nhận.');
      const [{ paid }] = await repository.confirmedTotal([d.DonHangID], c);
      if (paid + p.SoTien > d.TongThanhToan) fail('Khoản thu vượt giá trị đơn.');
      const proof = required(r.body.proof, 'bằng chứng thu tiền');
      await repository.confirmPayment([r.user.id, proof, p.ThanhToanID], c);
      await repository.clearDeadline([d.DonHangID], c);
      await audit(r.user, 'XAC_NHAN_THU', 'ThanhToan', p.ThanhToanID, proof, c);
    });
    return { ok: true };
  }

  async customerHistory(r) {
    return await repository.forCustomer([positive(r.params.id)]);
  }

  async updateDelivery(r) {
    return await tx(async (c) => {
      const [o] = await repository.lockOrder([positive(r.params.id)], c);
      if (!o) fail('Không có đơn.', 404);
      if (o.TrangThai !== 'DANG_GIAO') fail('Chỉ xử lý vận chuyển cho đơn đang giao.');
      const reason = required(r.body.reason, 'ghi chú vận chuyển');
      let status = o.TrangThaiGiaoHang;
      if (r.body.action === 'failed' && ['DANG_GIAO', 'CHUA_GIAO'].includes(status))
        status = 'GIAO_THAT_BAI';
      else if (r.body.action === 'retry' && status === 'GIAO_THAT_BAI') status = 'DANG_GIAO';
      else if (r.body.action === 'await-return' && status === 'GIAO_THAT_BAI') status = 'CHO_HOAN_VE';
      else if (r.body.action !== 'tracking') fail('Thao tác vận chuyển không phù hợp trạng thái.');
      await repository.updateDelivery(
        [status, text(r.body.carrier, 150) || null, text(r.body.tracking, 100) || null, o.DonHangID],
        c,
      );
      await repository.recordDelivery(
        [o.DonHangID, o.TrangThai, o.TrangThai, r.user.id, `Vận chuyển ${status}: ${reason}`],
        c,
      );
      await audit(r.user, 'VAN_CHUYEN', 'DonHang', o.DonHangID, reason, c);
      return { ok: true };
    });
  }

  async receiveDeliveryReturn(r) {
    return await tx(async (c) => {
      const [o] = await repository.lockOrder([positive(r.params.id)], c);
      if (!o) fail('Không có đơn.', 404);
      if (o.TrangThaiGiaoHang === 'DA_NHAN_HOAN') return { ok: true };
      if (o.TrangThai !== 'DANG_GIAO' || o.TrangThaiGiaoHang !== 'CHO_HOAN_VE')
        fail('Đơn phải được đánh dấu chờ hoàn về trước khi nhận.');
      const reason = required(r.body.reason, 'kết quả kiểm hàng');
      const lines = await repository.lockItems([o.DonHangID], c);
      if (!Array.isArray(r.body.items) || r.body.items.length !== lines.length)
        fail('Cần kiểm đếm đầy đủ các dòng hàng hoàn về.');
      for (const l of lines) {
        const matches = r.body.items.filter((x) => Number(x.itemId) === l.ChiTietDonHangID);
        const input = matches[0];
        if (
          matches.length !== 1 ||
          Number(input.received) !== l.SoLuong ||
          !Number.isSafeInteger(Number(input.sellable)) ||
          Number(input.sellable) < 0 ||
          Number(input.sellable) > l.SoLuong
        )
          fail('Số lượng nhận/bán lại không hợp lệ. Chỉ xác nhận khi đã kiểm đủ toàn bộ kiện hàng.');
      }
      const a = await repository.createReturnReceipt(
        [operationCode('GH'), randomUUID(), reason, r.user.id, r.user.id, o.DonHangID],
        c,
      );
      for (const l of lines) {
        const qty = Number(r.body.items.find((x) => Number(x.itemId) === l.ChiTietDonHangID).sellable);
        if (qty) {
          const line = await repository.addReceiptLine(
            [a.insertId, l.BienTheID, qty, `Đã nhận ${l.SoLuong}, bán lại ${qty}. ${reason}`],
            c,
          );
          await stockLog(c, r.user, l.BienTheID, 'NHAN_TRA', qty, 0, null, line.insertId, reason);
        }
      }
      await repository.cancelReturnedOrder([r.user.id, reason, o.DonHangID], c);
      await repository.cancelPendingPayments([o.DonHangID], c);
      await reserveRefunds(c, r.user, o.DonHangID, 'Hoàn tiền đơn giao thất bại ' + o.MaDonHang);
      await repository.recordReturn(
        [o.DonHangID, r.user.id, 'Đã nhận hàng giao thất bại. ' + reason],
        c,
      );
      await audit(
        r.user,
        'NHAN_HOAN_GIAO',
        'DonHang',
        o.DonHangID,
        JSON.stringify({ reason, items: r.body.items }),
        c,
      );
      return { ok: true };
    });
  }

  async recordLatePayment(r) {
    return await tx(async (c) => {
      const [o] = await repository.lockLateOrder([positive(r.params.id)], c);
      if (!o || o.TrangThai !== 'DA_HUY' || o.method !== 'BANK_TRANSFER')
        fail('Chỉ đối soát chuyển khoản đến sau khi đơn bị hủy.');
      const [p] = await repository.lockLatestPayment([o.DonHangID], c);
      if (!p) fail('Không có giao dịch.');
      if (p.TrangThai === 'DA_XAC_NHAN') return { ok: true };
      const amount = Number(r.body.amount);
      if (!Number.isSafeInteger(amount) || amount <= 0 || amount > o.TongThanhToan)
        fail('Số tiền nhận phải dương và không vượt tổng đơn.');
      await repository.confirmLatePayment(
        [amount, r.user.id, required(r.body.proof, 'bằng chứng tiền vào'), p.ThanhToanID],
        c,
      );
      await reserveRefunds(c, r.user, o.DonHangID, 'Chuyển khoản đến sau khi đơn đã hủy');
      await audit(
        r.user,
        'THU_TIEN_DEN_MUON',
        'DonHang',
        o.DonHangID,
        'Đối soát và tạo khoản hoàn; không khôi phục đơn',
        c,
      );
      return { ok: true };
    });
  }
}
