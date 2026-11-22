import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
import {query,pool,config} from '../server/db.js';
import {expireUnpaidOrders} from '../server/routes.js';

const base=`http://127.0.0.1:${process.env.PORT||5173}/api`;
class Client {
  constructor(area='customer'){this.area=area;this.cookie='';}
  async call(path,body,method=body?'POST':'GET') {
    const response=await fetch(base+path,{method,headers:{'Content-Type':'application/json','X-Sole-Request':'1','X-Sole-Area':this.area,Cookie:this.cookie},body:body?JSON.stringify(body):undefined});
    if(response.headers.getSetCookie().length)this.cookie=response.headers.getSetCookie().at(-1).split(';')[0];
    return {status:response.status,data:await response.json()};
  }
  async ok(...args){const result=await this.call(...args);assert.ok(result.status<300,JSON.stringify(result));return result.data;}
}

test('Luồng phát triển: phiếu kho, hết hạn, COD, giao thất bại, tiền đến muộn và hoàn tiền',async t=>{
  assert.match(config.database,/^sole_studio_nhom19/);
  const manager=new Client('manager'),customer=new Client(),admin=new Client('admin');
  let productId,variantId,receiptId,orderId;
  const requestIds=[];
  const stock=async()=> (await query('SELECT SoLuongTon physical,SoLuongDaPhanBo reserved FROM TonKho WHERE BienTheID=?',[variantId]))[0];
  const newOrder=async(method='COD',quantity=1)=>{
    await customer.ok('/cart',{variantId,quantity},'PUT');
    const payload={name:'QA nghiệp vụ',phone:'0901234567',address:'Địa chỉ QA',method};
    const quote=await customer.ok('/checkout/quote',payload);const requestId=randomUUID();requestIds.push(requestId);
    return customer.ok('/orders',{...payload,fingerprint:quote.fingerprint,requestId});
  };
  try {
    await manager.ok('/auth/login',{login:'shop@sole.vn',password:'SoleDemo@123'});
    await admin.ok('/auth/login',{login:'admin@sole.vn',password:'SoleDemo@123'});
    await customer.ok('/auth/register',{name:'QA nghiệp vụ',email:`workflow-${randomUUID()}@example.test`,password:'Workflow@123'});
    const lookups=await manager.ok('/manage/lookups');
    const p=await manager.ok('/manage/products',{code:'QA-'+randomUUID(),name:'QA luồng kho',price:2000000,categoryId:lookups.categories[0].id,brandId:lookups.brands[0].id,status:'DANG_BAN'});productId=p.id;
    const v=await manager.ok('/manage/variants',{productId,sku:'QA-'+randomUUID(),sizeId:lookups.sizes[0].id,colorId:lookups.colors[0].id});variantId=v.id;
    await t.test('Tạo màu, chọn size lẻ và chặn trùng tổ hợp',async()=>{
      const name='QA-'+randomUUID();
      const color=await manager.ok('/manage/colors',{name,hex:'#427651'});
      const size=await manager.ok('/manage/sizes',{name:'42,5'});
      let extra;
      try{
        assert.equal((await manager.ok('/manage/sizes',{name:'42.50'})).id,size.id);
        assert.equal((await manager.ok('/manage/colors',{name})).id,color.id);
        const fields={productId,sku:randomUUID(),sizeId:size.id,colorId:color.id};
        extra=await manager.ok('/manage/variants',fields);
        assert.equal((await manager.call('/manage/variants',{...fields,sku:randomUUID()})).status,409);
      }finally{
        if(extra){await manager.ok('/manage/variants/'+extra.id,{active:0},'PUT');await manager.ok('/manage/variants/'+extra.id,undefined,'DELETE');}
        await query('DELETE FROM MauSac WHERE MauSacID=? AND TenMau=?',[color.id,name]);
        if(!lookups.sizes.some(s=>s.id===size.id))await query('DELETE FROM KichCo WHERE KichCoID=?',[size.id]);
      }
    });
    await t.test('Biến thể đang bán phải lưu ẩn trước khi sửa hoặc xóa',async()=>{
      const sizeId=lookups.sizes[1].id,colorId=lookups.colors[0].id;
      const extra=await manager.ok('/manage/variants',{productId,sku:'QA-'+randomUUID(),sizeId,colorId});
      const path='/manage/variants/'+extra.id;
      assert.equal((await manager.call(path,undefined,'DELETE')).status,409);
      assert.equal((await manager.call(path,{active:0,sizeId,colorId,price:100000},'PUT')).status,409);
      await manager.ok(path,{active:0},'PUT');
      await manager.ok(path,{active:0,sizeId,colorId,price:100000},'PUT');
      const row=(await manager.ok('/manage/variants')).find(x=>x.id===extra.id);
      assert.ok(row.productCode);
      assert.equal(row.price,100000);
      await manager.ok(path,undefined,'DELETE');
    });
    await t.test('Công bố, kết thúc, gia hạn và xóa mã nháp trên trang khách',async()=>{
      const code='QA-'+randomUUID(), day=86400000;
      const time=offset=>new Date(Date.now()+offset*day).toISOString();
      const fields={code,name:'Ưu đãi QA',type:'PHAN_TRAM',value:10,minimum:500000,start:time(-2),end:time(-1)};
      assert.equal((await manager.call('/manage/promotions',{...fields,minimum:-1})).status,400);
      const {id}=await manager.ok('/manage/promotions',fields);
      const path='/manage/promotions/'+id;
      const visible=async()=>(await customer.ok('/promotions')).find(p=>p.code===code);
      const publishedCode=code+'P';
      const direct=await manager.ok('/manage/promotions',{...fields,code:publishedCode,end:time(1),status:'DANG_AP_DUNG'});
      try {
        const management=(await manager.ok('/manage/promotions')).find(p=>p.id===direct.id);
        const storefront=(await customer.ok('/promotions')).find(p=>p.code===publishedCode);
        assert.equal(management.visible,1);
        for(const field of ['code','name','type','value','minimum','start','end'])assert.equal(storefront[field],management[field]);
      } finally {
        await manager.ok('/manage/promotions/'+direct.id,{status:'NHAP'},'PUT');
        await manager.ok('/manage/promotions/'+direct.id,undefined,'DELETE');
      }
      try {
        assert.equal(await visible(),undefined);
        const edit={...fields,status:'DANG_AP_DUNG',end:time(2),name:'Gia hạn QA',minimum:150000};
        for(const bad of [{end:fields.end},{end:time(-3)},{minimum:-1},{value:-1},{value:101}]){
          assert.equal((await manager.call(path,{...edit,...bad},'PUT')).status,400);
        }
        await manager.ok(path,edit,'PUT');
        const published=await visible();
        assert.equal(published.name,edit.name);
        assert.equal(published.minimum,edit.minimum);
        assert.equal(published.value,10);
        assert.equal((await manager.call(path,undefined,'DELETE')).status,409);
        await manager.ok(path,{status:'KET_THUC'},'PUT');
        assert.equal(await visible(),undefined);
        await manager.ok(path,{...edit,start:time(1)},'PUT');
        assert.equal(await visible(),undefined);
      } finally {
        await manager.ok(path,{status:'NHAP'},'PUT');
        await manager.ok(path,undefined,'DELETE');
      }
      assert.ok(!(await manager.ok('/manage/promotions')).some(p=>p.id===id));
    });
    await t.test('Phiếu nháp không tăng kho; sửa được; xác nhận đồng thời chỉ tăng một lần',async()=>{
      const body={requestId:randomUUID(),type:'NHAP',supplier:'Nhà cung cấp QA',reference:'QA-'+randomUUID(),reason:'Nhập thử nghiệp vụ',items:[{variantId,quantity:10,cost:1000000}]};
      const a=await manager.ok('/manage/receipts',body);receiptId=a.id;assert.equal((await manager.ok('/manage/receipts',body)).id,a.id);assert.deepEqual(await stock(),{physical:0,reserved:0});
      await manager.ok('/manage/receipts/'+a.id,{...body,items:[{variantId,quantity:12,cost:1000000}]},'PUT');
      await Promise.all([manager.ok('/manage/receipts/'+a.id+'/confirm',{}),manager.ok('/manage/receipts/'+a.id+'/confirm',{})]);
      assert.deepEqual(await stock(),{physical:12,reserved:0});
      assert.equal((await manager.call('/manage/receipts/'+a.id,body,'PUT')).status,400);
      assert.equal((await manager.call('/manage/receipts',{...body,requestId:randomUUID()})).status,409);
      assert.equal((await admin.call('/manage/receipts')).status,403);
    });
    await t.test('Mức trần phần trăm, bỏ trần và giảm cố định khớp với báo giá',async()=>{
      const fields={code:'QA-'+randomUUID(),name:'QA mức trần',type:'PHAN_TRAM',value:10,minimum:0,maximum:120000,status:'DANG_AP_DUNG',start:new Date(Date.now()-60000).toISOString(),end:new Date(Date.now()+86400000).toISOString()};
      const {id}=await manager.ok('/manage/promotions',fields);
      const path='/manage/promotions/'+id;
      const quote=()=>customer.ok('/checkout/quote',{method:'COD',promo:fields.code});
      try{
        await customer.ok('/cart',{variantId,quantity:1},'PUT');
        assert.equal((await quote()).discount,120000);
        assert.equal((await manager.ok('/manage/promotions')).find(p=>p.id===id).maximum,120000);
        for(const maximum of [0,-1])assert.equal((await manager.call(path,{...fields,maximum},'PUT')).status,400);
        await manager.ok(path,{...fields,maximum:''},'PUT');
        assert.equal((await quote()).discount,200000);
        await manager.ok(path,{...fields,type:'SO_TIEN',value:3000000},'PUT');
        assert.equal((await quote()).discount,2000000);
        assert.equal((await customer.ok('/promotions')).find(p=>p.code===fields.code).maximum,null);
      }finally{
        await manager.ok(path,{status:'NHAP'},'PUT');
        await manager.ok(path,undefined,'DELETE');
      }
    });
    await t.test('Hủy nháp không đổi kho; điều chỉnh âm quá tồn bị hoàn tác toàn bộ',async()=>{
      const body={requestId:randomUUID(),type:'DIEU_CHINH',reason:'QA kiểm kê',items:[{variantId,quantity:-13}]};
      const a=await manager.ok('/manage/receipts',body);assert.equal((await manager.call('/manage/receipts/'+a.id+'/confirm',{})).status,400);assert.deepEqual(await stock(),{physical:12,reserved:0});
      await manager.ok('/manage/receipts/'+a.id+'/cancel',{reason:'QA hủy nháp'});assert.equal((await manager.call('/manage/receipts/'+a.id+'/confirm',{})).status,400);
      assert.equal((await manager.ok('/manage/receipts/'+a.id)).status,'DA_HUY');assert.ok((await manager.ok('/manage/receipts')).some(x=>x.id===a.id));
    });
    await t.test('Ngưỡng cảnh báo và sổ kho truy xuất được',async()=>{
      await manager.ok('/manage/stock/'+variantId+'/warning',{warning:10},'PUT');const v=(await manager.ok('/manage/variants')).find(x=>x.id===variantId);assert.equal(v.warning,10);
      assert.equal((await manager.ok('/manage/stock-history?variantId='+variantId)).length,1);
    });
    await t.test('Giá thay đổi sau bước kiểm tra: từ chối tạo đơn, không giữ hàng',async()=>{
      await customer.ok('/cart',{variantId,quantity:1},'PUT');const quote=await customer.ok('/checkout/quote',{method:'COD'});
      await query('UPDATE SanPham SET GiaBan=2100000 WHERE SanPhamID=?',[productId]);
      assert.equal((await customer.call('/orders',{requestId:randomUUID(),name:'QA',phone:'0901234567',address:'QA',method:'COD',fingerprint:quote.fingerprint})).status,409);
      assert.deepEqual(await stock(),{physical:12,reserved:0});await query('UPDATE SanPham SET GiaBan=2000000 WHERE SanPhamID=?',[productId]);
    });
    await t.test('Chuyển khoản chưa thu không được xác nhận; hết hạn giải phóng giữ hàng đúng một lần',async()=>{
      const a=await newOrder('BANK_TRANSFER',2);orderId=a.id;const o=await customer.ok('/orders/'+a.id);assert.ok(o.paymentDeadline);assert.deepEqual(await stock(),{physical:12,reserved:2});
      assert.equal((await manager.call('/manage/orders/'+a.id+'/status',{status:'DA_XAC_NHAN'})).status,400);
      await query('UPDATE DonHang SET HanThanhToan=DATE_SUB(UTC_TIMESTAMP(),INTERVAL 1 MINUTE) WHERE DonHangID=?',[a.id]);
      await expireUnpaidOrders();await expireUnpaidOrders();const after=await customer.ok('/orders/'+a.id);assert.equal(after.status,'DA_HUY');assert.equal(after.payments[0].status,'HET_HAN');assert.deepEqual(await stock(),{physical:12,reserved:0});
    });
    await t.test('Tiền đến sau hủy tạo một khoản hoàn, không khôi phục đơn hay giữ hàng',async()=>{
      await manager.ok('/manage/orders/'+orderId+'/late-payment',{amount:4000000,proof:'QA tiền vào sau hết hạn'});await manager.ok('/manage/orders/'+orderId+'/late-payment',{amount:4000000,proof:'QA gửi lại'});
      const o=await customer.ok('/orders/'+orderId);assert.equal(o.status,'DA_HUY');assert.equal(o.paid,4000000);assert.equal(o.refunds.length,1);assert.deepEqual(await stock(),{physical:12,reserved:0});
    });
    await t.test('COD chỉ ghi nhận thu sau giao; hàng đang giao thất bại chưa được hoàn kho',async()=>{
      const a=await newOrder('COD',2);orderId=a.id;let o=await customer.ok('/orders/'+a.id);
      assert.equal((await manager.call('/manage/payments/'+o.payments[0].id+'/confirm',{proof:'QA chưa giao'})).status,400);
      for(const status of ['DA_XAC_NHAN','DANG_CHUAN_BI','DANG_GIAO'])await manager.ok('/manage/orders/'+a.id+'/status',{status});
      await manager.ok('/manage/orders/'+a.id+'/delivery',{action:'tracking',carrier:'Vận chuyển QA',tracking:'QA-TRACK',reason:'QA bàn giao'});
      await manager.ok('/manage/orders/'+a.id+'/delivery',{action:'failed',reason:'QA không liên hệ được'});assert.deepEqual(await stock(),{physical:10,reserved:0});
      assert.equal((await manager.call('/manage/orders/'+a.id+'/status',{status:'DA_GIAO'})).status,400);
      assert.equal((await manager.call('/manage/orders/'+a.id+'/receive-delivery-return',{reason:'QA nhận sớm',items:[]})).status,400);
      await manager.ok('/manage/orders/'+a.id+'/delivery',{action:'await-return',reason:'QA yêu cầu hoàn kiện'});
    });
    await t.test('Thực nhận hàng giao thất bại: chỉ tăng số đôi bán lại, retry không cộng trùng',async()=>{
      const o=await customer.ok('/orders/'+orderId);const body={reason:'QA nhận đủ 2, một đôi hư',items:[{itemId:o.items[0].id,received:2,sellable:1}]};
      await manager.ok('/manage/orders/'+orderId+'/receive-delivery-return',body);await manager.ok('/manage/orders/'+orderId+'/receive-delivery-return',body);
      const after=await customer.ok('/orders/'+orderId);assert.equal(after.deliveryStatus,'DA_NHAN_HOAN');assert.equal(after.status,'DA_HUY');assert.deepEqual(await stock(),{physical:11,reserved:0});
    });
    await t.test('Trả hàng đã nhận được lưu dù COD chưa đối soát; chặn hoàn tất khi chưa hoàn đủ',async()=>{
      const a=await newOrder('COD');orderId=a.id;for(const status of ['DA_XAC_NHAN','DANG_CHUAN_BI','DANG_GIAO','DA_GIAO'])await manager.ok('/manage/orders/'+a.id+'/status',{status});
      const o=await customer.ok('/orders/'+a.id);await customer.ok('/returns',{type:'TRA',itemId:o.items[0].id,quantity:1,reason:'QA trả sau giao'});const y=(await customer.ok('/returns'))[0];
      await manager.ok('/manage/returns/'+y.id+'/action',{action:'approve',reason:'QA chấp nhận'});await manager.ok('/manage/returns/'+y.id+'/action',{action:'receive',sellable:true,reason:'QA đủ hàng'});
      assert.deepEqual(await stock(),{physical:11,reserved:0});assert.equal((await manager.call('/manage/returns/'+y.id+'/action',{action:'complete'})).status,400);
      const detail=await customer.ok('/returns/'+y.id);assert.equal(detail.items[0].received,1);assert.equal(detail.status,'DANG_XU_LY');
      await manager.ok('/manage/payments/'+o.payments[0].id+'/confirm',{proof:'QA tiền COD đã nhận đối soát'});
      await manager.ok('/manage/returns/'+y.id+'/action',{action:'prepare-refund'});await manager.ok('/manage/returns/'+y.id+'/action',{action:'prepare-refund'});
      const refunds=(await customer.ok('/returns/'+y.id)).refunds;assert.equal(refunds.length,1);
      const refund=(await manager.ok('/manage/refunds')).find(x=>x.code===o.code);
      await manager.ok('/manage/refunds/'+refund.id+'/failure',{reason:'QA ngân hàng tạm lỗi'});assert.equal((await manager.call('/manage/refunds/'+refund.id+'/confirm',{proof:'QA không được bỏ qua thử lại'})).status,400);
      await manager.ok('/manage/refunds/'+refund.id+'/retry',{reason:'QA đã kiểm tra lại'});await manager.ok('/manage/refunds/'+refund.id+'/confirm',{proof:'QA đã hoàn thực tế'});await manager.ok('/manage/refunds/'+refund.id+'/confirm',{proof:'QA gửi lại'});
      await manager.ok('/manage/returns/'+y.id+'/action',{action:'complete'});assert.equal((await customer.ok('/returns/'+y.id)).status,'HOAN_TAT');
    });
    await t.test('Quá thời hạn trả bị từ chối; sổ kho khớp tồn thực tế và đang giữ',async()=>{
      const a=await newOrder('COD');orderId=a.id;for(const status of ['DA_XAC_NHAN','DANG_CHUAN_BI','DANG_GIAO','DA_GIAO'])await manager.ok('/manage/orders/'+a.id+'/status',{status});
      const o=await customer.ok('/orders/'+a.id);await query('UPDATE DonHang SET NgayGiao=DATE_SUB(UTC_TIMESTAMP(),INTERVAL 366 DAY) WHERE DonHangID=?',[a.id]);
      assert.equal((await customer.call('/returns',{type:'TRA',itemId:o.items[0].id,quantity:1,reason:'QA quá hạn'})).status,400);
      const [sum]=await query('SELECT SUM(ThayDoiTon) physical,SUM(ThayDoiPhanBo) reserved FROM BienDongKho WHERE BienTheID=?',[variantId]);assert.deepEqual(await stock(),sum);
    });
  } finally {
    if(productId)await query("UPDATE SanPham SET TrangThai='NGUNG_BAN' WHERE SanPhamID=?",[productId]);
    // Test documents and ledger remain available for audit, rather than deleting transactional history.
    await pool.end();
  }
});
