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
export async function query(sql, args = []) { const [rows] = await pool.execute(sql, args); return rows; }
