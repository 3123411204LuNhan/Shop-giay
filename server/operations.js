import {randomUUID} from 'node:crypto';
import {query as q,tx,fail,positive,text} from './db.js';
import {audit} from './auth.js';

const required=(value,label)=>{const result=text(value);if(!result)fail(`Vui lòng nhập ${label}.`);return result;};
const code=prefix=>`${prefix}-${randomUUID()}`;
const requestKey=value=>{if(!/^[a-f\d-]{36}$/i.test(value||''))fail('Mã chống lặp không hợp lệ.');return value;};

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

export function installOperations(router,{manager,run}) {
  router.post('/manage/refunds/:id/retry',manager,run(async r=>await tx(async c=>{
    const [h]=await q('SELECT * FROM HoanTien WHERE HoanTienID=? FOR UPDATE',[positive(r.params.id)],c);if(!h)fail('Không có khoản hoàn.',404);
    if(h.TrangThai==='CHO_XU_LY')return {ok:true};if(h.TrangThai!=='THAT_BAI')fail('Chỉ thử lại khoản hoàn đã thất bại.');
    await q("UPDATE HoanTien SET TrangThai='CHO_XU_LY' WHERE HoanTienID=?",[h.HoanTienID],c);await audit(r.user,'THU_LAI_HOAN','HoanTien',h.HoanTienID,required(r.body.reason,'lý do thử lại'),c);return {ok:true};
  })));
  router.post('/manage/refunds/:id/failure',manager,run(async r=>await tx(async c=>{
    const [h]=await q('SELECT * FROM HoanTien WHERE HoanTienID=? FOR UPDATE',[positive(r.params.id)],c);if(!h)fail('Không có khoản hoàn.',404);
    if(h.TrangThai==='THAT_BAI')return {ok:true};if(!['CHO_XU_LY','DANG_XU_LY'].includes(h.TrangThai))fail('Khoản hoàn không còn ở trạng thái chờ.');
    await q("UPDATE HoanTien SET TrangThai='THAT_BAI' WHERE HoanTienID=?",[h.HoanTienID],c);await audit(r.user,'HOAN_THAT_BAI','HoanTien',h.HoanTienID,required(r.body.reason,'lý do thất bại'),c);return {ok:true};
  })));
  router.get('/manage/receipts',manager,run(async()=>await q(`SELECT p.PhieuKhoID id,p.MaPhieu code,p.LoaiPhieu type,p.TrangThai status,p.NhaCungCap supplier,p.MaChungTuThamChieu reference,p.LyDo reason,p.NgayTao date,p.NgayXacNhan confirmedAt,t.HoTen creator,COUNT(c.ChiTietPhieuKhoID) lineCount,COALESCE(SUM(c.SoLuongThayDoi*COALESCE(c.DonGiaNhap,0)),0) cost FROM PhieuKho p JOIN TaiKhoan t ON t.TaiKhoanID=p.NguoiThucHienID LEFT JOIN ChiTietPhieuKho c ON c.PhieuKhoID=p.PhieuKhoID GROUP BY p.PhieuKhoID ORDER BY p.PhieuKhoID DESC LIMIT 500`)));
  router.get('/manage/receipts/:id',manager,run(async r=>{
    const [p]=await q('SELECT PhieuKhoID id,MaPhieu code,LoaiPhieu type,TrangThai status,NhaCungCap supplier,MaChungTuThamChieu reference,LyDo reason,NgayTao date,NgayXacNhan confirmedAt FROM PhieuKho WHERE PhieuKhoID=?',[positive(r.params.id)]);if(!p)fail('Không tìm thấy phiếu.',404);
    p.items=await q('SELECT c.BienTheID variantId,c.SoLuongThayDoi quantity,c.DonGiaNhap cost,b.SKU sku,s.TenSanPham name,k.GiaTri size,m.TenMau color FROM ChiTietPhieuKho c JOIN BienTheSanPham b ON b.BienTheID=c.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID WHERE c.PhieuKhoID=?',[p.id]);return p;
  }));
  router.post('/manage/receipts',manager,run(async r=>await tx(async c=>{
    // Account lock serializes duplicate submissions before checking their unique request key.
    await q('SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE',[r.user.id],c);
    const [old]=await q('SELECT PhieuKhoID id,NguoiThucHienID owner FROM PhieuKho WHERE MaChongLap=?',[requestKey(r.body.requestId)],c);
    if(old){if(old.owner!==r.user.id)fail('Mã yêu cầu đã được sử dụng.',409);return {id:old.id};}
    return saveReceipt(c,r.user,r.body);
  })));
  router.put('/manage/receipts/:id',manager,run(async r=>await tx(async c=>{
    const [p]=await q('SELECT * FROM PhieuKho WHERE PhieuKhoID=? FOR UPDATE',[positive(r.params.id)],c);if(!p)fail('Không tìm thấy phiếu.',404);
    if(p.TrangThai!=='NHAP')fail('Chỉ được sửa phiếu nháp. Phiếu đã xác nhận cần lập phiếu điều chỉnh.');return saveReceipt(c,r.user,r.body,p.PhieuKhoID);
  })));
  router.post('/manage/receipts/:id/confirm',manager,run(async r=>await tx(async c=>{
    const [p]=await q('SELECT * FROM PhieuKho WHERE PhieuKhoID=? FOR UPDATE',[positive(r.params.id)],c);if(!p)fail('Không tìm thấy phiếu.',404);
    if(p.TrangThai==='DA_XAC_NHAN')return {ok:true};if(p.TrangThai!=='NHAP')fail('Phiếu đã hủy không thể xác nhận.');
    const lines=await q('SELECT * FROM ChiTietPhieuKho WHERE PhieuKhoID=? ORDER BY BienTheID',[p.PhieuKhoID],c);if(!lines.length)fail('Phiếu không có dòng hàng.');
    for(const line of lines)await stockLog(c,r.user,line.BienTheID,p.LoaiPhieu,line.SoLuongThayDoi,0,null,line.ChiTietPhieuKhoID,p.LyDo);
    await q("UPDATE PhieuKho SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=? WHERE PhieuKhoID=?",[r.user.id,p.PhieuKhoID],c);
    await audit(r.user,'XAC_NHAN_PHIEU_KHO','PhieuKho',p.PhieuKhoID,p.LyDo,c);return {ok:true};
  })));
  router.post('/manage/receipts/:id/cancel',manager,run(async r=>await tx(async c=>{
    const [p]=await q('SELECT * FROM PhieuKho WHERE PhieuKhoID=? FOR UPDATE',[positive(r.params.id)],c);if(!p)fail('Không tìm thấy phiếu.',404);if(p.TrangThai==='DA_HUY')return {ok:true};if(p.TrangThai!=='NHAP')fail('Không hủy phiếu đã ghi sổ; hãy lập phiếu điều chỉnh.');
    await q("UPDATE PhieuKho SET TrangThai='DA_HUY' WHERE PhieuKhoID=?",[p.PhieuKhoID],c);await audit(r.user,'HUY_NHAP_KHO','PhieuKho',p.PhieuKhoID,required(r.body.reason,'lý do hủy'),c);return {ok:true};
  })));
  router.put('/manage/stock/:id/warning',manager,run(async r=>{
    const value=Number(r.body.warning);if(!Number.isSafeInteger(value)||value<0||value>100000)fail('Ngưỡng cảnh báo không hợp lệ.');
    await tx(async c=>{const a=await q('UPDATE TonKho SET MucCanhBao=? WHERE BienTheID=?',[value,positive(r.params.id)],c);if(!a.affectedRows)fail('Không tìm thấy tồn kho.',404);await audit(r.user,'NGUONG_CANH_BAO','TonKho',r.params.id,String(value),c);});return {ok:true};
  }));
  router.get('/manage/stock-history',manager,run(async r=>await q(`SELECT b.BienDongID id,b.LoaiBienDong type,b.ThayDoiTon quantity,b.ThayDoiPhanBo reservedChange,b.TonSau stock,b.PhanBoSau reserved,b.LyDo reason,b.ThoiDiem date,v.SKU sku,t.HoTen actor FROM BienDongKho b JOIN BienTheSanPham v ON v.BienTheID=b.BienTheID LEFT JOIN TaiKhoan t ON t.TaiKhoanID=b.NguoiThucHienID ${r.query.variantId?'WHERE b.BienTheID=?':''} ORDER BY b.BienDongID DESC LIMIT 300`,r.query.variantId?[positive(r.query.variantId)]:[])));
  router.post('/manage/orders/:id/delivery',manager,run(async r=>await tx(async c=>{
    const [o]=await q('SELECT * FROM DonHang WHERE DonHangID=? FOR UPDATE',[positive(r.params.id)],c);if(!o)fail('Không có đơn.',404);if(o.TrangThai!=='DANG_GIAO')fail('Chỉ xử lý vận chuyển cho đơn đang giao.');
    const reason=required(r.body.reason,'ghi chú vận chuyển');let status=o.TrangThaiGiaoHang;
    if(r.body.action==='failed'&&['DANG_GIAO','CHUA_GIAO'].includes(status))status='GIAO_THAT_BAI';
    else if(r.body.action==='retry'&&status==='GIAO_THAT_BAI')status='DANG_GIAO';
    else if(r.body.action==='await-return'&&status==='GIAO_THAT_BAI')status='CHO_HOAN_VE';
    else if(r.body.action!=='tracking')fail('Thao tác vận chuyển không phù hợp trạng thái.');
    await q('UPDATE DonHang SET TrangThaiGiaoHang=?,DonViVanChuyen=COALESCE(?,DonViVanChuyen),MaVanDon=COALESCE(?,MaVanDon),PhienBan=PhienBan+1 WHERE DonHangID=?',[status,text(r.body.carrier,150)||null,text(r.body.tracking,100)||null,o.DonHangID],c);
    await q('INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,?,?)',[o.DonHangID,o.TrangThai,o.TrangThai,r.user.id,`Vận chuyển ${status}: ${reason}`],c);await audit(r.user,'VAN_CHUYEN','DonHang',o.DonHangID,reason,c);return {ok:true};
  })));
  router.post('/manage/orders/:id/receive-delivery-return',manager,run(async r=>await tx(async c=>{
    const [o]=await q('SELECT * FROM DonHang WHERE DonHangID=? FOR UPDATE',[positive(r.params.id)],c);if(!o)fail('Không có đơn.',404);if(o.TrangThaiGiaoHang==='DA_NHAN_HOAN')return {ok:true};
    if(o.TrangThai!=='DANG_GIAO'||o.TrangThaiGiaoHang!=='CHO_HOAN_VE')fail('Đơn phải được đánh dấu chờ hoàn về trước khi nhận.');
    const reason=required(r.body.reason,'kết quả kiểm hàng');const lines=await q('SELECT * FROM ChiTietDonHang WHERE DonHangID=? ORDER BY BienTheID',[o.DonHangID],c);
    if(!Array.isArray(r.body.items)||r.body.items.length!==lines.length)fail('Cần kiểm đếm đầy đủ các dòng hàng hoàn về.');
    for(const l of lines){const matches=r.body.items.filter(x=>Number(x.itemId)===l.ChiTietDonHangID);const input=matches[0];if(matches.length!==1||Number(input.received)!==l.SoLuong||!Number.isSafeInteger(Number(input.sellable))||Number(input.sellable)<0||Number(input.sellable)>l.SoLuong)fail('Số lượng nhận/bán lại không hợp lệ. Chỉ xác nhận khi đã kiểm đủ toàn bộ kiện hàng.');}
    const a=await q("INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID,TrangThai,NgayXacNhan,NguoiXacNhanID,DonHangID) VALUES(?,?,'NHAN_TRA',?,?,'DA_XAC_NHAN',UTC_TIMESTAMP(),?,?)",[code('GH'),randomUUID(),reason,r.user.id,r.user.id,o.DonHangID],c);
    for(const l of lines){const qty=Number(r.body.items.find(x=>Number(x.itemId)===l.ChiTietDonHangID).sellable);if(qty){const line=await q('INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,GhiChu) VALUES(?,?,?,?)',[a.insertId,l.BienTheID,qty,`Đã nhận ${l.SoLuong}, bán lại ${qty}. ${reason}`],c);await stockLog(c,r.user,l.BienTheID,'NHAN_TRA',qty,0,null,line.insertId,reason);}}
    await q("UPDATE DonHang SET TrangThai='DA_HUY',TrangThaiGiaoHang='DA_NHAN_HOAN',NgayHuy=UTC_TIMESTAMP(),NguoiHuyID=?,LyDoHuy=?,PhienBan=PhienBan+1 WHERE DonHangID=?",[r.user.id,reason,o.DonHangID],c);
    await q("UPDATE ThanhToan SET TrangThai='DA_HUY' WHERE DonHangID=? AND TrangThai='CHO_XAC_NHAN'",[o.DonHangID],c);
    await reserveRefunds(c,r.user,o.DonHangID,'Hoàn tiền đơn giao thất bại '+o.MaDonHang);
    await q("INSERT INTO LichSuDonHang(DonHangID,TrangThaiCu,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,'DANG_GIAO','DA_HUY',?,?)",[o.DonHangID,r.user.id,'Đã nhận hàng giao thất bại. '+reason],c);
    await audit(r.user,'NHAN_HOAN_GIAO','DonHang',o.DonHangID,JSON.stringify({reason,items:r.body.items}),c);return {ok:true};
  })));
  router.post('/manage/orders/:id/late-payment',manager,run(async r=>await tx(async c=>{
    const [o]=await q('SELECT d.*,p.MaPhuongThuc method FROM DonHang d JOIN PhuongThucThanhToan p ON p.PhuongThucID=d.PhuongThucID WHERE d.DonHangID=? FOR UPDATE',[positive(r.params.id)],c);
    if(!o||o.TrangThai!=='DA_HUY'||o.method!=='BANK_TRANSFER')fail('Chỉ đối soát chuyển khoản đến sau khi đơn bị hủy.');
    const [p]=await q('SELECT * FROM ThanhToan WHERE DonHangID=? ORDER BY ThanhToanID LIMIT 1 FOR UPDATE',[o.DonHangID],c);if(!p)fail('Không có giao dịch.');
    if(p.TrangThai==='DA_XAC_NHAN')return {ok:true};
    const amount=Number(r.body.amount);if(!Number.isSafeInteger(amount)||amount<=0||amount>o.TongThanhToan)fail('Số tiền nhận phải dương và không vượt tổng đơn.');
    await q("UPDATE ThanhToan SET TrangThai='DA_XAC_NHAN',SoTien=?,NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=?,BangChung=? WHERE ThanhToanID=?",[amount,r.user.id,required(r.body.proof,'bằng chứng tiền vào'),p.ThanhToanID],c);
    await reserveRefunds(c,r.user,o.DonHangID,'Chuyển khoản đến sau khi đơn đã hủy');await audit(r.user,'THU_TIEN_DEN_MUON','DonHang',o.DonHangID,'Đối soát và tạo khoản hoàn; không khôi phục đơn',c);return {ok:true};
  })));
}
