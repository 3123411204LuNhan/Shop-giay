import 'dotenv/config';
import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID} from 'node:crypto';

test('Cùng trình duyệt: admin, chủ shop và khách giữ phiên độc lập',async()=>{
  const jar=new Map();
  const base=`http://127.0.0.1:${process.env.PORT||5173}/api`;
  async function call(area,path,body){
    const res=await fetch(base+path,{method:body?'POST':'GET',headers:{'Content-Type':'application/json','X-Sole-Request':'1','X-Sole-Area':area,Cookie:[...jar].map(([k,v])=>`${k}=${v}`).join('; ')},body:body?JSON.stringify(body):undefined});
    const cookies=res.headers.getSetCookie();
    for(const cookie of cookies){const [pair]=cookie.split(';');const equal=pair.indexOf('=');const name=pair.slice(0,equal),value=pair.slice(equal+1);if(value)jar.set(name,value);else jar.delete(name);}
    return {status:res.status,data:await res.json(),cookies};
  }
  const admin=await call('admin','/auth/login',{login:'admin@sole.vn',password:'SoleDemo@123'});
  assert.equal(admin.status,200);assert.equal(admin.data.area,'admin');
  assert.match(admin.cookies.join(';'),/sole_admin_session=/);
  assert.match(admin.cookies.join(';'),/HttpOnly/i);assert.match(admin.cookies.join(';'),/SameSite=Strict/i);
  assert.doesNotMatch(admin.cookies.join(';'),/; Secure/i,'HTTP local không được buộc cookie Secure.');
  assert.equal((await call('customer','/me')).data.user,null);
  assert.equal((await call('manager','/me')).data.user,null);
  const adminId=(await call('admin','/me')).data.user.id;
  assert.equal((await call('__proto__','/admin/accounts')).status,200);

  assert.equal((await call('customer','/auth/register',{email:`isolation-${randomUUID()}@example.test`,name:'QA Phiên khách',password:'IsolationTest@123'})).status,200);
  assert.equal((await call('manager','/auth/login',{login:'shop@sole.vn',password:'SoleDemo@123'})).status,200);
  assert.deepEqual((await call('customer','/me')).data.user.roles,['CUSTOMER']);
  assert.deepEqual((await call('manager','/me')).data.user.roles,['SHOP_MANAGER']);
  assert.equal((await call('admin','/me')).data.user.id,adminId);
  assert.equal(jar.size,3);

  // A rejected login cannot replace either the manager cookie or admin session.
  const managerToken=jar.get('sole_manager_session');
  assert.equal((await call('manager','/auth/login',{login:'admin@sole.vn',password:'SoleDemo@123'})).status,403);
  assert.equal(jar.get('sole_manager_session'),managerToken);
  assert.equal((await call('admin','/me')).data.user.id,adminId);

  const logout=await call('customer','/auth/logout',{});
  assert.equal(logout.status,200);
  assert.match(logout.cookies.join(';'),/sole_customer_session=;/);
  assert.equal((await call('customer','/me')).data.user,null);
  assert.ok((await call('manager','/me')).data.user);
  assert.equal((await call('admin','/me')).data.user.id,adminId);

  jar.set('sole_customer_session','invalid-cookie');
  const invalid=await call('customer','/me');
  assert.equal(invalid.status,200);assert.equal(invalid.data.user,null);
  assert.equal(jar.has('sole_customer_session'),false);
  assert.ok((await call('admin','/me')).data.user);
  await call('manager','/auth/logout',{});
  assert.equal((await call('manager','/me')).data.user,null);
  assert.equal((await call('admin','/me')).data.user.id,adminId);
  await call('admin','/auth/logout',{});
  assert.equal((await call('admin','/me')).data.user,null);
});
