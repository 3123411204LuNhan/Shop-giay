import { ReportRepository } from './ReportRepository.js';
import { fail, tx } from '../db.js';
import { publicStoreConfig } from '../store-config.js';

// ReportController chuyển yêu cầu tới các phương thức xử lý cấu hình công khai, khách hàng, doanh thu và giám sát hệ thống.
const repository = new ReportRepository();
const dayMs = 86400000;
const shopOffset = 7 * 3600000;
const statuses = ['CHO_XAC_NHAN','DA_XAC_NHAN','DANG_CHUAN_BI','DANG_GIAO','DA_GIAO','HOAN_THANH','DA_HUY'];

function dayStart(value) {
  if (typeof value !== 'string' || !/^[1-9]\d{3}-\d{2}-\d{2}$/.test(value)) fail('Khoảng ngày không hợp lệ.');
  const midnight = Date.parse(value+'T00:00:00Z');
  if (!Number.isFinite(midnight) || new Date(midnight).toISOString().slice(0,10) !== value)
    fail('Khoảng ngày không hợp lệ.');
  return midnight-shopOffset;
}

export class ReportService {
  async settings() {
    return publicStoreConfig();
  }

  async customers() {
    return await repository.customers();
  }

  async sales(r) {
    const now = Date.now()+shopOffset;
    const {
      from=new Date(now-30*dayMs).toISOString().slice(0,10),
      to=new Date(now).toISOString().slice(0,10),
      type='summary', group='product', rank='quantity',
    } = r.query;
    const start = dayStart(from), end = dayStart(to);
    if (start>end) fail('Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.');
    if (!['summary','revenue','orders','best-sellers'].includes(type) ||
        !['product','variant'].includes(group) || !['quantity','amount'].includes(rank))
      fail('Loại hoặc tiêu chí báo cáo không hợp lệ.');
    const args = [new Date(start),new Date(end+dayMs)];

    // Mục 3.9: một bản báo cáo chỉ được trả nếu đọc thành công mọi dữ liệu thuộc loại đã chọn.
    return tx(async connection=>{
      const report={from,to,type,group,rank,currency:'VND',generatedAt:new Date().toISOString()};
      if (type==='summary' || type==='revenue') {
        report.daily=await repository.cashByDay(args,connection);
        const sum=key=>report.daily.reduce((total,row)=>total+Math.round(row[key]*100),0);
        const income=sum('income'), refund=sum('refund');
        Object.assign(report,{income:income/100,refund:refund/100,revenue:(income-refund)/100});
      }
      if (type==='summary' || type==='orders') {
        const counts=await repository.orderCounts(args,connection);
        report.orders=statuses.map(status=>({status,count:counts.find(row=>row.status===status)?.count??0}));
      }
      if (type==='summary' || type==='best-sellers')
        report.top=await repository.bestSellers(args,group,rank,connection);
      if (type==='summary') report.lowStock=(await repository.lowStock(connection))[0].n;
      return report;
    });
  }

  async logs() {
    return await repository.logs();
  }

  async system() {
    return {
      database: (await repository.databaseInfo())[0],
      tables: (await repository.tableCount())[0].n,
      sessions: (await repository.activeSessions())[0].n,
      accounts: (await repository.accountCount())[0].n,
    };
  }
}
