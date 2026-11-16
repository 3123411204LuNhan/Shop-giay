import test from 'node:test';
import assert from 'node:assert/strict';
import { SESSION_AREAS, requestArea, loginArea, sessionCookieOptions, validSessionToken } from '../server/session-cookie.js';
import { areaForPath } from '../src/session-area.js';

test('HTTP local giữ được cookie dù NODE_ENV=production; HTTPS vẫn dùng Secure',()=>{
  const previous=process.env.NODE_ENV;
  try {
    process.env.NODE_ENV='production';
    assert.deepEqual(sessionCookieOptions({secure:false}),{httpOnly:true,sameSite:'strict',secure:false,path:'/'});
    assert.equal(sessionCookieOptions({secure:true}).secure,true);
  } finally {if(previous===undefined)delete process.env.NODE_ENV;else process.env.NODE_ENV=previous;}
});
test('Ba khu vực có cookie riêng và chỉ đăng nhập vào vai trò được cấp',()=>{
  assert.equal(new Set(Object.values(SESSION_AREAS).map(x=>x.cookie)).size,3);
  assert.equal(loginArea(['SYSTEM_ADMIN'],'customer'),'admin');
  assert.equal(loginArea(['SYSTEM_ADMIN'],'manager'),null);
  assert.equal(loginArea(['CUSTOMER'],'admin'),null);
  assert.equal(loginArea(['CUSTOMER','SHOP_MANAGER'],'manager'),'manager');
});
test('Chọn đúng phiên khi đổi URL và bỏ qua header khu vực không hợp lệ',()=>{
  assert.equal(areaForPath('/admin/accounts'),'admin');
  assert.equal(areaForPath('/manager/orders'),'manager');
  assert.equal(areaForPath('/account'),'customer');
  assert.equal(areaForPath('/administrator'),'customer');
  assert.equal(requestArea({get:()=> 'manager',path:'/me'}),'manager');
  assert.equal(requestArea({get:()=> '__proto__',path:'/admin/accounts'}),'admin');
});
test('Bỏ cookie sai kiểu/định dạng thay vì làm hỏng yêu cầu đăng nhập',()=>{
  assert.equal(validSessionToken('a'.repeat(64)),true);
  for(const v of [undefined,[],{},'bad-token','a'.repeat(10000)])assert.equal(validSessionToken(v),false);
});
