import { productImage, required, code } from '../input.js';
import { ReturnRepository } from './ReturnRepository.js';
import { randomUUID } from 'node:crypto';
import { tx, fail, positive, text } from '../db.js';
import { audit } from '../auth.js';
import { storeConfig } from '../store-config.js';
import { stockLog, prepareReturnRefunds } from '../operations.js';
const repository = new ReturnRepository();

// ReturnController chuyển yêu cầu tới các phương thức xử lý đánh giá, yêu cầu trả hàng và hoàn tiền.
export class ReturnService {
  async review(r) {
    const stars = Number(r.body.stars);
    if (!Number.isInteger(stars) || stars < 1 || stars > 5) fail('Số sao từ 1 đến 5.');
    const [line] = await repository.reviewableLine([positive(r.body.itemId), r.user.id]);
    if (!line) fail('Chỉ đánh giá sản phẩm trong đơn đã hoàn thành.');
    await repository.insertReview([
      line.DonHangID,
      r.body.itemId,
      r.user.id,
      stars,
      text(r.body.content, 2000),
    ]);
    return { ok: true };
  }

  async listMine(r) {
    return await repository.forCustomer([r.user.id]);
  }

  async requestReturn(r) {
    await tx(async (c) => {
      const item = positive(r.body.itemId),
        qty = positive(r.body.quantity);
      const requestId = r.body.requestId || randomUUID();
      if (!/^[a-f\d-]{36}$/i.test(requestId)) fail('Mã chống lặp không hợp lệ.');
      await repository.lockCustomer([r.user.id], c);
      const [old] = await repository.findByKey([requestId], c);
      if (old) {
        if (old.owner !== r.user.id) fail('Mã yêu cầu đã sử dụng.', 409);
        return { id: old.id };
      }
      const [l] = await repository.lockEligibleLine([item, r.user.id], c);
      if (!l) fail('Đơn chưa đủ điều kiện đổi/trả.');
      if (!l.NgayGiao || Date.now() - new Date(l.NgayGiao).getTime() > storeConfig.returnDays * 86400000)
        fail('Đã hết thời hạn ' + storeConfig.returnDays + ' ngày gửi yêu cầu trả hàng.');
      const [{ n }] = await repository.requestedQuantity([item], c);
      if (Number(n) + qty > l.SoLuong) fail('Số lượng vượt quá đơn.');
      if (r.body.type !== 'TRA') fail('Đổi size được xử lý bằng trả hàng, hoàn tiền và đặt đơn mới.');
      const a = await repository.createRequest(
        [code('DT'), requestId, l.DonHangID, r.user.id, r.body.type, required(r.body.reason, 'lý do')],
        c,
      );
      await repository.addLine([a.insertId, l.DonHangID, item, qty], c);
      if (r.body.image) {
        if (!/^https?:\/\//.test(r.body.image)) fail('Ảnh bằng chứng phải là URL http/https.');
        await repository.attachImage([a.insertId, productImage(r.body.image)], c);
      }
      await repository.recordSubmission([a.insertId, r.user.id], c);
    });
    return { ok: true };
  }

  async detail(r) {
    const [y] = await repository.ownedRequest([positive(r.params.id), r.user.id]);
    if (!y) fail('Không tìm thấy yêu cầu.', 404);
    y.items = await repository.lines([y.id]);
    y.history = await repository.history([y.id]);
    y.refunds = await repository.refunds([y.id]);
    return y;
  }

  async listAll() {
    return await repository.all();
  }

  async processReturn(r) {
    await tx(async (c) => {
      const id = positive(r.params.id),
        action = r.body.action;
      const [y] = await repository.lockRequest([id], c);
      if (!y) fail('Không có yêu cầu.');
      const lines = await repository.lockLines([id], c);
      let status;
      if (['approve', 'reject'].includes(action)) {
        if (y.TrangThai !== 'CHO_DUYET') fail('Yêu cầu đã được duyệt.');
        status = action === 'approve' ? 'DA_DUYET' : 'TU_CHOI';
        if (action === 'approve')
          for (const l of lines) {
            await repository.approveLine(
              [Math.floor((l.paid * l.SoLuongYeuCau) / l.bought), l.ChiTietDoiTraID],
              c,
            );
          }
        await repository.recordDecision([r.user.id, required(r.body.reason, 'lý do xử lý'), id], c);
      } else if (action === 'receive') {
        if (y.TrangThai !== 'DA_DUYET') fail('Chỉ nhận hàng một lần sau khi duyệt.');
        status = 'DANG_XU_LY';
        for (const l of lines) {
          const sellable = r.body.sellable ? l.SoLuongDuyet : 0;
          await repository.receiveLine([sellable, r.user.id, text(r.body.reason), l.ChiTietDoiTraID], c);
          if (sellable) {
            const a = await repository.createReceipt(
              [code('HT'), randomUUID(), 'Nhận hàng trả ' + y.MaYeuCau, r.user.id],
              c,
            );
            const p = await repository.addReceiptLine(
              [a.insertId, l.BienTheID, sellable, l.ChiTietDoiTraID],
              c,
            );
            await stockLog(
              c,
              r.user,
              l.BienTheID,
              'NHAN_TRA',
              sellable,
              0,
              null,
              p.insertId,
              'Hàng trả đủ điều kiện bán lại',
            );
          }
        }
        await prepareReturnRefunds(c, id, y.DonHangID, y.LyDo);
      } else if (action === 'prepare-refund') {
        if (y.TrangThai !== 'DANG_XU_LY') fail('Cần nhận hàng trước khi lập khoản hoàn.');
        await prepareReturnRefunds(c, id, y.DonHangID, y.LyDo);
        status = 'DANG_XU_LY';
      } else if (action === 'complete') {
        if (y.TrangThai !== 'DANG_XU_LY') fail('Cần nhận và kiểm tra hàng trước.');
        const [{ n }] = await repository.pendingRefunds([id], c);
        if (n) fail('Cần xác nhận hoàn tiền trước khi hoàn tất.');
        const [{ owed }] = await repository.approvedTotal([id], c);
        const [{ refunded }] = await repository.confirmedRefunds([id], c);
        if (refunded < owed)
          fail('Chưa hoàn đủ tiền được duyệt. Đối soát khoản thu rồi lập bổ sung khoản hoàn.');
        status = 'HOAN_TAT';
        await repository.markComplete([id], c);
      } else fail('Thao tác không hợp lệ.');
      await repository.setStatus([status, id], c);
      await repository.recordHistory(
        [id, y.TrangThai, status, r.user.id, text(r.body.reason) || action],
        c,
      );
      await audit(r.user, 'XU_LY_DOI_TRA', 'YeuCauDoiTra', id, action, c);
    });
    return { ok: true };
  }

  async refunds() {
    return await repository.allRefunds();
  }

  async confirmRefund(r) {
    await tx(async (c) => {
      const [h] = await repository.lockRefund([positive(r.params.id)], c);
      if (!h) fail('Không có khoản hoàn.');
      if (h.TrangThai === 'DA_XAC_NHAN') return;
      if (!['CHO_XU_LY', 'DANG_XU_LY'].includes(h.TrangThai))
        fail('Khoản hoàn cần ở trạng thái chờ; hãy thử lại nếu đã thất bại.');
      const [payment] = await repository.lockPayment([h.ThanhToanID], c);
      const [{ returned }] = await repository.refundedForPayment([h.ThanhToanID], c);
      if (!payment || payment.TrangThai !== 'DA_XAC_NHAN' || returned + h.SoTien > payment.SoTien)
        fail('Tổng hoàn vượt tiền thực thu.');
      await repository.confirmRefund(
        [r.user.id, required(r.body.proof, 'bằng chứng hoàn tiền'), h.HoanTienID],
        c,
      );
      await audit(r.user, 'XAC_NHAN_HOAN', 'HoanTien', h.HoanTienID, 'Xác nhận có bằng chứng', c);
    });
    return { ok: true };
  }

  async retryRefund(r) {
    return await tx(async (c) => {
      const [h] = await repository.lockRefund([positive(r.params.id)], c);
      if (!h) fail('Không có khoản hoàn.', 404);
      if (h.TrangThai === 'CHO_XU_LY') return { ok: true };
      if (h.TrangThai !== 'THAT_BAI') fail('Chỉ thử lại khoản hoàn đã thất bại.');
      await repository.retryRefund([h.HoanTienID], c);
      await audit(
        r.user,
        'THU_LAI_HOAN',
        'HoanTien',
        h.HoanTienID,
        required(r.body.reason, 'lý do thử lại'),
        c,
      );
      return { ok: true };
    });
  }

  async markRefundFailed(r) {
    return await tx(async (c) => {
      const [h] = await repository.lockRefund([positive(r.params.id)], c);
      if (!h) fail('Không có khoản hoàn.', 404);
      if (h.TrangThai === 'THAT_BAI') return { ok: true };
      if (!['CHO_XU_LY', 'DANG_XU_LY'].includes(h.TrangThai))
        fail('Khoản hoàn không còn ở trạng thái chờ.');
      await repository.failRefund([h.HoanTienID], c);
      await audit(
        r.user,
        'HOAN_THAT_BAI',
        'HoanTien',
        h.HoanTienID,
        required(r.body.reason, 'lý do thất bại'),
        c,
      );
      return { ok: true };
    });
  }
}
