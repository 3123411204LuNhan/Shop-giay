import 'dotenv/config';
import mysql from 'mysql2/promise';
import { readFile } from 'node:fs/promises';
import { config } from '../server/db.js';
const name=config.database;
if(!/^[a-zA-Z0-9_]+$/.test(name))throw new Error('DB_NAME chỉ được chứa chữ, số và dấu gạch dưới.');
const c=await mysql.createConnection({...config,database:undefined,multipleStatements:true});
try {
  await c.query(`CREATE DATABASE IF NOT EXISTS \`${name}\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci`);
  await c.changeUser({database:name});
  const [[{count}]]=await c.query('SELECT COUNT(*) count FROM information_schema.tables WHERE table_schema=?',[name]);
  if(count) throw new Error('Database đã có bảng. Không ghi đè; chọn DB_NAME mới hoặc dùng npm run db:seed nếu schema đã đầy đủ.');
  await c.query(await readFile(new URL('../database/schema.sql',import.meta.url),'utf8'));
  console.log('Đã tạo đủ 37 bảng MySQL trong '+name+'.');
} finally {await c.end();}
