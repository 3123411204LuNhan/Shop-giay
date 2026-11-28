import test from 'node:test';
import assert from 'node:assert/strict';
import 'dotenv/config';

test('Catalog BCE trả đúng dữ liệu hiển thị và từ chối bộ lọc sai', async () => {
  const api = `http://127.0.0.1:${process.env.PORT || 5173}/api`;
  const response = await fetch(api + '/catalog');
  assert.equal(response.status, 200);
  const catalog = await response.json();
  for (const key of ['products', 'categories', 'brands', 'sizes', 'colors'])
    assert.ok(Array.isArray(catalog[key]), key);
  const product = catalog.products[0];
  assert.ok(product);
  const detail = await (await fetch(api + '/products/' + product.id)).json();
  assert.equal(detail.code, product.code);
  assert.equal(detail.price, product.price);
  assert.ok(Array.isArray(detail.variants));
  assert.ok(Array.isArray(detail.reviews));
  assert.equal((await fetch(api + '/catalog?brand=bad')).status, 400);
});
