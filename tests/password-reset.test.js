import test from 'node:test';
import assert from 'node:assert/strict';
import {randomUUID,createHash} from 'node:crypto';
import {readFile,readdir} from 'node:fs/promises';
import {join} from 'node:path';
import {pool,query,config} from '../server/db.js';
import {localMailbox} from '../server/mail.js';
const base=`http://127.0.0.1:${process.env.PORT||5173}/api`;
const hash=token=>createHash('sha256').update(token).digest();
async function call(path,body,cookie=''){const res=await fetch(base+path,{method:body?'POST':'GET',headers:{'Content-Type':'application/json','X-Sole-Request':'1','X-Sole-Area':'customer',Cookie:cookie},body:body?JSON.stringify(body):undefined});return {status:res.status,data:await res.json(),cookie:res.headers.getSetCookie().at(-1)?.split(';')[0]};}
async function createCustomer(){const email=`otp-${randomUUID()}@example.test`;const result=await call('/auth/register',{email,name:'QA OTP',password:'BeforeOtp@123'});assert.equal(result.status,200);return {email,cookie:result.cookie};}
async function readCode(challenge){const files=(await readdir(localMailbox)).filter(x=>x.startsWith('reset-')&&x.includes(challenge.slice(0,8)));assert.equal(files.length,1);const body=await readFile(join(localMailbox,files[0]),'utf8');const code=body.match(/Mã xác minh: (\d{6})/)[1];assert.ok(body.includes(challenge));return code;}
test('OTP: local fallback, xác minh, giới hạn thử, đặt mật khẩu và thu hồi phiên',async t=>{
 assert.match(config.database,/^sole_studio_nhom19/);
 try{
  const user=await createCustomer();const forgot=await call('/auth/forgot',{email:user.email});assert.equal(forgot.status,200);assert.ok(/^[a-f0-9]{64}$/.test(forgot.data.challenge));assert.equal(forgot.data.code,undefined);assert.equal(forgot.data.delivery,undefined);const challenge=forgot.data.challenge,code=await readCode(challenge);
  await t.test('Không thể dùng challenge để bỏ qua bước OTP',async()=>{assert.equal((await call('/auth/reset',{token:challenge,password:'AfterOtp@123'})).status,400);});
  await t.test('Gửi lại có giới hạn 60 giây; lần nhập sai được lưu thật',async()=>{assert.equal((await call('/auth/forgot',{email:user.email})).status,429);const wrong=code==='000000'?'999999':'000000';assert.equal((await call('/auth/verify-otp',{challenge,code:wrong})).status,400);const [row]=await query('SELECT SoLanThu,MaXacMinhHash FROM DatLaiMatKhau WHERE TokenHash=?',[hash(challenge)]);assert.equal(row.SoLanThu,1);assert.equal(row.MaXacMinhHash.length,32);});
  let token;
  await t.test('OTP đúng cấp quyền đặt lại một lần, OTP không được dùng lại',async()=>{const verified=await call('/auth/verify-otp',{challenge,code});assert.equal(verified.status,200);token=verified.data.resetToken;assert.equal((await call('/auth/verify-otp',{challenge,code})).status,400);});
  await t.test('Đổi mật khẩu thành công, thu hồi phiên cũ, chặn dùng lại token',async()=>{assert.equal((await call('/auth/reset',{token,password:'AfterOtp@123'})).status,200);assert.equal((await call('/me',undefined,user.cookie)).data.user,null);assert.equal((await call('/auth/login',{login:user.email,password:'AfterOtp@123'})).status,200);assert.equal((await call('/auth/reset',{token,password:'OtherOtp@123'})).status,400);});
  await t.test('Nhập sai 5 lần thì mã bị khóa kể cả khi sau đó nhập đúng',async()=>{const u=await createCustomer();const {data}=await call('/auth/forgot',{email:u.email});const right=await readCode(data.challenge),wrong=right==='000000'?'999999':'000000';for(let i=0;i<5;i++)assert.equal((await call('/auth/verify-otp',{challenge:data.challenge,code:wrong})).status,400);assert.equal((await call('/auth/verify-otp',{challenge:data.challenge,code:right})).status,400);const [r]=await query('SELECT SoLanThu,DaSuDungLuc FROM DatLaiMatKhau WHERE TokenHash=?',[hash(data.challenge)]);assert.equal(r.SoLanThu,5);assert.ok(r.DaSuDungLuc);});
  await t.test('Email không có tài khoản ghi thư local nhưng không cho đặt mật khẩu',async()=>{const email=`unknown-${randomUUID()}@example.test`;const {data,status}=await call('/auth/forgot',{email});assert.equal(status,200);const code=await readCode(data.challenge);assert.equal((await call('/auth/verify-otp',{challenge:data.challenge,code})).status,400);const [{n}]=await query('SELECT COUNT(*) n FROM TaiKhoan WHERE Email=?',[email]);assert.equal(n,0);});
 }finally{await pool.end();}
});
