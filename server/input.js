import { randomBytes } from 'node:crypto';
import { fail, text } from './db.js';

export const required = (v, label) => {
  const s = text(v);
  if (!s) fail(`Vui lòng nhập ${label}.`);
  return s;
};
export const price = (v) => {
  const n = Number(v);
  if (!Number.isFinite(n) || n <= 0 || n > 1e10) fail('Giá tiền không hợp lệ.');
  return Math.round(n);
};
export const code = (prefix) =>
  prefix +
  '-' +
  Date.now().toString(36).toUpperCase() +
  '-' +
  randomBytes(2).toString('hex').toUpperCase();

// CatalogService và ReturnService dùng chung kiểm tra đường dẫn ảnh.
export function productImage(value) {
  const url = String(value ?? '').trim();
  if (url && (url.length > 1000 || !/^(https?:\/\/[^\s/]+|\/images\/[^\s]+)/i.test(url)))
    fail('Nhập URL ảnh HTTP(S) hoặc đường dẫn /images/ của cửa hàng.');
  return url;
}
