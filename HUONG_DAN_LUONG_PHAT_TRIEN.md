# Hướng dẫn sử dụng các luồng nghiệp vụ phiên bản phát triển

Phiên bản cập nhật ngày 18/11/2026 dành cho cửa hàng nhỏ. Quản lý kiêm kho và xử lý kinh doanh; admin phụ trách tài khoản, phân quyền và nhật ký. Thanh toán và vận chuyển được quản lý đối soát thủ công, chưa kết nối ngân hàng hoặc đơn vị giao hàng.

## Chạy môi trường phát triển

Trong thư mục `shoe-store`, giữ cấu hình kết nối MySQL và Gmail hiện có trong `.env`:

```powershell
npm.cmd run db:migrate
npm.cmd run dev
```

Website mặc định: http://localhost:5173. MySQL vẫn dùng cổng 3306. Migration được chạy lại an toàn: không xóa bảng, không seed lại dữ liệu. Database vẫn có 37 bảng.

Những biến sau là tùy chọn trong `.env`; không cần thay mật khẩu hoặc cấu hình SMTP:

```dotenv
APP_ENV=development
PAYMENT_HOLD_MINUTES=30
RETURN_WINDOW_DAYS=7
BANK_NAME=
BANK_ACCOUNT=
BANK_HOLDER=
```

`APP_ENV` phân biệt development/staging/production, độc lập với `NODE_ENV` và cách phục vụ bản build. `BANK_*` là thông tin tài khoản nhận tiền được hiển thị cho khách; chỉ điền thông tin ngân hàng đã được shop xác minh. Không điền mật khẩu ngân hàng. Nếu để trống, khách được hướng dẫn liên hệ shop. Đổi cấu hình cần khởi động lại máy chủ.

## Khách đặt hàng

1. Thêm giày theo size/màu vào giỏ. Bước này không giữ hàng.
2. Chọn Tiến hành đặt hàng, điền địa chỉ, phương thức COD/chuyển khoản và mã ưu đãi.
3. Bấm Kiểm tra tổng tiền. Máy chủ trả giá, giảm giá, phí vận chuyển và tổng cuối cùng.
4. Xem lại rồi bấm Xác nhận đặt hàng. Máy chủ kiểm tra lại; dữ liệu thay đổi thì yêu cầu xem lại thay vì âm thầm chốt giá mới.
5. Tạo đơn, giữ hàng và xóa các dòng giỏ đã đặt trong cùng giao dịch. Gửi lại cùng mã yêu cầu trả đơn cũ.

Giỏ đăng nhập vẫn tách theo tài khoản. Không tự gộp giỏ vãng lai sang tài khoản khác trên cùng máy.

## Quản lý đơn và đối soát thu tiền

Vào Đơn hàng → Chi tiết. Đơn, thanh toán và vận chuyển hiển thị riêng.

- COD: xác nhận đơn → chuẩn bị → đang giao → đã giao → đối soát khoản thu bằng chứng thực tế → hoàn thành. Không được xác nhận thu COD trước khi giao thành công.
- Chuyển khoản: khách nhận hướng dẫn trong chi tiết đơn → quản lý đối chiếu tiền thực nhận và bấm Đối soát thu → mới xác nhận/xử lý đơn. Ảnh chuyển khoản không tự xác nhận khoản thu.
- Khoản thu đã xác nhận không được ghi nhận lần hai. Không hoàn thành khi chưa thu đủ tổng đơn.

Đơn chuyển khoản mới được giữ hàng mặc định 30 phút. Bộ xử lý chạy mỗi 15 giây khi Node đang hoạt động và chạy lại khi máy chủ khởi động. Hết hạn, chưa có khoản thu xác nhận, đơn chưa xuất sẽ tự hủy và giải phóng phần giữ hàng. Mốc hạn được lưu trong database; khởi động lại không gia hạn. Các đơn cũ có hạn thanh toán NULL được giữ nguyên, không bị tự hủy hồi tố.

Nếu tiền chuyển đến sau khi đơn hủy: quản lý mở đơn → Tiền chuyển đến sau hủy → nhập số tiền thực nhận và bằng chứng. Hệ thống ghi khoản thu, tạo yêu cầu hoàn và giữ nguyên đơn hủy; không tự giữ hàng lại. Luồng này hỗ trợ một lần đối soát tiền đến muộn với số tiền dương không vượt tổng đơn. Các khoản chuyển sai/nhận vượt cần đối soát ngoài hệ thống ở phiên bản hiện tại.

## Kho và phiếu nhập

Vào Biến thể & kho để xem tồn thực tế, đang giữ, khả dụng và ngưỡng cảnh báo. Bấm Chỉ xem sắp hết để lọc khả dụng ≤ ngưỡng; bấm Ngưỡng để đặt mức 5, 10 hoặc mức phù hợp mỗi biến thể.

Quản lý kiểm tra cảnh báo, đặt hàng nhà cung cấp bên ngoài website, nhận và kiểm hàng rồi lập phiếu:

1. Mở Phiếu kho → Lập phiếu kho, hoặc chọn Lập phiếu kho trên một biến thể.
2. Chọn Nhập hàng thực nhận, nhập nhà cung cấp, mã chứng từ, ghi chú, các biến thể, số lượng thực nhận và giá nhập.
3. Lưu phiếu nháp: chưa tăng tồn, có thể sửa hoặc hủy.
4. Kiểm tra chi tiết rồi Xác nhận ghi sổ kho: cập nhật tồn và sổ kho trong cùng giao dịch, chỉ một lần.
5. Phiếu đã ghi sổ được giữ nguyên; sai số lượng thì lập phiếu Điều chỉnh chênh lệch (+/−), ghi lý do và mã phiếu gốc.

Mã chứng từ nhập không được dùng lại cho một phiếu nhập khác, kể cả khi tải lại trang. Nháp đã hủy vẫn giữ chứng từ để đối soát. Phiếu điều chỉnh ghi phần chênh lệch, không ghi số tồn cuối. Ví dụ tồn hệ thống 10, kiểm thực tế 8 thì nhập −2. Điều chỉnh không được làm tồn thực tế thấp hơn phần đã giữ.

Lịch sử kho cho biết người xử lý, tăng/giảm tồn và phần giữ, số dư sau mỗi thao tác. Với thao tác hết hạn tự động, người thực hiện để trống và lý do ghi rõ Hệ thống.

## Giao hàng và hàng hoàn về

Khi chuyển Đang giao, hệ thống giảm tồn thực tế và phần đã giữ tương ứng. Quản lý ghi đơn vị vận chuyển và mã vận đơn trong chi tiết đơn.

Nếu giao thất bại:

1. Bấm Giao thất bại và ghi lý do. Tồn kho chưa tăng.
2. Chọn Giao lại nếu tiếp tục giao, hoặc Chuyển chờ hoàn về nếu thu hồi kiện hàng.
3. Khi thực nhận đủ toàn bộ kiện, bấm Nhận và kiểm hàng hoàn. Nhập số đôi bán lại theo từng dòng; số không bán lại vẫn được ghi trong kết quả kiểm hàng.
4. Hệ thống nhập lại phần đủ chất lượng, hủy đơn và lập khoản hoàn nếu đã thu tiền. Gửi lại xác nhận không cộng kho lần hai.

Phiên bản này xác nhận nhận hoàn vận chuyển một lần khi đã kiểm đủ tất cả dòng/số lượng; chưa hỗ trợ nhận từng phần kiện thất lạc. Hàng lỗi không được cộng vào tồn bán được; chưa có phân hệ kho hàng lỗi riêng.

## Khách trả hàng và hoàn tiền

Khách mở Đơn hàng của tôi → Chi tiết → Trả hàng / đổi size. Mặc định gửi trong 7 ngày từ ngày giao. Hệ thống kiểm tra chủ đơn và số lượng còn được yêu cầu, bao gồm các yêu cầu trước đang chờ/xử lý.

Quản lý vào Đổi / trả hàng:

1. Duyệt hoặc từ chối kèm lý do.
2. Sau khi nhận đủ phần hàng được duyệt, xác nhận nhận hàng và tình trạng bán lại. Phiên bản này xử lý nhận một lần cho toàn bộ số lượng được duyệt của yêu cầu.
3. Hàng đủ chất lượng được nhập lại kho. Nếu khoản thu chưa đối soát, hàng nhận vẫn được lưu; yêu cầu tiếp tục chờ xử lý tiền.
4. Đối soát khoản thu nếu cần, rồi chọn Lập bổ sung khoản hoàn. Thao tác lặp không tạo dư nghĩa vụ hoàn.
5. Vào Hoàn tiền, thực hiện chuyển tiền bên ngoài hệ thống rồi nhập bằng chứng Xác nhận đã hoàn. Nếu chuyển tiền thất bại, ghi Hoàn thất bại; sau khi khắc phục bấm Thử lại hoàn tiền rồi xác nhận.
6. Chỉ Hoàn tất yêu cầu sau khi đã nhận hàng và xác nhận hoàn đủ tiền được duyệt.

Khách có nút Theo dõi xử lý để xem số lượng duyệt/nhận, phản hồi, lịch sử và trạng thái hoàn tiền. Đổi size dùng trả hàng, hoàn tiền và đặt đơn mới; chưa tự giao hàng thay thế.

## Dữ liệu và kiểm thử

Migration thêm thông tin vận chuyển vào DonHang; trạng thái nháp, nhà cung cấp và xác nhận vào PhieuKho. Phiếu cũ mặc định Đã xác nhận để không ghi kho lần hai. Không tạo thêm bảng, không thay đổi số dư tồn cũ. Lịch sử kho/đơn cho phép người thực hiện NULL cho tác vụ tự động. Ràng buộc trạng thái và khóa ngoại vẫn kiểm tra dữ liệu.

```powershell
npm.cmd run check
npm.cmd run build
npm.cmd test
```

Máy chủ phải chạy trước khi kiểm thử API. Có thể kiểm thử riêng trên cổng khác bằng cách đặt PORT cho máy chủ và tiến trình test giống nhau. Chỉ chạy test trên database phát triển có tên bắt đầu bằng sole_studio_nhom19. Bộ test tạo giao dịch QA, không gửi tiền thực tế hoặc gọi dịch vụ vận chuyển; chứng từ và sổ kho QA được giữ để đối chiếu.

## Trước khi chuyển sang cửa hàng thật

Bản này là môi trường phát triển. Đơn đặt nhà cung cấp, đơn bán tại quầy, tích hợp cổng thanh toán/vận chuyển, quản lý hàng lỗi và nhận hoàn từng phần chưa được triển khai.

Chuẩn bị database vận hành riêng, nhập dữ liệu sản phẩm thật và kiểm kê tồn đầu kỳ; không dùng giao dịch QA làm doanh số thật. Sao lưu và thử phục hồi; cấu hình HTTPS, tài khoản database quyền phù hợp và bí mật riêng. Không chạy seed/test trên database vận hành. APP_ENV=production ẩn nút tài khoản trải nghiệm; phải thay hoặc loại tài khoản mẫu trước mở bán, chỉ ẩn nút không làm tài khoản mẫu mất hiệu lực.

Trước mỗi lần nâng cấp: sao lưu, thử migration ở staging rồi mới áp dụng database thật. Không chạy lại db:setup/seed để nâng cấp. Khi thêm cổng thanh toán, đặt xác minh giao dịch phía máy chủ trước cùng nghiệp vụ ghi nhận khoản thu; khi thêm vận chuyển, vẫn giữ xác nhận kiểm hàng thực nhận cho hàng hoàn.
