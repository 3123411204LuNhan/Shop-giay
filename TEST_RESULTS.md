# Kết quả kiểm tra bản bàn giao

Ngày kiểm tra: 11/11/2026. Môi trường: Node.js 24.20.0, MySQL 8.0.46, database riêng `sole_studio_nhom19`, web cổng 5173, MySQL cổng 3306.

## Đã xác minh

- `npm run check`: cú pháp backend hợp lệ.
- `npm run build`: build React/Vite thành công.
- `npm test`: 12 test được Node báo pass, 0 fail, gồm một bài kiểm thử tổng và 11 tình huống con.
- API health trả HTTP 200, MySQL connected, database có đủ 37 bảng.
- Đăng ký chỉ tạo vai trò CUSTOMER dù request cố truyền SYSTEM_ADMIN.
- Khách không truy cập được API quản lý; chủ shop không truy cập API kỹ thuật; admin kỹ thuật không được xử lý nghiệp vụ shop.
- Tạo đơn có ưu đãi và gửi lại cùng request không tạo đơn/giữ kho lặp.
- Không xem đơn của người khác và không nhảy trạng thái đơn trái quy trình.
- Xác nhận thu một lần, xuất kho, giao hàng và hoàn thành đơn.
- Chỉ đánh giá dòng mua của đơn đã hoàn thành; không đánh giá trùng cùng dòng.
- Duyệt trả, nhận hàng bán lại đúng một lần, xác nhận hoàn có bằng chứng, hoàn tất yêu cầu.
- Hủy đơn giải phóng lượng giữ đúng, không tăng tồn vật lý; retry hủy không giải phóng hai lần.
- Báo cáo khớp công thức thu đã xác nhận trừ hoàn đã xác nhận; có nhật ký quản trị.
- Tổng biến động kho khớp số dư `TonKho`, không có dòng chênh lệch sau các bài kiểm thử.

## Kiểm tra trực tiếp giao diện

- Cửa hàng tải đúng sản phẩm và ảnh cục bộ.
- Kiểm tra bố cục máy tính 1440px và điện thoại 390px; không phát hiện tràn ngang trang cửa hàng tại kích thước điện thoại đã kiểm tra.
- Đăng nhập chủ shop, xem dashboard thật và bảng/biểu mẫu sản phẩm.
- Đăng nhập admin kỹ thuật, kiểm tra dashboard hệ thống và bảng tài khoản.
- Chọn size khi chưa đăng nhập, thêm vào giỏ, đăng nhập khách và xác nhận giỏ được gộp.
- Đặt một đơn COD từ giao diện có mã WELCOME10; hiển thị thành công với số tiền đúng 2.990.000đ cho đôi 3.290.000đ (trần giảm 300.000đ).
- Không ghi nhận lỗi console trong lần kiểm tra cuối của trang cửa hàng.

Ảnh chụp nằm trong `screenshots/`. Dữ liệu QA và đơn thử được giữ trong database đồ án để đối chiếu. Các kiểm tra không thực hiện thanh toán ngân hàng thật, gửi email ra ngoài hoặc triển khai public.

## Giới hạn đã ghi trong README

Chưa tích hợp cổng thanh toán trực tuyến/email production; ảnh nhận URL thay vì upload. Đổi size đang thực hiện qua trả/hoàn tiền và đặt lại đơn mới. Giao diện khuyến mãi tạo phạm vi toàn bộ và nhận một mã trên mỗi đơn. Các phần này không được xem là tích hợp dịch vụ bên ngoài đã hoàn thành.

## Cập nhật 16/11/2026 — phiên đăng nhập theo khu vực

- Tách cookie khách hàng, chủ shop và admin; kiểm tra bằng cùng cookie jar và cùng trình duyệt thật.
- Đã đăng nhập admin, tiếp tục đăng nhập chủ shop và khách; đăng xuất khách rồi tải lại admin/manager: cả hai phiên quản lý vẫn còn hoạt động.
- Bỏ nút Xem cửa hàng ở cả hai sidebar quản lý.
- HTTP local không còn bị áp cờ Secure chỉ vì NODE_ENV=production; HTTPS vẫn có cờ Secure.
- Xóa cookie sai/hết hiệu lực và cookie chung cũ; header khu vực không thay thế kiểm tra vai trò.
- `npm run check`, `npm run build` thành công; `npm test`: 17 pass, 0 fail.
- Không đổi schema hoặc mật khẩu MySQL. Sau cập nhật cần đăng nhập lại từng khu vực một lần.

## Cập nhật OTP Gmail — 16/11/2026

- `npm run check` và `npm run build` thành công; `npm test`: 28 pass, 0 fail.
- Kiểm thử SMTP chấp nhận/từ chối bằng transport giả lập; fallback local không ghi mật khẩu SMTP.
- Kiểm thử OTP đúng, sai, khóa sau 5 lần, dùng lại OTP/token, ngăn bỏ qua OTP, thu hồi phiên và email không có tài khoản.
- Kiểm thử trực tiếp giao diện: gửi yêu cầu cho tài khoản QA miền .test, xuất hiện ô nhập OTP, nút gửi lại đếm ngược, mã đúng mở bước đặt mật khẩu.
- `npm run mail:verify` kết nối và xác thực Gmail SMTP thành công bằng cấu hình riêng trong .env. Lệnh này không gửi email; chưa xác nhận nhận thư thực tế trong Inbox.
- Migration đã thêm cột MaXacMinhHash mà không xóa dữ liệu, giữ 37 bảng.
- Kiểm thử phiên khách dùng tài khoản QA riêng, không phụ thuộc mật khẩu mẫu mà người dùng có thể đã đổi.

## Cập nhật giỏ hàng và phân quyền — 16/11/2026

- `npm run check`, `npm run build` thành công; `npm test`: 31 pass, 0 fail.
- Kiểm thử API với cùng cookie jar: A giữ 2 đôi sau đăng xuất; B có giỏ riêng; đăng nhập lại mỗi người vẫn thấy đúng giỏ của mình.
- Yêu cầu từ tab cũ mang mã tài khoản A trong phiên B bị chặn HTTP 409 SESSION_CHANGED; giỏ B không bị sửa.
- Giỏ vãng lai dùng sessionStorage theo tab, xóa dữ liệu sole-cart cũ và không tự gộp khi đăng nhập.
- Kiểm thử giao diện: giỏ vãng lai 2 đôi không cộng vào giỏ A đang có 2 đôi; A đăng xuất, B đăng nhập có giỏ trống. Ảnh: screenshots/gio-hang-b-doc-lap.png.
- Chủ shop và kỹ thuật bị từ chối đăng nhập ở trang khách hàng; API mua sắm trả 403 cho phiên quản lý. Chức năng quản lý và xem chi tiết đơn của chủ shop vẫn hoạt động.
- Không thay đổi schema, mật khẩu hoặc cấu hình SMTP.

## Cập nhật luồng nghiệp vụ phát triển — 18/11/2026

- `npm run check` và `npm run build` thành công. Build có cảnh báo kích thước bundle trên 500 kB, không ngăn chạy; chưa tối ưu tách bundle trong lần cập nhật nghiệp vụ này.
- `npm test`: 42 pass, 0 fail; chạy tuần tự để các tình huống nghiệp vụ có thể đối chiếu số dư kho ổn định.
- Migration chạy lặp thành công, giữ 37 bảng và dữ liệu hiện có; thêm cột vận chuyển, trạng thái phiếu, nhà cung cấp, xác nhận cùng ràng buộc dữ liệu.
- Phiếu nháp không tăng tồn, sửa/hủy nháp được, xác nhận đồng thời chỉ ghi kho một lần; điều chỉnh quá tồn hoàn tác.
- Tổng tiền được xem trước; thay đổi giá giữa xem trước và đặt bị từ chối. Chuyển khoản chưa thu không được đẩy đơn đi xử lý.
- Đơn chờ thanh toán hết hạn giải phóng giữ hàng đúng một lần; tác vụ hệ thống không gán người thực hiện là khách. Tiền chuyển đến sau hủy được đối soát, lập khoản hoàn một lần và không giữ hàng lại.
- COD không xác nhận thu trước khi giao. Giao thất bại chưa cộng kho; chỉ nhập lại số bán được khi thực nhận đầy đủ kiện hàng, gửi lại không cộng trùng.
- Nhận hàng trả vẫn được lưu khi khoản thu chưa đối soát. Không hoàn tất yêu cầu nếu chưa hoàn đủ. Hoàn tiền thất bại có luồng thử lại, không tạo lại phiếu nhập.
- Giao diện kiểm tra: lập/lưu/mở phiếu nháp; chọn biến thể từ trang kho sang phiếu; xem trước đơn Nike Air Max Pulse 3.290.000đ, WELCOME10 giảm 300.000đ, tổng 2.990.000đ. Không bấm đặt đơn trong kiểm thử UI này.
- Ảnh: screenshots/phieu-kho-phat-trien.png và screenshots/kiem-tra-tong-tien.png. Phiếu UI-DEV-1811-01 là nháp QA chưa ghi sổ. Các giao dịch QA từ kiểm thử API giữ lại để đối chiếu; không phải thanh toán thật.
- Hướng dẫn: HUONG_DAN_LUONG_PHAT_TRIEN.md. Không gọi ngân hàng, đơn vị vận chuyển hoặc gửi email thật trong lần kiểm thử này.
- Xác minh thêm trên Vite development cổng 5173: 42/42 kiểm thử đạt, health database connected, trang chủ HTTP 200. Giới hạn Node watch vào server và .env để tránh vòng khởi động lại do file cấu hình tạm của Vite.

## Tách ba trang mua sắm — 19/11/2026

- `npm run build`, `npm run check` thành công; `npm test`: 42 pass, 0 fail.
- Kiểm thử trình duyệt: trang Khám phá đổi danh sách gợi ý; chọn Giày chạy bộ mở /products?category=2 và thấy 2 mẫu. Lọc Nike rồi tải lại vẫn giữ category=2&brand=1, chỉ có Nike Pegasus 41.
- Running Club hiển thị WELCOME10 giảm 10%, tối đa 300.000đ, đơn từ 1.500.000đ; SOLE200 giảm 200.000đ, đơn từ 2.500.000đ, kèm hạn/điều kiện lấy từ database.
- Sao chép WELCOME10 hiển thị Đã sao chép. Mua với mã này mở sản phẩm; vào checkout thấy mã WELCOME10 được điền sẵn. Không đặt đơn hoặc thu tiền trong kiểm thử giao diện.
- Kiểm tra ở chiều rộng 390px: ba mục điều hướng hiển thị; trang sản phẩm và Running Club không tràn ngang. Ảnh: screenshots/running-club.png, screenshots/running-club-mobile.png.
- Không đổi schema, mật khẩu hoặc cấu hình Gmail/MySQL.
