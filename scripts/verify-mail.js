import 'dotenv/config';
import nodemailer from 'nodemailer';
import {smtpConfig} from '../server/mail.js';
const config=smtpConfig();
if(!config){console.log('Chưa cấu hình đủ SMTP_HOST, SMTP_USER và SMTP_APP_PASSWORD (hoặc SMTP_PASSWORD) trong .env.');process.exitCode=1;}
else{try{await nodemailer.createTransport(config).verify();console.log('Kết nối và xác thực SMTP thành công. Chưa gửi email nào.');}catch(error){console.log('Chưa kết nối/xác thực được SMTP. Mã lỗi:',String(error.code||'SMTP_ERROR'));process.exitCode=1;}}
