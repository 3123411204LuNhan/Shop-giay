import {randomUUID} from 'node:crypto';
import {query as q,fail,positive,text} from './db.js';
import {audit} from './auth.js';

const required=(value,label)=>{const result=text(value);if(!result)fail(`Vui lòng nhập ${label}.`);return result;};
const code=prefix=>`${prefix}-${randomUUID()}`;
const requestKey=value=>{if(!/^[a-f\d-]{36}$/i.test(value||''))fail('Mã chống lặp không hợp lệ.');return value;};

// Các service đơn hàng, đổi trả và kho cùng gọi stockLog trong giao dịch đang mở để ghi tồn và sổ kho đồng thời.
export async function stockLog(c,user,variant,type,dt,dp,detailId,receiptId,reason) {
  const [stock]=await q('SELECT * FROM TonKho WHERE BienTheID=? FOR UPDATE',[variant],c);
  if(!stock||stock.SoLuongTon+dt<stock.SoLuongDaPhanBo+dp||stock.SoLuongDaPhanBo+dp<0)fail('Không đủ tồn khả dụng hoặc số phân bổ không hợp lệ.');
  await q('UPDATE TonKho SET SoLuongTon=SoLuongTon+?,SoLuongDaPhanBo=SoLuongDaPhanBo+?,NgayCapNhat=UTC_TIMESTAMP(),PhienBan=PhienBan+1 WHERE BienTheID=?',[dt,dp,variant],c);
  await q('INSERT INTO BienDongKho(BienTheID,LoaiBienDong,ThayDoiTon,ThayDoiPhanBo,TonSau,PhanBoSau,ChiTietDonHangID,ChiTietPhieuKhoID,NguoiThucHienID,MaThaoTac,LyDo) VALUES(?,?,?,?,?,?,?,?,?,?,?)',[variant,type,dt,dp,stock.SoLuongTon+dt,stock.SoLuongDaPhanBo+dp,detailId,receiptId,user.id||null,randomUUID(),reason],c);
}

export async function reserveRefunds(c,user,orderId,reason) {
  const payments=await q("SELECT t.*,COALESCE((SELECT SUM(h.SoTien) FROM HoanTien h WHERE h.ThanhToanID=t.ThanhToanID AND h.TrangThai<>'DA_HUY'),0) reserved FROM ThanhToan t WHERE t.DonHangID=? AND t.TrangThai='DA_XAC_NHAN' ORDER BY t.ThanhToanID FOR UPDATE",[orderId],c);
  for(const p of payments)if(p.SoTien>p.reserved)await q('INSERT INTO HoanTien(DonHangID,ThanhToanID,MaYeuCau,LyDo,SoTien) VALUES(?,?,?,?,?)',[orderId,p.ThanhToanID,randomUUID(),reason,p.SoTien-p.reserved],c);
}

export async function prepareReturnRefunds(c,returnId,orderId,reason) {
  const [{owed}]=await q('SELECT COALESCE(SUM(TienHoanDuocDuyet),0) owed FROM ChiTietDoiTra WHERE YeuCauDoiTraID=?',[returnId],c);
  const [{reserved}]=await q("SELECT COALESCE(SUM(SoTien),0) reserved FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai<>'DA_HUY'",[returnId],c);
  let remaining=owed-reserved;
  const payments=await q("SELECT t.*,COALESCE((SELECT SUM(h.SoTien) FROM HoanTien h WHERE h.ThanhToanID=t.ThanhToanID AND h.TrangThai<>'DA_HUY'),0) used FROM ThanhToan t WHERE t.DonHangID=? AND t.TrangThai='DA_XAC_NHAN' ORDER BY t.ThanhToanID FOR UPDATE",[orderId],c);
  for(const p of payments){const amount=Math.min(remaining,p.SoTien-p.used);if(amount>0){await q('INSERT INTO HoanTien(DonHangID,ThanhToanID,YeuCauDoiTraID,MaYeuCau,LyDo,SoTien) VALUES(?,?,?,?,?,?)',[orderId,p.ThanhToanID,returnId,randomUUID(),reason,amount],c);remaining-=amount;}}
  return {remaining};
}

async function receiptLines(c,body) {
  if(!['NHAP','DIEU_CHINH'].includes(body.type))fail('Loại phiếu không hợp lệ.');
  if(!Array.isArray(body.items)||!body.items.length||body.items.length>100)fail('Phiếu cần từ 1 đến 100 dòng hàng.');
  const seen=new Set();
  const lines=body.items.map(item=>{
    const variantId=positive(item.variantId),quantity=Number(item.quantity),cost=item.cost===''||item.cost==null?null:Number(item.cost);
    if(seen.has(variantId))fail('Biến thể bị lặp trong phiếu.');seen.add(variantId);
    if(!Number.isSafeInteger(quantity)||quantity===0||Math.abs(quantity)>100000||(body.type==='NHAP'&&quantity<0))fail('Số lượng nhập hoặc chênh lệch không hợp lệ.');
    if(cost!==null&&(!Number.isFinite(cost)||cost<0||cost>1e10))fail('Giá nhập không hợp lệ.');
    return {variantId,quantity,cost};
  }).sort((a,b)=>a.variantId-b.variantId);
  for(const line of lines){const [v]=await q('SELECT BienTheID FROM BienTheSanPham WHERE BienTheID=?',[line.variantId],c);if(!v)fail('Biến thể không tồn tại.');}
  return lines;
}

async function saveReceipt(c,user,body,id) {
  const reason=required(body.reason,'lý do'),reference=text(body.reference,100)||null;
  if(body.type==='NHAP'&&!reference)fail('Phiếu nhập cần mã chứng từ để tránh nhập trùng.');
  if(body.type==='NHAP'&&!text(body.supplier,200))fail('Vui lòng nhập nhà cung cấp.');
  const lines=await receiptLines(c,body);
  if(id){await q('UPDATE PhieuKho SET LoaiPhieu=?,MaChungTuThamChieu=?,LyDo=?,NhaCungCap=? WHERE PhieuKhoID=?',[body.type,reference,reason,text(body.supplier,200)||null,id],c);await q('DELETE FROM ChiTietPhieuKho WHERE PhieuKhoID=?',[id],c);}
  else {const result=await q("INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,MaChungTuThamChieu,LyDo,NguoiThucHienID,TrangThai,NhaCungCap) VALUES(?,?,?,?,?,?,'NHAP',?)",[code('PK'),requestKey(body.requestId),body.type,reference,reason,user.id,text(body.supplier,200)||null],c);id=result.insertId;}
  for(const line of lines)await q('INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,DonGiaNhap) VALUES(?,?,?,?)',[id,line.variantId,line.quantity,line.cost],c);
  await audit(user,'LUU_NHAP_KHO','PhieuKho',id,reason,c);return {id};
}

export { requestKey, code as operationCode, saveReceipt };
