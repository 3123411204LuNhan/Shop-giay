import {query, tx, fail, positive, text} from './db.js';
import {audit} from './auth.js';

function amount(raw, label, min, max) {
  const value = Number(raw);
  if (raw == null || String(raw).trim() === '' || !Number.isSafeInteger(value) || value < min || value > max) {
    fail(`${label} phải là số nguyên từ ${min} đến ${max}.`);
  }
  return value;
}

function promotionFields(body, previousMaximum = null) {
  const name = text(body.name);
  if (!name) fail('Vui lòng nhập tên chương trình.');
  if (!['SO_TIEN', 'PHAN_TRAM'].includes(body.type)) fail('Hình thức giảm không hợp lệ.');
  const raw = body.maximum === undefined ? previousMaximum : body.maximum;
  const maximum = body.type === 'SO_TIEN' || raw == null || String(raw).trim() === ''
    ? null : amount(raw, 'Giảm tối đa', 1, Number.MAX_SAFE_INTEGER);
  return [name, body.type, amount(body.value, 'Giá trị giảm', 1, body.type === 'PHAN_TRAM' ? 100 : 1e10),
    amount(body.minimum ?? 0, 'Đơn tối thiểu', 0, Number.MAX_SAFE_INTEGER), maximum];
}

function dateValue(value) {
  if (value instanceof Date) return value;
  // Older clients sent datetime-local values explicitly labelled UTC.
  const raw = String(value ?? '').trim();
  const normalized = /^\d{4}-\d\d-\d\d[T ]\d\d:\d\d(:\d\d)?$/.test(raw) ? raw.replace(' ', 'T') + 'Z' : raw;
  const date = new Date(normalized);
  if (!Number.isFinite(date.getTime())) fail('Ngày giờ chương trình không hợp lệ.');
  return date;
}

function schedule(start, end, status) {
  const dates = [dateValue(start), dateValue(end)];
  if (dates[1] <= dates[0]) fail('Ngày kết thúc phải sau ngày bắt đầu.');
  if (status === 'DANG_AP_DUNG' && dates[1].getTime() <= Date.now()) {
    fail('Chương trình đã hết hạn. Hãy chọn ngày kết thúc mới trước khi kích hoạt.');
  }
  return dates;
}

async function lockedPromotion(id, connection) {
  const [promo] = await query('SELECT * FROM KhuyenMai WHERE KhuyenMaiID=? FOR UPDATE', [id], connection);
  if (!promo) fail('Không có chương trình.', 404);
  return promo;
}

export async function createPromotion(req) {
  const code = text(req.body.code, 50);
  if (!code) fail('Vui lòng nhập mã chương trình.');
  const fields = promotionFields(req.body);
  const status = req.body.status ?? 'NHAP';
  if (!['NHAP','DANG_AP_DUNG'].includes(status)) fail('Trạng thái tạo chương trình không hợp lệ.');
  const dates = schedule(req.body.start, req.body.end, status);
  return tx(async c => {
    const result = await query("INSERT INTO KhuyenMai(MaKhuyenMai,TenKhuyenMai,HinhThuc,GiaTri,GiaTriDonToiThieu,GiamToiDa,PhamVi,BatDauLuc,KetThucLuc,NguoiTaoID,TrangThai) VALUES(?,?,?,?,?,?,'TOAN_BO',?,?,?,?)", [code, ...fields, ...dates, req.user.id,status], c);
    await query("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,'TAO',?)", [result.insertId, req.user.id, fields[0]], c);
    return {id: result.insertId};
  });
}

export async function updatePromotion(req) {
  const id = positive(req.params.id), {status} = req.body;
  if (!['NHAP','DANG_AP_DUNG','KET_THUC'].includes(status)) fail('Trạng thái không hợp lệ.');
  return tx(async c => {
    const promo = await lockedPromotion(id, c);
    const dates = schedule(req.body.start ?? promo.BatDauLuc, req.body.end ?? promo.KetThucLuc, status);
    if ('name' in req.body) {
      await query('UPDATE KhuyenMai SET TenKhuyenMai=?,HinhThuc=?,GiaTri=?,GiaTriDonToiThieu=?,GiamToiDa=? WHERE KhuyenMaiID=?', [...promotionFields(req.body,promo.HinhThuc==='PHAN_TRAM'?promo.GiamToiDa:null), id], c);
    }
    await query('UPDATE KhuyenMai SET TrangThai=?,BatDauLuc=?,KetThucLuc=?,NgayCapNhat=UTC_TIMESTAMP() WHERE KhuyenMaiID=?', [status, ...dates, id], c);
    await query('INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,?,?)', [id, req.user.id, status, dates.map(d => d.toISOString()).join(' → ')], c);
    return {ok:true};
  });
}

export async function deleteDraft(req) {
  const id = positive(req.params.id);
  return tx(async c => {
    const promo = await lockedPromotion(id, c);
    if (promo.TrangThai !== 'NHAP') fail('Chỉ được xóa bản nháp.', 409);
    const used = await query('SELECT 1 FROM KhuyenMaiDonHang WHERE KhuyenMaiID=? LIMIT 1', [id], c);
    if (used.length) fail('Mã đã xuất hiện trong đơn hàng. Hãy kết thúc chương trình để giữ lịch sử.', 409);
    for (const table of ['KhuyenMaiSanPham','KhuyenMaiDanhMuc','KhuyenMaiThuongHieu','LichSuKhuyenMai']) {
      await query(`DELETE FROM ${table} WHERE KhuyenMaiID=?`, [id], c);
    }
    await query('DELETE FROM KhuyenMai WHERE KhuyenMaiID=?', [id], c);
    await audit(req.user, 'XOA_NHAP', 'KhuyenMai', id, promo.MaKhuyenMai, c);
    return {ok:true};
  });
}
