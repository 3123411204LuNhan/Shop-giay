import test from 'node:test';
import assert from 'node:assert/strict';
import {areaForPath} from '../frontend/src/session-area.js';

test('Giao diện chọn khu vực phiên theo đường dẫn',()=>{
  assert.equal(areaForPath('/admin/accounts'),'admin');
  assert.equal(areaForPath('/manager/orders'),'manager');
  assert.equal(areaForPath('/account'),'customer');
  assert.equal(areaForPath('/administrator'),'customer');
});
