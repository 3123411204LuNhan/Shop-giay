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
  if (!url) return '';
  if (url.length<=1000 && !/\s/.test(url)) {
    if (/^\/images\/[^?#\\]+$/.test(url) && !url.split('/').includes('..')) return url;
    if (/^https?:\/\//i.test(url)) {
      const parsed=URL.parse(url);
      if (parsed?.hostname && !parsed.username && !parsed.password) return url;
    }
  }
  fail('Nhập URL ảnh HTTP(S) hoặc đường dẫn /images/ của cửa hàng.');
}
