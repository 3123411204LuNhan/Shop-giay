import test from 'node:test';
import assert from 'node:assert/strict';
import {reportCSV} from '../frontend/src/report-export.js';

test('CSV ghi đúng kỳ, tiêu chí, dữ liệu đã hiển thị và vô hiệu công thức từ tên sản phẩm',()=>{
  const report={type:'summary',group:'variant',rank:'amount',from:'2040-02-12',to:'2040-02-13',
    generatedAt:'2040-02-14T00:00:00.000Z',currency:'VND',income:100,refund:150,revenue:-50,
    daily:[{day:'2040-02-12',income:100,refund:150,revenue:-50}],orders:[{status:'DA_HUY',count:0}],
    top:[{code:'QA,40',name:'=Đôi "QA"',quantity:1,amount:100}]};
  const csv=reportCSV(report,{DA_HUY:'Đã hủy'});
  for(const row of ['"Từ ngày","2040-02-12"','"Đến ngày","2040-02-13"','"Thời điểm tổng hợp (UTC)","2040-02-14T00:00:00.000Z"',
    '"Đơn vị tiền","VND"','"2040-02-12","100","150","-50"','"Đã hủy","0"',
    '"Tiêu chí xếp hạng","Theo biến thể · Xếp theo giá trị thực thu"',`"QA,40","'=Đôi ""QA""","1","100"`])assert.ok(csv.split('\r\n').includes(row),row);
});
