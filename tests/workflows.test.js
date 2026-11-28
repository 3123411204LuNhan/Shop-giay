import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
import {query,pool,config} from './db.js';

const base=`http://127.0.0.1:${process.env.PORT||5173}/api`;
class Client {
  constructor(area='customer'){this.area=area;this.cookie='';}
  async call(path,body,method=body?'POST':'GET') {
    const multipart=body instanceof FormData;
    const response=await fetch(base+path,{method,headers:{...(!multipart?{'Content-Type':'application/json'}:{}),'X-Sole-Request':'1','X-Sole-Area':this.area,Cookie:this.cookie},body:multipart?body:body?JSON.stringify(body):undefined});
    if(response.headers.getSetCookie().length)this.cookie=response.headers.getSetCookie().at(-1).split(';')[0];
    return {status:response.status,data:await response.json()};
  }
  async ok(...args){const result=await this.call(...args);assert.ok(result.status<300,JSON.stringify(result));return result.data;}
}

const png=Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jCOcAAAAASUVORK5CYII=','base64');
function imageForm(fields,bytes=png){
  const form=new FormData();
  for(const [key,value] of Object.entries(fields))form.set(key,String(value));
  form.set('imageFile',new Blob([bytes],{type:'image/png'}),'qa.png');
  return form;
}

test('Luồng phát triển: phiếu kho, hết hạn, COD, giao thất bại, tiền đến muộn và hoàn tiền',async t=>{
  assert.match(config.database,/^sole_studio_nhom19/);
  const manager=new Client('manager'),customer=new Client(),admin=new Client('admin');
  let productId,variantId,receiptId,orderId;
  const requestIds=[];
  const stock=async()=> (await query('SELECT SoLuongTon physical,SoLuongDaPhanBo reserved FROM TonKho WHERE BienTheID=?',[variantId]))[0];
  const newOrder=async(method='COD',quantity=1,promo)=>{
    await customer.ok('/cart',{variantId,quantity},'PUT');
    const payload={name:'QA nghiệp vụ',phone:'0901234567',address:'Địa chỉ QA',method,promo};
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
    await t.test('Ảnh sản phẩm: tạo, giữ, đổi URL, xóa và từ chối tệp không hợp lệ',async()=>{
      const fields={code:'QA-'+randomUUID(),name:'QA ảnh',price:100000,categoryId:lookups.categories[0].id,brandId:lookups.brands[0].id};
      const {id}=await manager.ok('/manage/products',imageForm(fields));
      const path='/manage/products/'+id;
      const get=()=>customer.ok('/products/'+id);
      const image=(await get()).image;
      assert.match(image,/^\/api\/product-images\//);
      const file=await fetch(new URL(image,base));
      assert.match(file.headers.get('content-type'),/^image\/png/);
      assert.deepEqual(Buffer.from(await file.arrayBuffer()),png);
      await manager.ok(path,{description:'Sửa không thay ảnh'},'PUT');
      assert.equal((await get()).image,image);
      assert.equal((await manager.call(path,imageForm({},Buffer.from('<svg>file giả</svg>')),'PUT')).status,415);
      assert.equal((await manager.call(path,imageForm({},Buffer.alloc(5*1024*1024+1)),'PUT')).status,413);
      const url='https://example.test/qa-photo.png';
      assert.equal((await manager.call(path,imageForm({image:url}),'PUT')).status,400);
      assert.equal((await get()).image,image);
      await manager.ok(path,{image:url},'PUT');
      assert.equal((await get()).image,url);
      assert.equal((await fetch(new URL(image,base))).status,404);
      await manager.ok(path,imageForm({}),'PUT');
      const replacement=(await get()).image;
      assert.notEqual(replacement,image);
      await manager.ok(path,{removeImage:true},'PUT');
      assert.equal((await get()).image,null);
      assert.equal((await fetch(new URL(replacement,base))).status,404);
    });
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
      await manager.ok('/manage/stock/'+variantId+'/warning',{warning:10},'PUT');const v=(await manager.ok('/manage/stock')).find(x=>x.id===variantId);assert.equal(v.warning,10);
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
      let after;
      const deadline=Date.now()+20000;
      do {
        after=await customer.ok('/orders/'+a.id);
        if(after.status==='DA_HUY')break;
        await new Promise(resolve=>setTimeout(resolve,250));
      }while(Date.now()<deadline);assert.equal(after.status,'DA_HUY');assert.equal(after.payments[0].status,'HET_HAN');assert.deepEqual(await stock(),{physical:12,reserved:0});
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
      const o=await customer.ok('/orders/'+a.id);
      const image='https://example.test/return-evidence.jpg';
      await customer.ok('/returns',{type:'TRA',itemId:o.items[0].id,quantity:1,reason:'QA trả sau giao',image});
      const y=(await customer.ok('/returns'))[0];
      assert.equal((await manager.ok('/manage/returns')).find(row=>row.id===y.id).image,image);
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
    const deliver=async orderId=>{
      for(const status of ['DA_XAC_NHAN','DANG_CHUAN_BI','DANG_GIAO','DA_GIAO'])
        await manager.ok('/manage/orders/'+orderId+'/status',{status});
      return customer.ok('/orders/'+orderId);
    };
    const confirmPaid=order=>manager.ok('/manage/payments/'+order.payments[0].id+'/confirm',{proof:'QA đối soát'});
    const decision=(request,action,body={})=>manager.ok('/manage/returns/'+request+'/action',{action,reason:'QA kiểm hàng',...body});

    await t.test('Chi tiết, retry gửi yêu cầu, nhận 2 chỉ nhập kho 1 và phân bổ đủ tiền hoàn lẻ',async()=>{
      const promo='QA-'+randomUUID(),now=Date.now();
      await manager.ok('/manage/promotions',{code:promo,name:'Giảm lẻ 1 đồng',type:'SO_TIEN',value:1,minimum:0,start:new Date(now-60000),end:new Date(now+3600000),status:'DANG_AP_DUNG'});
      const order=await deliver((await newOrder('COD',3,promo)).id);
      await confirmPaid(order);
      const item=order.items[0];
      assert.equal(item.total%3,2);
      const before=await stock();
      const payload={type:'TRA',itemId:item.id,quantity:2,reason:'Trả hai đôi',image:'https://example.test/evidence.jpg',requestId:randomUUID()};
      const first=await customer.ok('/returns',payload);
      assert.equal(first.status,'CHO_DUYET');assert.ok(first.code);
      assert.deepEqual(await customer.ok('/returns',payload),first);
      assert.equal((await customer.call('/returns',{...payload,requestId:randomUUID()})).status,400);
      assert.equal((await customer.call('/manage/returns/'+first.id)).status,403);
      assert.equal((await manager.call('/manage/returns/99999')).status,404);
      const path='/manage/returns/'+first.id;
      let detail=await manager.ok(path);
      assert.equal(detail.orderCode,order.code);
      assert.equal(detail.items[0].size,item.size);
      assert.equal(detail.images[0].url,payload.image);
      await decision(first.id,'approve');
      assert.equal((await customer.ok('/returns',payload)).status,'DA_DUYET');
      detail=await manager.ok(path);
      const line=detail.items[0];
      assert.equal((await manager.call(path+'/action',{action:'receive',items:[{id:line.id,received:1,sellable:1}]})).status,400);
      assert.equal((await manager.call(path+'/action',{action:'receive',items:[{id:line.id,received:2,sellable:3}]})).status,400);
      await decision(first.id,'receive',{items:[{id:line.id,received:2,sellable:1}]});
      assert.equal((await manager.call(path+'/action',{action:'receive',items:[{id:line.id,received:2,sellable:1}]})).status,400);
      assert.equal((await stock()).physical,before.physical+1);
      const second=await customer.ok('/returns',{...payload,quantity:1,requestId:randomUUID()});
      await decision(second.id,'approve');
      const rest=await manager.ok('/manage/returns/'+second.id);
      assert.equal(line.refund+rest.items[0].refund,item.total);
    });

    await t.test('Ảnh bằng chứng tải lên: retry giữ một ảnh và chỉ khách sở hữu hoặc chủ shop được đọc',async()=>{
      const order=await deliver((await newOrder()).id);
      const fields={type:'TRA',itemId:order.items[0].id,quantity:1,reason:'QA tải ảnh',requestId:randomUUID()};
      assert.equal((await customer.call('/returns',imageForm(fields,Buffer.from('không phải ảnh')))).status,415);
      const request=await customer.ok('/returns',imageForm(fields));
      assert.deepEqual(await customer.ok('/returns',imageForm(fields)),request);
      const customerDetail=await customer.ok('/returns/'+request.id);
      assert.equal(customerDetail.images.length,1);
      const url=customerDetail.images[0].url;
      const get=(path,client)=>fetch(new URL(path,base),{headers:client?{Cookie:client.cookie}:{}});
      const file=await get(url,customer);
      assert.equal(file.status,200);
      assert.equal(file.headers.get('cache-control'),'no-store');
      assert.deepEqual(Buffer.from(await file.arrayBuffer()),png);
      assert.equal((await get(url)).status,401);
      const other=new Client();
      await other.ok('/auth/register',{name:'QA người khác',email:randomUUID()+'@example.test',password:'Workflow@123'});
      assert.equal((await get(url,other)).status,404);
      const shopDetail=await manager.ok('/manage/returns/'+request.id);
      const shopImage=await get(shopDetail.images[0].url,manager);
      assert.equal(shopImage.status,200);
      assert.deepEqual(Buffer.from(await shopImage.arrayBuffer()),png);
      assert.equal((await get(shopDetail.images[0].url,admin)).status,401);
    });
    await t.test('Đổi cùng giá: lưu nhận hàng khi còn vướng đối soát và tồn, sau đó tạo đúng một đơn',async()=>{
      const target=(await manager.ok('/manage/variants',{productId,sku:randomUUID(),sizeId:lookups.sizes[0].id,colorId:lookups.colors[1].id})).id;
      const adjust=async(type,quantity)=>{
        const {id}=await manager.ok('/manage/receipts',{requestId:randomUUID(),type,reference:randomUUID(),supplier:'QA',reason:'QA tồn đơn đổi',items:[{variantId:target,quantity}]});
        await manager.ok('/manage/receipts/'+id+'/confirm',{});
      };
      await adjust('NHAP',1);
      const original=await deliver((await newOrder()).id);
      const item=original.items[0];
      const options=await customer.ok('/returns/options/'+item.id);
      assert.equal(options.remaining,1);
      assert.ok(options.variants.some(v=>v.id===target));
      const data={type:'DOI',itemId:item.id,quantity:1,reason:'Đổi màu',requestId:randomUUID()};
      assert.equal((await customer.call('/returns',{...data,targetVariantId:item.variantId})).status,400);
      const request=await customer.ok('/returns',{...data,targetVariantId:target});
      const path='/manage/returns/'+request.id;
      const detail=await manager.ok(path);
      assert.equal(detail.items[0].targetColor,lookups.colors[1].name);
      await decision(request.id,'approve');
      await adjust('DIEU_CHINH',-1);
      const received=await decision(request.id,'receive',{items:[{id:detail.items[0].id,received:1,sellable:0}]});
      assert.match(received.message,/đối soát/);
      assert.equal((await manager.ok(path)).items[0].received,1);
      assert.equal((await manager.call(path+'/action',{action:'complete'})).status,400);
      await confirmPaid(original);
      const unavailable=await manager.call(path+'/action',{action:'create-exchange'});
      assert.equal(unavailable.status,409);assert.match(unavailable.data.message,/thiếu tồn/);
      await adjust('NHAP',1);
      await Promise.all([decision(request.id,'create-exchange'),decision(request.id,'create-exchange')]);
      await decision(request.id,'receive');
      await decision(request.id,'complete');
      const final=await customer.ok('/returns/'+request.id);
      assert.equal(final.status,'HOAN_TAT');
      assert.equal(final.refunds.length,0);
      assert.ok(final.replacementCode);
      const replacements=(await customer.ok('/orders')).filter(o=>o.code===final.replacementCode);
      assert.equal(replacements.length,1);
      const replacement=await customer.ok('/orders/'+replacements[0].id);
      assert.equal(replacement.total,0);assert.equal(replacement.items[0].variantId,target);
      const [balance]=await query('SELECT SoLuongDaPhanBo reserved FROM TonKho WHERE BienTheID=?',[target]);
      assert.equal(balance.reserved,1);
    });
    await t.test('Kiểm đếm trên API kho: 12 → 9, từ chối tồn sổ cũ và xác nhận chỉ một lần',async()=>{
      const target=(await manager.ok('/manage/variants',{productId,sku:randomUUID(),sizeId:lookups.sizes.at(-1).id,colorId:lookups.colors.at(-1).id})).id;
      const read=async()=>(await manager.ok('/manage/stock?productId='+productId)).find(v=>v.id===target);
      const save=(items,type='DIEU_CHINH')=>manager.ok('/manage/receipts',{requestId:randomUUID(),type,reference:randomUUID(),supplier:'QA',reason:'Kiểm kê',items});
      const confirm=id=>manager.ok('/manage/receipts/'+id+'/confirm',{});
      await confirm((await save([{variantId:target,quantity:12}],'NHAP')).id);
      const main=await save([{variantId:target,counted:9,baseline:12}]);
      const stale=await save([{variantId:target,counted:11,baseline:12}]);
      assert.equal((await read()).stock,12);
      const detail=await manager.ok('/manage/receipts/'+main.id);
      assert.equal(detail.items[0].quantity,-3);
      assert.equal(detail.items[0].baseline,12);
      assert.equal(detail.items[0].counted,9);
      await Promise.all([confirm(main.id),confirm(main.id)]);
      assert.equal((await read()).stock,9);
      assert.equal((await manager.call('/manage/receipts/'+stale.id+'/confirm',{})).status,409);
      const ledger=await manager.ok('/manage/stock-history?variantId='+target);
      assert.deepEqual(ledger.map(v=>v.quantity),[-3,12]);
      const wrong=await manager.call('/manage/receipts',{requestId:randomUUID(),type:'DIEU_CHINH',reason:'Mốc đã cũ',items:[{variantId:target,counted:8,baseline:12}]});
      assert.equal(wrong.status,409);
      await customer.ok('/cart',{variantId:target,quantity:4},'PUT');
      const order=await customer.ok('/orders',{requestId:randomUUID(),name:'QA',phone:'0901234567',address:'QA',method:'COD'});
      assert.equal((await read()).reserved,4);
      const below=await manager.call('/manage/receipts',{requestId:randomUUID(),type:'DIEU_CHINH',reason:'Thấp hơn hàng giữ',items:[{variantId:target,counted:3,baseline:9}]});
      assert.equal(below.status,400);
      assert.equal((await read()).stock,9);
      await customer.ok('/orders/'+order.id+'/cancel',{reason:'Kết thúc ca QA'});
    });
    await t.test('Báo cáo tính thu, hoàn, số đơn và bán ròng theo kỳ giờ Việt Nam',async reportTest=>{
      const report='/manage/reports';
      const {user:{id:customerId}}=await customer.ok('/me');
      const {user:{id:managerId}}=await manager.ok('/me');
      const [{id:method}]=await query("SELECT PhuongThucID id FROM PhuongThucThanhToan WHERE MaPhuongThuc='COD'");
      const [{id:otherVariant}]=await query('SELECT BienTheID id FROM BienTheSanPham WHERE SanPhamID<>? ORDER BY BienTheID LIMIT 1',[productId]);
      const {id:secondVariant}=await manager.ok('/manage/variants',{productId,sku:randomUUID(),sizeId:lookups.sizes[1].id,colorId:lookups.colors.at(-1).id});
      const inPeriod='2040-02-11 17:00:00', nextDay='2040-02-12 17:00:00';
      const range=report+'?from=2040-02-12&to=2040-02-12';
      const insert=async(sql,args)=>(await query(sql,args)).insertId;
      const orderIds=[];
      reportTest.after(async()=>{
        for(const table of ['HoanTien','ChiTietDoiTra','YeuCauDoiTra','ThanhToan','ChiTietDonHang','DonHang'])
          if(orderIds.length)await query('DELETE FROM '+table+' WHERE DonHangID IN ('+orderIds.map(()=>'?').join(',')+')',orderIds);
      });
      const order=async(status,total,lines)=>{
        const id=await insert(`INSERT INTO DonHang(MaDonHang,MaYeuCauTao,KhachHangID,HoTenNguoiNhan,SoDienThoaiNhan,
          DiaChiNhan,PhuongThucID,TrangThai,TienHang,NgayDat,NgayHoanThanh,NgayHuy) VALUES (?, ?, ?, 'QA', '0901234567', 'QA', ?, ?, ?, ?, ?, ?)`,
          [randomUUID(),randomUUID(),customerId,method,status,total,inPeriod,status==='HOAN_THANH'?inPeriod:null,status==='DA_HUY'?inPeriod:null]);
        orderIds.push(id);
        const items=[];
        for(const [variant,quantity,price] of lines)items.push(await insert(`INSERT INTO ChiTietDonHang(DonHangID,BienTheID,MaSanPhamLucMua,
          SKULucMua,TenSanPhamLucMua,KichCoLucMua,MauSacLucMua,DanhMucLucMua,ThuongHieuLucMua,SoLuong,DonGia)
          VALUES (?, ?, 'QA', 'QA', 'QA', '40', 'QA', 'QA', 'QA', ?, ?)`,[id,variant,quantity,price]));
        return {id,items};
      };
      const pay=(order,amount,date)=>insert(`INSERT INTO ThanhToan(DonHangID,PhuongThucID,MaYeuCau,SoTien,TrangThai,NgayXacNhan)
        VALUES (?, ?, ?, ?, ?, ?)`,[order.id,method,randomUUID(),amount,date?'DA_XAC_NHAN':'CHO_XAC_NHAN',date]);
      const A=await order('HOAN_THANH',500,[[variantId,3,100],[secondVariant,2,100]]);
      const B=await order('HOAN_THANH',1000,[[otherVariant,1,1000]]);
      const cancelled=await order('DA_HUY',2000,[[otherVariant,2,1000]]);
      const unpaid=await order('DA_GIAO',3000,[[otherVariant,3,1000]]);
      const late=await order('HOAN_THANH',4000,[[otherVariant,4,1000]]);
      const payment=await pay(A,500,inPeriod);
      await pay(B,1000,inPeriod);
      await pay(cancelled,2000,null);
      await pay(unpaid,3000,null);
      await pay(late,4000,nextDay);
      const request=await insert(`INSERT INTO YeuCauDoiTra(MaYeuCau,MaChongLap,DonHangID,KhachHangID,LoaiYeuCau,LyDo,TrangThai)
        VALUES (?, ?, ?, ?, 'TRA', 'QA', 'HOAN_TAT')`,[randomUUID(),randomUUID(),A.id,customerId]);
      await query(`INSERT INTO ChiTietDoiTra(YeuCauDoiTraID,DonHangID,ChiTietDonHangID,SoLuongYeuCau,SoLuongDuyet,
        SoLuongDaNhan,TienHoanDuocDuyet,NgayNhanHang) VALUES (?, ?, ?, 1, 1, 1, 100, ?)`,[request,A.id,A.items[0],inPeriod]);
      for(const [amount,date] of [[30,inPeriod],[70,inPeriod],[50,null]])await query(`INSERT INTO HoanTien(DonHangID,ThanhToanID,YeuCauDoiTraID,
        MaYeuCau,LyDo,SoTien,TrangThai,BangChung,NguoiXuLyID,NgayXacNhan) VALUES (?, ?, ?, ?, 'QA', ?, ?, 'QA', ?, ?)`,
        [A.id,payment,request,randomUUID(),amount,date?'DA_XAC_NHAN':'CHO_XU_LY',managerId,date]);

      const summary=await manager.ok(range);
      assert.deepEqual(summary.daily,[{day:'2040-02-12',income:1500,refund:100,revenue:1400}]);
      assert.deepEqual([summary.income,summary.refund,summary.revenue],[1500,100,1400]);
      assert.equal(summary.currency,'VND');
      assert.ok(!Number.isNaN(Date.parse(summary.generatedAt)));
      assert.deepEqual(summary.orders.map(row=>row.count),[0,0,0,0,1,3,1]);
      assert.deepEqual(summary.top.map(row=>[row.quantity,row.amount]),[[4,400],[1,1000]]);
      const byValue=await manager.ok(range+'&type=best-sellers&rank=amount');
      assert.deepEqual(byValue.top.map(row=>row.amount),[1000,400]);
      assert.equal(byValue.daily,undefined);
      const byVariant=await manager.ok(range+'&type=best-sellers&group=variant');
      assert.equal(byVariant.top.length,3);
      const net=byVariant.top.find(row=>row.id===variantId);
      assert.deepEqual([net.quantity,net.amount],[2,200]);
      const following=await manager.ok(report+'?from=2040-02-13&to=2040-02-13&type=revenue');
      assert.deepEqual(following.daily,[{day:'2040-02-13',income:4000,refund:0,revenue:4000}]);
      const empty=await manager.ok(report+'?from=2040-02-14&to=2040-02-14');
      assert.deepEqual([empty.income,empty.refund,empty.revenue,empty.daily,empty.top],[0,0,0,[],[]]);
      assert.ok(empty.orders.every(row=>row.count===0));
      const orders=await manager.ok(range+'&type=orders');
      assert.deepEqual(orders.orders,summary.orders);
      assert.equal(orders.top,undefined);
      assert.equal((await manager.call(report+'?from=2040-02-30&to=2040-03-01')).status,400);
      assert.equal((await manager.call(report+'?from=2040-02-14&to=2040-02-13')).status,400);
      assert.equal((await manager.call(range+'&rank=invalid')).status,400);
      assert.equal((await customer.call(range)).status,403);
    });
  } finally {
    if(productId)await query("UPDATE SanPham SET TrangThai='NGUNG_BAN' WHERE SanPhamID=?",[productId]);
    // Test documents and ledger remain available for audit, rather than deleting transactional history.
    await pool.end();
  }
});
