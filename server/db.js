import 'dotenv/config';
import mysql from 'mysql2/promise';
export const config = {
  host: process.env.DB_HOST || '127.0.0.1', port: Number(process.env.DB_PORT || 3306),
  user: process.env.DB_USER || 'root', password: process.env.DB_PASSWORD || '',
  database: process.env.DB_NAME || 'sole_studio_nhom19', timezone: 'Z',
  decimalNumbers: true, connectionLimit: 10, waitForConnections: true, connectTimeout: 5000,
};
export const pool = mysql.createPool(config);
pool.on('connection', c => c.query("SET time_zone = '+00:00'"));
export async function query(sql, args = [], connection = pool) { const [rows] = await connection.execute(sql, args); return rows; }
export async function tx(fn) {
  const c = await pool.getConnection();
  try { await c.beginTransaction(); const result = await fn(c); await c.commit(); return result; }
  catch (e) { await c.rollback(); throw e; } finally { c.release(); }
}
export function fail(message, status = 400) { const e = new Error(message); e.status = status; throw e; }
export function positive(value, name = 'Số lượng') { const n = Number(value); if (!Number.isSafeInteger(n) || n < 1 || n > 100000) fail(`${name} không hợp lệ.`); return n; }
export function text(value, max = 1000) { return String(value ?? '').trim().slice(0, max); }
