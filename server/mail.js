import nodemailer from 'nodemailer';
import { mkdir, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';

export const localMailbox = fileURLToPath(new URL('../.local/mail/', import.meta.url));

export function smtpConfig(env = process.env) {
  const port = Number(env.SMTP_PORT || 465);
  const user = (env.SMTP_USER || '').trim();
  const pass = env.SMTP_APP_PASSWORD ? env.SMTP_APP_PASSWORD.replace(/\s/g, '') : (env.SMTP_PASSWORD || '');
  if (!env.SMTP_HOST || !user || !pass) return null;
  return {
    host: env.SMTP_HOST.trim(), port,
    secure: env.SMTP_SECURE ? env.SMTP_SECURE === 'true' : port === 465,
    requireTLS: port !== 465,
    auth: { user, pass },
    connectionTimeout: 8000, greetingTimeout: 8000, socketTimeout: 12000,
    logger: false, debug: false,
  };
}

export function reservedRecipient(email) {
  const domain = email.split('@')[1]?.toLowerCase() || '';
  return /\.(test|invalid|localhost|example|local)$/.test(domain) || ['localhost', 'example.com', 'example.net', 'example.org'].includes(domain);
}

export async function deliverResetCode({email, code, challenge, accountExists}, options = {}) {
  const env = options.env || process.env;
  const config = smtpConfig(env);
  let reason = !accountExists ? 'EMAIL_NOT_REGISTERED' : reservedRecipient(email) ? 'RESERVED_DOMAIN' : !config ? 'SMTP_NOT_CONFIGURED' : '';
  if (!reason) {
    try {
      const transport = options.transport || nodemailer.createTransport(config);
      const info = await transport.sendMail({
        from: { name: env.SMTP_FROM || 'SOLE STUDIO', address: config.auth.user },
        to: email,
        subject: 'SOLE STUDIO — Mã xác minh đặt lại mật khẩu',
        text: `Mã xác minh của bạn: ${code}\n\nMã có hiệu lực 10 phút và chỉ dùng một lần. Không chia sẻ mã này với người khác.\nNếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.`,
        html: `<div style="font-family:Arial,sans-serif;max-width:500px;margin:auto;color:#20291e"><h2>SOLE STUDIO</h2><p>Mã xác minh đặt lại mật khẩu của bạn:</p><p style="font-size:34px;letter-spacing:8px;font-weight:bold;background:#edf4d7;padding:22px;text-align:center">${code}</p><p>Mã có hiệu lực <b>10 phút</b> và chỉ dùng một lần.</p><p>Không chia sẻ mã với người khác. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p></div>`,
        disableFileAccess: true, disableUrlAccess: true,
      });
      const accepted = (info.accepted || []).some(recipient => (typeof recipient === 'string' ? recipient : recipient.address)?.toLowerCase() === email.toLowerCase());
      if (accepted) return { channel: 'smtp' };
      reason = 'SMTP_RECIPIENT_REJECTED';
    } catch (error) {
      // Store only a short error code, never credentials or the SMTP transcript.
      reason = 'SMTP_' + String(error.code || 'SEND_FAILED').replace(/[^A-Z0-9_]/gi, '').slice(0, 30);
    }
  }
  if (env.MAIL_FALLBACK_LOCAL === 'false') throw Object.assign(new Error('Chưa gửi được mã xác minh. Vui lòng thử lại sau.'), {status:503});
  const directory = options.directory || localMailbox;
  await mkdir(directory, {recursive:true});
  const file = `reset-${Date.now()}-${challenge.slice(0, 8)}.txt`;
  const origin = env.APP_ORIGIN || 'http://localhost:5173';
  const message = [
    'SOLE STUDIO — HỘP THƯ LOCAL', `Đến: ${email}`, `Mã xác minh: ${code}`,
    'Hiệu lực: 10 phút. Chỉ dùng một lần.', `Lý do lưu local: ${reason}`,
    `Mở lại màn hình nhập mã: ${origin}/?verify=${challenge}`,
    accountExists ? 'Nhập mã ở bước xác minh trên website, sau đó đặt mật khẩu mới.' : 'Email chưa đăng ký tài khoản. Mã này chỉ minh họa hộp thư local, KHÔNG thể dùng đặt lại mật khẩu.',
    '',
  ].join('\n');
  await writeFile(join(directory, file), message, {encoding:'utf8',mode:0o600});
  return {channel:'local'};
}
