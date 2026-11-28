import test from 'node:test';
import assert from 'node:assert/strict';
import {normalizeStockPath} from '../frontend/src/stock-navigation.js';

test('Đường dẫn kho cũ mở đúng phần và giữ dữ liệu chọn biến thể',()=>{
  const entry=new URL(normalizeStockPath('/manager/receipts?variant=18&type=DIEU_CHINH#draft'),'http://localhost');
  assert.equal(entry.pathname,'/manager/stock');
  assert.equal(entry.searchParams.get('view'),'receipts');
  assert.equal(entry.searchParams.get('variant'),'18');
  assert.equal(entry.searchParams.get('type'),'DIEU_CHINH');
  assert.equal(entry.hash,'#draft');
  assert.equal(normalizeStockPath('/manager/stock-history?variant=18'),'/manager/stock?variant=18&view=history');
  assert.equal(normalizeStockPath('/manager/stock?view=receipts'),'/manager/stock?view=receipts');
  assert.equal(normalizeStockPath('/manager/variants'),'/manager/variants');
});
