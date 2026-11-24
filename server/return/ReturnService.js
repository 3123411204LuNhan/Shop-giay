import { withImageUpload } from '../image-upload.js';
import { productImage, required, code } from '../input.js';
import { ReturnRepository } from './ReturnRepository.js';
import { CheckoutRepository } from '../checkout/CheckoutRepository.js';
import { OrderRepository } from '../order/OrderRepository.js';
import { randomUUID } from 'node:crypto';
import { tx, fail, positive, text } from '../db.js';
import { audit } from '../auth.js';
import { storeConfig } from '../store-config.js';
import { stockLog, prepareReturnRefunds } from '../operations.js';
const repository = new ReturnRepository();
const orders = new OrderRepository();
const checkout = new CheckoutRepository();

async function eligibleLine(itemId,customerId,connection) {
  const [line]=await repository.lockEligibleLine([itemId,customerId],connection);
  if(!line)fail('Đơn chưa đủ điều kiện đổi/trả.');
  if(!line.NgayGiao||Date.now()-new Date(line.NgayGiao).getTime()>storeConfig.returnDays*86400000)
    fail('Đã hết thời hạn '+storeConfig.returnDays+' ngày gửi yêu cầu đổi/trả.');
  const [{n}]=await repository.requestedQuantity([itemId],connection);
  return {line,remaining:line.SoLuong-Number(n)};
}

// Mục 5.3: nhận hàng trước, đơn đổi giá trị thu bằng 0 chỉ tạo khi đủ tiền đơn gốc và đủ tồn.
async function createReplacement(connection,user,request,original,line) {
  const [{paid}]=await orders.confirmedTotal([request.DonHangID],connection);
  if(paid<original.TongThanhToan)return 'Chờ đối soát đủ tiền đơn gốc trước khi tạo đơn thay thế.';
  const [target]=await repository.exchangeOptions(line.BienTheID,line.DonGia,line.BienTheDoiSangID,connection);
  if(!target||target.stock<line.SoLuongDuyet)
    return 'Biến thể thay thế thiếu tồn hoặc không còn đủ điều kiện cùng giá. Thử lại khi đủ điều kiện.';
  const value=line.SoLuongDuyet*line.DonGia;
  const orderCode=code('DOI');
  const {insertId:id}=await checkout.createOrder([
    orderCode,randomUUID(),original.KhachHangID,original.HoTenNguoiNhan,original.SoDienThoaiNhan,
    original.DiaChiNhan,'Đơn thay thế cho '+request.MaYeuCau,original.PhuongThucID,value,value,0
  ],connection);
  const {insertId:itemId}=await checkout.addOrderLine([
    id,target.id,line.MaSanPhamLucMua,target.sku,line.TenSanPhamLucMua,target.size,target.color,
    line.DanhMucLucMua,line.ThuongHieuLucMua,line.SoLuongDuyet,line.DonGia,value
  ],connection);
  await checkout.allocateStock([itemId,line.SoLuongDuyet],connection);
  await stockLog(connection,user,target.id,'GIU_HANG',0,line.SoLuongDuyet,itemId,null,'Giữ hàng đổi '+request.MaYeuCau);
  await orders.appendHistory([id,null,'CHO_XAC_NHAN',user.id,'Đơn thay thế cho '+request.MaYeuCau],connection);
  await repository.linkReplacement([id,request.YeuCauDoiTraID],connection);
  return null;
}

function receivedStock(lines,body){
  // Một lần nhận đủ hàng đã duyệt. Chỉ số đôi đạt kiểm tra mới được nhập lại kho.
  if(body.items===undefined&&typeof body.sellable==='boolean')
    return lines.map(line=>body.sellable?line.SoLuongDuyet:0);
  if(!Array.isArray(body.items)||body.items.length!==lines.length)fail('Hãy nhập kết quả kiểm hàng cho từng dòng.');
  const items=new Map(body.items.map(item=>[Number(item?.id),item]));
  return lines.map(line=>{
    const item=items.get(line.ChiTietDoiTraID);
    if(!item||item.received==null||Number(item.received)!==line.SoLuongDuyet)fail('Chỉ xác nhận khi đã nhận đủ hàng được duyệt.');
    const sellable=Number(item.sellable);
    if(String(item.sellable??'').trim()===''||!Number.isSafeInteger(sellable)||sellable<0||sellable>line.SoLuongDuyet)
      fail('Số đôi bán lại phải từ 0 đến số đã nhận.');
    return sellable;
  });
}

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

  async options(r) {
    return tx(async c=>{
      const {line,remaining}=await eligibleLine(positive(r.params.itemId),r.user.id,c);
      const variants=remaining>0?await repository.exchangeOptions(line.BienTheID,line.DonGia,null,c):[];
      return {remaining,variants};
    });
  }

  async requestReturn(r) {
    return withImageUpload(r.imageFile,'returns',save=>tx(async (c) => {
      const item = positive(r.body.itemId),
        qty = positive(r.body.quantity);
      const requestId = r.body.requestId || randomUUID();
      if (!/^[a-f\d-]{36}$/i.test(requestId)) fail('Mã chống lặp không hợp lệ.');
      await repository.lockCustomer([r.user.id], c);
      const [old] = await repository.findByKey([requestId], c);
      if (old) {
        if (old.owner !== r.user.id) fail('Mã yêu cầu đã sử dụng.', 409);
        return { ok:true,id:old.id,code:old.code,status:old.status };
      }
      const {line:l,remaining}=await eligibleLine(item,r.user.id,c);
      if(qty>remaining)fail('Số lượng vượt phần còn được đổi/trả.');
      const {type}=r.body;
      if(!['TRA','DOI'].includes(type))fail('Loại yêu cầu không hợp lệ.');
      const targetId=type==='DOI'?positive(r.body.targetVariantId,'Biến thể đổi'):null;
      if(targetId!=null){
        const [target]=await repository.exchangeOptions(l.BienTheID,l.DonGia,targetId,c);
        if(!target)fail('Chọn biến thể khác cùng sản phẩm, cùng đơn giá lúc mua.');
        if(target.stock<qty)fail('Biến thể đổi không đủ hàng.',409);
      }
      const requestCode=code('DT');
      const a = await repository.createRequest(
        [requestCode, requestId, l.DonHangID, r.user.id, r.body.type, required(r.body.reason, 'lý do')],
        c,
      );
      await repository.addLine([a.insertId, l.DonHangID, item, qty, targetId], c);
      let image=productImage(r.body.image);
      if (image && !/^https?:\/\//i.test(image)) fail('Ảnh bằng chứng phải là URL http/https.');
      if (r.imageFile) image='/api/returns/'+a.insertId+'/images/'+await save();
      if (image) await repository.attachImage([a.insertId,image],c);
      await repository.recordSubmission([a.insertId, r.user.id], c);
      return {ok:true,id:a.insertId,code:requestCode,status:'CHO_DUYET'};
    }));
  }

  async detail(r) {
    return this.#readDetail(positive(r.params.id),r.user.id);
  }

  async managedDetail(r) {
    const detail=await this.#readDetail(positive(r.params.id));
    for (const image of detail.images) {
      if (image.url.startsWith('/api/returns/')) image.url=image.url.replace('/api/returns/','/api/manage/returns/');
    }
    return detail;
  }

  async requireEvidence(r,managed) {
    const id=positive(r.params.id);
    const url='/api/returns/'+id+'/images/'+r.params.name;
    const [image]=await repository.requestImage(id,url,managed?undefined:r.user.id);
    if (!image) fail('Không tìm thấy ảnh.',404);
  }

  // Mục 3.7: chủ shop đọc đơn gốc, dòng hàng và bằng chứng trước khi quyết định.
  async #readDetail(id,customerId) {
    const [y] = await repository.requestDetails(id,customerId);
    if (!y) fail('Không tìm thấy yêu cầu.', 404);
    y.items = await repository.lines([y.id]);
    y.history = await repository.history([y.id]);
    y.refunds = await repository.refunds([y.id]);
    y.images = await repository.images([y.id]);
    return y;
  }

  async listAll() {
    return await repository.all();
  }

  async processReturn(r) {
    return tx(async (c) => {
      const id = positive(r.params.id),
        action = r.body.action;
      const [y] = await repository.lockRequest([id], c);
      if (!y) fail('Không tìm thấy yêu cầu.',404);
      const exchange=y.LoaiYeuCau==='DOI';
      const [original]=exchange?await orders.lockOrder([y.DonHangID],c):[];
      const lines = await repository.lockLines([id], c);
      if(!lines.length||(exchange&&lines.length!==1))fail('Yêu cầu có dòng hàng không hợp lệ.',409);
      let status, pending;
      if(exchange&&y.DonHangDoiMoiID&&['create-exchange','receive'].includes(action))return {ok:true};
      if (['approve', 'reject'].includes(action)) {
        if (y.TrangThai !== 'CHO_DUYET') fail('Yêu cầu đã được duyệt.');
        status = action === 'approve' ? 'DA_DUYET' : 'TU_CHOI';
        if (action === 'approve')
          for (const l of lines) {
            // Phân bổ theo tổng số đã duyệt để lần trả cuối nhận đủ phần tiền lẻ.
            if(exchange){await repository.approveLine([0,l.ChiTietDoiTraID],c);continue;}
            const [shares]=await repository.approvedReturnShares([l.ChiTietDonHangID,id],c);
            const quantity=Number(shares.quantity)+l.SoLuongYeuCau;
            const allocated=quantity===l.bought?l.paid:Math.floor(l.paid*quantity/l.bought);
            const refund=Math.max(0,Math.round((allocated-Number(shares.amount))*100)/100);
            await repository.approveLine([refund,l.ChiTietDoiTraID],c);
          }
        await repository.recordDecision([r.user.id, required(r.body.reason, 'lý do xử lý'), id], c);
      } else if (action === 'receive') {
        if (y.TrangThai !== 'DA_DUYET') fail('Chỉ nhận hàng một lần sau khi duyệt.');
        status = 'DANG_XU_LY';
        const counts=receivedStock(lines,r.body);
        for (const [index,l] of lines.entries()) {
          const sellable = counts[index];
          await repository.receiveLine([sellable, r.user.id, text(r.body.reason), l.ChiTietDoiTraID], c);
          if (sellable) {
            const a = await repository.createReceipt(
              [code('HT'), randomUUID(), 'Nhận hàng trả ' + y.MaYeuCau, r.user.id, r.user.id],
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
        if(exchange)pending=await createReplacement(c,r.user,y,original,lines[0]);
        else await prepareReturnRefunds(c, id, y.DonHangID, y.LyDo);
      } else if(action==='create-exchange'){
        if(!exchange||y.TrangThai!=='DANG_XU_LY')fail('Cần nhận đủ hàng đổi trước khi tạo đơn thay thế.');
        pending=await createReplacement(c,r.user,y,original,lines[0]);
        if(pending)fail(pending,409);
        status='DANG_XU_LY';
      } else if (action === 'prepare-refund') {
        if(exchange)fail('Yêu cầu đổi hàng dùng đơn thay thế.');
        if (y.TrangThai !== 'DANG_XU_LY') fail('Cần nhận hàng trước khi lập khoản hoàn.');
        await prepareReturnRefunds(c, id, y.DonHangID, y.LyDo);
        status = 'DANG_XU_LY';
      } else if (action === 'complete') {
        if (y.TrangThai !== 'DANG_XU_LY') fail('Cần nhận và kiểm tra hàng trước.');
        if(exchange&&!y.DonHangDoiMoiID)fail('Cần tạo đơn thay thế trước khi hoàn tất.');
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
        [id, y.TrangThai, status, r.user.id, pending || (action==='create-exchange'?'Đã tạo đơn thay thế':text(r.body.reason)||action)],
        c,
      );
      await audit(r.user, 'XU_LY_DOI_TRA', 'YeuCauDoiTra', id, action, c);
      return {ok:true,...(pending?{message:pending}:{})};
    });
  }

  async refunds() {
    return await repository.allRefunds();
  }

  async confirmRefund(r) {
    await tx(async (c) => {
      const [h] = await repository.lockRefund([positive(r.params.id)], c);
      if (!h) fail('Không có khoản hoàn.',404);
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
