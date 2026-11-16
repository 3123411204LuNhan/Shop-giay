import { randomBytes, randomInt, timingSafeEqual } from 'node:crypto';
import { query, tx, fail, text } from './db.js';
import { hashPassword, tokenHash } from './auth.js';
import { deliverResetCode } from './mail.js';

const invalid = 'Mã xác minh không hợp lệ, đã hết hạn hoặc đã được sử dụng.';
const otpHash = (challenge, code) => tokenHash(challenge + ':' + code);
const challenges = new Map();

export async function requestPasswordReset(rawEmail) {
  const email = text(rawEmail, 254).toLowerCase();
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) fail('Vui lòng nhập địa chỉ email hợp lệ.');
  const now = Date.now(), emailKey = tokenHash(email).toString('hex');
  for (const [key, at] of challenges) if (now - at >= 60000) challenges.delete(key);
  if (challenges.has(emailKey)) fail('Vui lòng chờ 60 giây trước khi yêu cầu mã mới.',429);
  challenges.set(emailKey, now);
  const challenge = randomBytes(32).toString('hex');
  const code = String(randomInt(0, 1000000)).padStart(6, '0');
  let accountId = null;
  try {
    await tx(async c => {
      const [user] = await query("SELECT TaiKhoanID id FROM TaiKhoan WHERE Email=? AND TrangThai='HOAT_DONG' FOR UPDATE", [email], c);
      if (!user) return;
      accountId = user.id;
      const [recent] = await query('SELECT YeuCauID FROM DatLaiMatKhau WHERE TaiKhoanID=? AND MaXacMinhHash IS NOT NULL AND NgayTao>DATE_SUB(UTC_TIMESTAMP(),INTERVAL 60 SECOND) LIMIT 1', [user.id], c);
      if (recent) fail('Vui lòng chờ 60 giây trước khi yêu cầu mã mới.',429);
      await query('UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL', [user.id], c);
      await query('INSERT INTO DatLaiMatKhau(TaiKhoanID,TokenHash,MaXacMinhHash,HetHanLuc) VALUES(?,?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 10 MINUTE))', [user.id,tokenHash(challenge),otpHash(challenge,code)], c);
    });
    try {
      await deliverResetCode({email,code,challenge,accountExists:Boolean(accountId)});
    } catch (error) {
      // A failed delivery without fallback must not leave an active code behind.
      if (accountId) await query('UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TokenHash=?', [tokenHash(challenge)]);
      throw error;
    }
  } catch (error) {if(error.status!==429)challenges.delete(emailKey);throw error;}
  // No OTP, delivery-channel or account-existence disclosure in public JSON.
  return {challenge,expiresIn:600,resendAfter:60,message:'Yêu cầu đã được xử lý. Kiểm tra email (kể cả Spam); nếu không gửi được, xem hộp thư local trên máy chạy server.'};
}

async function lockedRequest(challenge, operation) {
  const [meta] = await query('SELECT TaiKhoanID id FROM DatLaiMatKhau WHERE TokenHash=?', [tokenHash(challenge)]);
  if (!meta) return {error:invalid};
  return tx(async c => {
    const [user] = await query("SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? AND TrangThai='HOAT_DONG' FOR UPDATE", [meta.id], c);
    if (!user) return {error:invalid};
    const [row] = await query('SELECT * FROM DatLaiMatKhau WHERE TokenHash=? AND DaSuDungLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP() FOR UPDATE', [tokenHash(challenge)], c);
    if (!row) return {error:invalid};
    return operation(c,row);
  });
}

export async function verifyResetCode({challenge,code}) {
  if (!/^[a-f0-9]{64}$/.test(String(challenge||'')) || !/^\d{6}$/.test(String(code||''))) fail('Nhập đủ mã xác minh 6 chữ số.');
  const result = await lockedRequest(challenge, async(c,row) => {
    if (!row.MaXacMinhHash || row.SoLanThu>=5) return {error:invalid};
    if (!timingSafeEqual(otpHash(challenge,String(code)),row.MaXacMinhHash)) {
      const attempts=row.SoLanThu+1;
      await query('UPDATE DatLaiMatKhau SET SoLanThu=?,DaSuDungLuc=IF(? >= 5,UTC_TIMESTAMP(),NULL) WHERE YeuCauID=?',[attempts,attempts,row.YeuCauID],c);
      // Return the error instead of throwing inside tx, so failed attempts persist.
      return {error:attempts>=5?'Bạn đã nhập sai 5 lần. Vui lòng yêu cầu mã mới.':'Mã xác minh chưa đúng.'};
    }
    await query('UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE YeuCauID=?',[row.YeuCauID],c);
    const resetToken=randomBytes(32).toString('hex');
    await query('INSERT INTO DatLaiMatKhau(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 10 MINUTE))',[row.TaiKhoanID,tokenHash(resetToken)],c);
    return {resetToken,expiresIn:600};
  });
  if (result.error) fail(result.error);
  return result;
}

export async function resetPassword({token,password}) {
  if (typeof password!=='string'||password.length<8||password.length>128) fail('Mật khẩu cần từ 8 đến 128 ký tự.');
  if (!/^[a-f0-9]{64}$/.test(String(token||''))) fail('Phiên đặt lại mật khẩu không hợp lệ.');
  const result=await lockedRequest(token,async(c,row)=>{
    // A challenge is not a reset grant. Only verified grants or legacy links pass.
    if (row.MaXacMinhHash!==null) return {error:'Bạn cần xác minh OTP trước khi đặt mật khẩu mới.'};
    await query('UPDATE TaiKhoan SET MatKhauHash=?,NgayDoiMatKhau=UTC_TIMESTAMP(),SoLanDangNhapSai=0,KhoaDenLuc=NULL WHERE TaiKhoanID=?',[hashPassword(password),row.TaiKhoanID],c);
    await query('UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL',[row.TaiKhoanID],c);
    await query('UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=?',[row.TaiKhoanID],c);
    return {ok:true};
  });
  if(result.error)fail(result.error);
  return result;
}
