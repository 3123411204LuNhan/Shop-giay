import { required } from '../input.js';
import { InventoryRepository } from './InventoryRepository.js';

import { tx, fail, positive } from '../db.js';
import { audit } from '../auth.js';
import { stockLog } from '../operations.js';
import { saveReceipt, requestKey } from '../operations.js';
const repository = new InventoryRepository();

// InventoryController chuyển yêu cầu tới các phương thức xử lý phiếu kho, ngưỡng cảnh báo và lịch sử tồn.
export class InventoryService {
  async stock(r) {
    const {productId}=r.query;
    return repository.stock(productId==null?undefined:positive(productId));
  }

  async rejectDirectAdjustment() {
    return fail('Vui lòng lập phiếu kho nháp rồi xác nhận tại mục Phiếu kho.', 409);
  }

  async receipts() {
    return await repository.receipts();
  }

  async receipt(r) {
    const [p] = await repository.receipt([positive(r.params.id)]);
    if (!p) fail('Không tìm thấy phiếu.', 404);
    p.items = await repository.lines([p.id]);
    return p;
  }

  async createReceipt(r) {
    return await tx(async (c) => {
      // Account lock serializes duplicate submissions before checking their unique request key.
      await repository.lockActor([r.user.id], c);
      const [old] = await repository.findByKey([requestKey(r.body.requestId)], c);
      if (old) {
        if (old.owner !== r.user.id) fail('Mã yêu cầu đã được sử dụng.', 409);
        return { id: old.id };
      }
      return saveReceipt(c, r.user, r.body);
    });
  }

  async updateReceipt(r) {
    return await tx(async (c) => {
      const [p] = await repository.lockReceipt([positive(r.params.id)], c);
      if (!p) fail('Không tìm thấy phiếu.', 404);
      if (p.TrangThai !== 'NHAP')
        fail('Chỉ được sửa phiếu nháp. Phiếu đã xác nhận cần lập phiếu điều chỉnh.');
      return saveReceipt(c, r.user, r.body, p.PhieuKhoID);
    });
  }

  async confirmReceipt(r) {
    return await tx(async (c) => {
      const [p] = await repository.lockReceipt([positive(r.params.id)], c);
      if (!p) fail('Không tìm thấy phiếu.', 404);
      if (p.TrangThai === 'DA_XAC_NHAN') return { ok: true };
      if (p.TrangThai !== 'NHAP') fail('Phiếu đã hủy không thể xác nhận.');
      const lines = await repository.lockLines([p.PhieuKhoID], c);
      if (!lines.length) fail('Phiếu không có dòng hàng.');
      for (const line of lines) {
        if(line.TonSoSach!=null){
          const [stock]=await repository.lockStock([line.BienTheID],c);
          if(stock?.stock!==line.TonSoSach)fail('Tồn sổ đã thay đổi. Hãy kiểm đếm và lưu lại phiếu nháp.',409);
        }
        await stockLog(
          c,
          r.user,
          line.BienTheID,
          p.LoaiPhieu,
          line.SoLuongThayDoi,
          0,
          null,
          line.ChiTietPhieuKhoID,
          p.LyDo,
        );
      }
      await repository.confirmReceipt([r.user.id, p.PhieuKhoID], c);
      await audit(r.user, 'XAC_NHAN_PHIEU_KHO', 'PhieuKho', p.PhieuKhoID, p.LyDo, c);
      return { ok: true };
    });
  }

  async cancelReceipt(r) {
    return await tx(async (c) => {
      const [p] = await repository.lockReceipt([positive(r.params.id)], c);
      if (!p) fail('Không tìm thấy phiếu.', 404);
      if (p.TrangThai === 'DA_HUY') return { ok: true };
      if (p.TrangThai !== 'NHAP') fail('Không hủy phiếu đã ghi sổ; hãy lập phiếu điều chỉnh.');
      await repository.cancelReceipt([p.PhieuKhoID], c);
      await audit(
        r.user,
        'HUY_NHAP_KHO',
        'PhieuKho',
        p.PhieuKhoID,
        required(r.body.reason, 'lý do hủy'),
        c,
      );
      return { ok: true };
    });
  }

  async setWarning(r) {
    const value = Number(r.body.warning);
    if (!Number.isSafeInteger(value) || value < 0 || value > 100000)
      fail('Ngưỡng cảnh báo không hợp lệ.');
    await tx(async (c) => {
      const a = await repository.setThreshold([value, positive(r.params.id)], c);
      if (!a.affectedRows) fail('Không tìm thấy tồn kho.', 404);
      await audit(r.user, 'NGUONG_CANH_BAO', 'TonKho', r.params.id, String(value), c);
    });
    return { ok: true };
  }

  async stockHistory(r) {
    return await repository.history(r.query.variantId ? [positive(r.query.variantId)] : []);
  }
}
