import { ReportRepository } from './ReportRepository.js';
import { fail, text } from '../db.js';
import { publicStoreConfig } from '../store-config.js';

// ReportController chuyển yêu cầu tới các phương thức xử lý cấu hình công khai, khách hàng, doanh thu và giám sát hệ thống.
const repository = new ReportRepository();

export class ReportService {
  async settings() {
    return publicStoreConfig();
  }

  async customers() {
    return await repository.customers();
  }

  async sales(r) {
    const from = text(r.query.from) || new Date(Date.now() - 30 * 86400000).toISOString().slice(0, 10),
      to = text(r.query.to) || new Date().toISOString().slice(0, 10);
    if (!/^\d{4}-\d{2}-\d{2}$/.test(from) || !/^\d{4}-\d{2}-\d{2}$/.test(to) || from > to)
      fail('Khoảng ngày không hợp lệ.');
    const args = [
      new Date(from + 'T00:00:00+07:00'),
      new Date(new Date(to + 'T00:00:00+07:00').getTime() + 86400000),
    ];
    const [income] = await repository.income(args);
    const [refund] = await repository.refunded(args);
    return {
      from,
      to,
      income: income.value,
      refund: refund.value,
      revenue: income.value - refund.value,
      orders: await repository.orderCounts(args),
      daily: await repository.dailyIncome(args),
      top: await repository.bestSellers(args),
      lowStock: (await repository.lowStock())[0].n,
    };
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
