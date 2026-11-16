import test from 'node:test';
import assert from 'node:assert/strict';
import {readFile} from 'node:fs/promises';
import {resolve} from 'node:path';
import {randomUUID} from 'node:crypto';
import {smtpConfig,deliverResetCode} from '../server/mail.js';

const env={SMTP_HOST:'smtp.gmail.com',SMTP_PORT:'465',SMTP_SECURE:'true',SMTP_USER:'sender@gmail.com',SMTP_APP_PASSWORD:'abcd efgh ijkl mnop',MAIL_FALLBACK_LOCAL:'true',APP_ORIGIN:'http://localhost:5173'};
const payload=()=>({email:'recipient@gmail.com',code:'012345',challenge:randomUUID().replaceAll('-','').repeat(2),accountExists:true});
test('Gmail SMTP dùng TLS và App Password, không dùng mật khẩu đăng nhập web',()=>{const c=smtpConfig(env);assert.equal(c.host,'smtp.gmail.com');assert.equal(c.secure,true);assert.equal(c.auth.pass,'abcdefghijklmnop');assert.equal(smtpConfig({}),null);});
test('SMTP chấp nhận người nhận: gửi mã 6 số, không ghi hộp thư local',async()=>{let sent;const result=await deliverResetCode(payload(),{env,transport:{sendMail:async mail=>{sent=mail;return {accepted:['recipient@gmail.com']};}}});assert.equal(result.channel,'smtp');assert.match(sent.text,/012345/);assert.equal(sent.subject,'SOLE STUDIO — Mã xác minh đặt lại mật khẩu');assert.equal(result.file,undefined);});
test('SMTP từ chối hoặc lỗi: mã được lưu local, không ghi mật khẩu SMTP',async()=>{const p=payload(),directory=resolve('.local/test-mails',randomUUID());const result=await deliverResetCode(p,{env,directory,transport:{sendMail:async()=>{throw Object.assign(new Error(env.SMTP_APP_PASSWORD),{code:'EAUTH'});}}});assert.equal(result.channel,'local');const {readdir}=await import('node:fs/promises');const [file]=await readdir(directory);const body=await readFile(resolve(directory,file),'utf8');assert.match(body,/Mã xác minh: 012345/);assert.match(body,/SMTP_EAUTH/);assert.ok(!body.includes(env.SMTP_APP_PASSWORD));});
test('Email chưa đăng ký hoặc miền test chỉ ghi local, không gửi thư ra ngoài',async()=>{for(const changes of [{accountExists:false},{email:'qa@example.test'}]){let sent=false;const directory=resolve('.local/test-mails',randomUUID());const p={...payload(),...changes};const result=await deliverResetCode(p,{env,directory,transport:{sendMail:async()=>{sent=true;return {accepted:[p.email]};}}});assert.equal(result.channel,'local');assert.equal(sent,false);}});
