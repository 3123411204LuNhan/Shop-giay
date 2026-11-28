
import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';
import {readGuestCart,writeGuestCart,clearGuestCart} from '../../../frontend/src/guest-cart.js';

const storage=()=>{const values=new Map();return {getItem:k=>values.get(k)??null,setItem:(k,v)=>values.set(k,v),removeItem:k=>values.delete(k)};};
test('Giỏ khách không đọc giỏ localStorage cũ và được dọn khi chuyển tài khoản',()=>{
 const shared=storage(),tabA=storage(),tabB=storage();shared.setItem('sole-cart',JSON.stringify([{variantId:1,quantity:2}]));
 assert.deepEqual(readGuestCart(tabB,shared),[]);assert.equal(shared.getItem('sole-cart'),null);
 writeGuestCart([{variantId:2,quantity:2}],tabA);assert.equal(readGuestCart(tabA,shared)[0].quantity,2);assert.deepEqual(readGuestCart(tabB,shared),[]);
 clearGuestCart(tabA,shared);assert.deepEqual(readGuestCart(tabA,shared),[]);
});

test('A giữ 2 đôi trong giỏ, B đăng nhập cùng trình duyệt không nhận giỏ A; tab A cũ không ghi vào B',async()=>{
 const base=`http://127.0.0.1:${process.env.PORT||5173}/api`,jar=new Map();
 async function call(path,body,account){const response=await fetch(base+path,{method:body?'POST':'GET',headers:{'Content-Type':'application/json','X-Sole-Request':'1','X-Sole-Area':'customer',Cookie:[...jar].map(([k,v])=>`${k}=${v}`).join('; '),...(account?{'X-Sole-Account':String(account)}:{})},body:body?JSON.stringify(body):undefined});for(const cookie of response.headers.getSetCookie()){const pair=cookie.split(';')[0],at=pair.indexOf('=');if(pair.slice(at+1))jar.set(pair.slice(0,at),pair.slice(at+1));else jar.delete(pair.slice(0,at));}return {status:response.status,data:await response.json()};}
 async function putCart(variantId,quantity,account){const response=await fetch(base+'/cart',{method:'PUT',headers:{'Content-Type':'application/json','X-Sole-Request':'1','X-Sole-Area':'customer',Cookie:[...jar].map(([k,v])=>`${k}=${v}`).join('; '),'X-Sole-Account':String(account)},body:JSON.stringify({variantId,quantity})});return {status:response.status,data:await response.json()};}
 const a={name:'QA Giỏ A',email:`cart-a-${randomUUID()}@example.test`,password:'CartOwner@123'},b={name:'QA Giỏ B',email:`cart-b-${randomUUID()}@example.test`,password:'CartOwner@123'};
 assert.equal((await call('/auth/register',a)).status,200);const aid=(await call('/me')).data.user.id;
 const products=(await call('/catalog')).data.products;const detail=(await call('/products/'+products[0].id)).data;const variant=detail.variants.find(v=>v.stock>=3);
 assert.equal((await putCart(variant.id,2,aid)).status,200);assert.equal((await call('/cart',undefined,aid)).data[0].quantity,2);
 assert.equal((await call('/auth/logout',{},aid)).status,200);
 assert.equal((await call('/auth/register',b)).status,200);const bid=(await call('/me')).data.user.id;
 assert.deepEqual((await call('/cart',undefined,bid)).data,[]);
 const stale=await putCart(variant.id,3,aid);assert.equal(stale.status,409);assert.equal(stale.data.code,'SESSION_CHANGED');assert.deepEqual((await call('/cart',undefined,bid)).data,[]);
 assert.equal((await putCart(variant.id,1,bid)).status,200);
 await call('/auth/logout',{},bid);assert.equal((await call('/auth/login',{login:a.email,password:a.password})).status,200);
 assert.equal((await call('/cart',undefined,aid)).data[0].quantity,2);
 await call('/auth/logout',{},aid);assert.equal((await call('/auth/login',{login:b.email,password:b.password})).status,200);
 assert.equal((await call('/cart',undefined,bid)).data[0].quantity,1);
});

test('Chủ shop và kỹ thuật không được đăng nhập/mua hàng ở khu vực khách',async()=>{
 const base=`http://127.0.0.1:${process.env.PORT||5173}/api`;
 async function login(area,login){const response=await fetch(base+'/auth/login',{method:'POST',headers:{'Content-Type':'application/json','X-Sole-Request':'1','X-Sole-Area':area},body:JSON.stringify({login,password:'SoleDemo@123'})});return {status:response.status,cookies:response.headers.getSetCookie(),body:await response.json()};}
 for(const [area,email]of [['manager','shop@sole.vn'],['admin','admin@sole.vn']]){
  const denied=await login('customer',email);assert.equal(denied.status,403);assert.ok(!denied.cookies.some(x=>x.startsWith('sole_customer_session=')&&!x.startsWith('sole_customer_session=;')));
  const own=await login(area,email);assert.equal(own.status,200);const cookie=own.cookies.map(x=>x.split(';')[0]).join('; ');
  for(const route of ['/cart','/orders','/returns']){const r=await fetch(base+route,{headers:{Cookie:cookie,'X-Sole-Area':area}});assert.equal(r.status,403,route);}
  if(area==='manager'){const r=await fetch(base+'/manage/orders',{headers:{Cookie:cookie,'X-Sole-Area':area}});assert.equal(r.status,200);const orders=await r.json();if(orders[0])assert.equal((await fetch(base+'/orders/'+orders[0].id,{headers:{Cookie:cookie,'X-Sole-Area':area}})).status,200);}
 }
});
