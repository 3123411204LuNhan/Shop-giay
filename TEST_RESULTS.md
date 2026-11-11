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
