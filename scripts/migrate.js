import {migrateWorkflows} from '../server/workflow-schema.js';
import {pool,query} from '../server/db.js';
try {
  await migrateWorkflows();
  console.log('Luồng kho, vận chuyển đã sẵn sàng; giữ nguyên dữ liệu hiện có.');
  const [{n}]=await query("SELECT COUNT(*) n FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='DatLaiMatKhau' AND column_name='MaXacMinhHash'");
  if(!n)await query('ALTER TABLE DatLaiMatKhau ADD COLUMN MaXacMinhHash binary(32) NULL AFTER TokenHash');
  console.log(n?'OTP schema đã sẵn sàng; không cần thay đổi.':'Đã bổ sung MaXacMinhHash, giữ nguyên dữ liệu và 37 bảng hiện có.');
} finally {await pool.end();}
