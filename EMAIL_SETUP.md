# Gửi OTP qua SMTP thật

Backend gửi email bằng MailKit. Khi SMTP được cấu hình đúng, mã OTP được gửi tới hộp thư của tài khoản; nếu máy chưa có cấu hình hoặc SMTP từ chối gửi, mặc định ứng dụng ghi mã vào `.local/mail` để phát triển local.

## Sử dụng trên website

1. Chọn **Đăng nhập → Quên mật khẩu**, nhập email của tài khoản đã đăng ký.
2. Kiểm tra thư đến và Spam, lấy mã **6 chữ số**.
3. Nhập mã trên website, bấm **Xác minh mã**.
4. Nhập và xác nhận mật khẩu mới. Các phiên cũ của tài khoản sẽ bị thu hồi.

OTP có hạn **10 phút**, chỉ dùng một lần, tối đa **5 lần nhập sai**. Nút gửi lại mở sau **60 giây**. Mã cũ bị vô hiệu hóa khi yêu cầu mã mới.

## Cấu hình Gmail

Trong `.env` ở thư mục gốc của dự án, điền cấu hình SMTP như sau.

```dotenv
SMTP_HOST=smtp.gmail.com
SMTP_PORT=465
SMTP_SECURE=true
SMTP_USER=your-account@gmail.com
SMTP_APP_PASSWORD=your_google_app_password
SMTP_FROM=SOLE STUDIO
MAIL_FALLBACK_LOCAL=true
```

`SMTP_USER` là tài khoản **gửi thư**, không bắt buộc là tài khoản người nhận. `SMTP_APP_PASSWORD` là App Password, không phải mật khẩu đăng nhập Gmail. Có thể dán App Password có khoảng trắng; ứng dụng bỏ khoảng trắng khi sử dụng.

Để tạo App Password, bật Xác minh 2 bước rồi mở [Mật khẩu ứng dụng của Google](https://myaccount.google.com/apppasswords). Xem [hướng dẫn chính thức](https://support.google.com/accounts/answer/185833?hl=vi) nếu tùy chọn không khả dụng.

Sau khi sửa `.env`, dừng và chạy lại server. `.env` và `.local/` không được đưa lên GitHub.

Để bắt buộc mọi yêu cầu OTP phải gửi qua SMTP và báo lỗi thay vì ghi local, đặt `MAIL_FALLBACK_LOCAL=false`. Chỉ nên bật khi SMTP đã hoạt động; email chưa đăng ký và địa chỉ miền thử nghiệm không bao giờ được gửi qua SMTP.

Kiểm tra kết nối và đăng nhập SMTP:

```powershell
npm.cmd run mail:verify
```

Lệnh này kết nối SMTP và xác thực tài khoản gửi, nhưng **không gửi thư**. Khi báo thành công, dùng Quên mật khẩu trên website với email thật đã đăng ký trong database để kiểm tra gửi và nhận thư.

## Nâng cấp database cũ

```powershell
npm.cmd ci
npm.cmd run db:migrate
npm.cmd run dev
```

Lệnh C# nâng cấp schema cũ và bổ sung cột `MaXacMinhHash binary(32) NULL` vào `DatLaiMatKhau` nếu thiếu. Lệnh giữ các bản ghi hiện có và có thể chạy lại. Database tạo mới từ `database/schema.sql` đã có cột này.

## Khi nào lưu local

- Chưa có cấu hình SMTP.
- Gmail/SMTP trả lỗi xác thực, lỗi kết nối hoặc từ chối người nhận.
- Email của dữ liệu thử dùng miền dự phòng như `.test`, `.invalid` hoặc `example.com`.
- Email chưa có tài khoản đang hoạt động trong hệ thống: chỉ tạo thư minh họa local, **không cấp quyền đặt lại mật khẩu**.

Mở `.local/mail/reset-....txt` mới nhất trong thư mục ứng dụng đang chạy. File có dòng `Mã xác minh: 123456` và lý do lưu local. Nếu đóng mất hộp nhập OTP, có thể mở đường dẫn `/?verify=...` được ghi trong file để nhập lại mã còn hiệu lực. File được lưu cả khi chạy bản build production nếu `MAIL_FALLBACK_LOCAL=true`.

Hệ thống không thể xác nhận chắc chắn mọi địa chỉ email có thật. SMTP có thể chấp nhận thư rồi thông báo không phát được sau đó; trường hợp đó không thể tự chuyển local ngay trong yêu cầu HTTP ban đầu. Trạng thái chấp nhận SMTP cũng không chứng minh thư đã đến Inbox. Kiểm tra Spam và email báo lỗi của tài khoản gửi khi cần.

## Lưu ý về tài khoản và bảo mật

- Email chưa đăng ký không được tự tạo tài khoản hoặc thay mật khẩu của tài khoản khác.
- API không trả mã OTP hoặc mật khẩu SMTP cho trình duyệt. OTP và token đặt lại được lưu dạng hash trong database.
- Cookie đăng nhập của khách/manager/admin vẫn độc lập; đổi mật khẩu thu hồi các phiên của đúng tài khoản được đổi.
- Token từ bản cũ có thể tiếp tục hoạt động đến khi hết hạn nếu chưa có yêu cầu mới. Các yêu cầu OTP mới cần hoàn thành bước xác minh trước khi được cấp token đặt lại mật khẩu.
- Kiểm thử tự động không dùng hộp thư thật. Kiểm tra gửi/nhận thực tế phải dùng hộp thư bạn quản lý. Không đặt dữ liệu `.local` trong thư mục public.

## Dùng SMTP khác

Có thể đổi SMTP_HOST, SMTP_PORT, SMTP_SECURE, SMTP_USER và SMTP_PASSWORD. Khi dùng SMTP_PASSWORD hãy để trống SMTP_APP_PASSWORD. Cổng 587 dùng `SMTP_SECURE=false` và yêu cầu nâng cấp STARTTLS; cổng 465 dùng TLS ngay từ đầu.

Thư viện SMTP của backend là MailKit.
