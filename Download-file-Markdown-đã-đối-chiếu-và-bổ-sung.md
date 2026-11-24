# Danh sách chức năng và toàn bộ luồng hoạt động của hệ thống quản lý bán giày

> Nguồn tổng hợp: báo cáo `S2_Nhom19_Lan4-1.docx`. Nội dung dưới đây ưu tiên yêu cầu chức năng, Use Case, đặc tả Use Case và các quy tắc nghiệp vụ trong báo cáo. Các chức năng được nhóm theo tác nhân để thuận tiện dùng cho BFD/DFD/Use Case/Sequence.

## 1. Tác nhân của hệ thống

- **Khách hàng (Customer):** tra cứu sản phẩm, quản lý giỏ hàng, đặt hàng, thanh toán và theo dõi giao dịch.
- **Quản lý cửa hàng (Shop Manager):** quản lý sản phẩm, biến thể, danh mục, thương hiệu, kho, đơn hàng, đổi/trả, khuyến mãi và báo cáo.
- **Quản trị hệ thống (System Admin):** quản lý tài khoản, trạng thái tài khoản và phân quyền.
- **Cổng/kênh thanh toán:** đối tượng ngoài hệ thống tham gia khi xử lý thanh toán trực tuyến/chuyển khoản.

---

# 2. Chức năng dành cho Khách hàng

## 2.1. Đăng ký tài khoản

### Mục tiêu
Tạo tài khoản khách hàng mới.

### Luồng chính
1. Khách hàng mở chức năng **Đăng ký**.
2. Hệ thống hiển thị biểu mẫu đăng ký.
3. Khách hàng nhập thông tin tài khoản và xác nhận mật khẩu.
4. Hệ thống kiểm tra dữ liệu bắt buộc và tính hợp lệ.
5. Hệ thống kiểm tra thông tin định danh có bị trùng hay không.
6. Nếu hợp lệ, hệ thống tạo tài khoản mới.
7. Hệ thống bảo vệ mật khẩu trước khi lưu.
8. Hệ thống thông báo đăng ký thành công.

### Luồng thay thế/ngoại lệ
- Thiếu hoặc sai dữ liệu: hệ thống yêu cầu sửa.
- Email/tên đăng nhập đã tồn tại: từ chối tạo tài khoản.
- Lưu tài khoản thất bại: không tạo tài khoản dở dang và thông báo lỗi.

### Kết quả
Tài khoản mới được tạo hoặc dữ liệu không thay đổi nếu đăng ký thất bại.

---

## 2.2. Đăng nhập

### Mục tiêu
Xác thực khách hàng và tạo phiên truy cập.

### Luồng chính
1. Khách hàng nhập tên đăng nhập và mật khẩu.
2. Hệ thống tìm tài khoản tương ứng.
3. Hệ thống kiểm tra trạng thái tài khoản.
4. Hệ thống xác thực mật khẩu.
5. Nếu hợp lệ, hệ thống tạo phiên đăng nhập.
6. Hệ thống chuyển khách hàng đến trang phù hợp.

### Ngoại lệ
- Sai thông tin đăng nhập: thông báo lỗi.
- Tài khoản bị khóa/vô hiệu hóa: từ chối đăng nhập.
- Không tạo được phiên: đăng nhập không hoàn tất.

---

## 2.3. Đăng xuất

### Luồng chính
1. Khách hàng chọn **Đăng xuất**.
2. Hệ thống yêu cầu xác nhận nếu cần.
3. Khách hàng xác nhận.
4. Hệ thống thu hồi/kết thúc phiên đăng nhập.
5. Hệ thống xóa thông tin xác thực ở giao diện.
6. Khách hàng trở về trạng thái chưa đăng nhập.

### Ngoại lệ
- Khách hàng hủy xác nhận: giữ nguyên phiên.

---

## 2.4. Khôi phục mật khẩu

### Luồng chính
1. Khách hàng chọn **Quên mật khẩu**.
2. Khách hàng nhập email/thông tin nhận diện.
3. Hệ thống tiếp nhận yêu cầu và tạo mã/token xác minh có thời hạn.
4. Hệ thống gửi mã hoặc liên kết đặt lại mật khẩu.
5. Khách hàng cung cấp mã xác minh và mật khẩu mới.
6. Hệ thống kiểm tra mã còn hiệu lực, chưa sử dụng và hợp lệ.
7. Hệ thống kiểm tra mật khẩu mới.
8. Hệ thống cập nhật mật khẩu mới và đánh dấu mã đã sử dụng.
9. Hệ thống thông báo kết quả.

### Ngoại lệ
- Mã sai/hết hạn/đã dùng: từ chối.
- Mật khẩu mới không hợp lệ: yêu cầu nhập lại.
- Không tiết lộ việc email có tồn tại hay không.

---

## 2.5. Quản lý thông tin tài khoản

### Luồng tổng quát
1. Khách hàng đăng nhập.
2. Mở trang thông tin cá nhân.
3. Hệ thống hiển thị hồ sơ hiện tại.
4. Khách hàng thay đổi thông tin được phép.
5. Hệ thống kiểm tra tính hợp lệ.
6. Hệ thống cập nhật hồ sơ.
7. Hệ thống thông báo kết quả.

### Chức năng liên quan trong phạm vi báo cáo
- Xem/cập nhật thông tin cá nhân.
- Quản lý địa chỉ nhận hàng.
- Thay đổi mật khẩu.
- Truy cập lịch sử mua hàng.

> Lưu ý: Chương 2 nêu chức năng này nhưng Chương 3 chưa có đặc tả Use Case độc lập tương ứng.

---

## 2.6. Tìm kiếm sản phẩm

### Luồng chính
1. Khách hàng nhập từ khóa.
2. Hệ thống tiếp nhận điều kiện tìm kiếm.
3. Hệ thống tra cứu các sản phẩm đang được công bố/kinh doanh.
4. Hệ thống trả về danh sách phù hợp.
5. Khách hàng có thể chọn sản phẩm để xem chi tiết.

### Ngoại lệ
- Không có kết quả: hiển thị danh sách rỗng/thông báo không tìm thấy.

---

## 2.7. Lọc sản phẩm

### Luồng chính
1. Khách hàng chọn tiêu chí lọc như danh mục, thương hiệu, giá, size hoặc màu.
2. Hệ thống tiếp nhận bộ lọc.
3. Hệ thống áp dụng điều kiện trên tập sản phẩm đang công bố.
4. Hệ thống cập nhật danh sách kết quả.
5. Khách hàng có thể điều chỉnh hoặc bỏ bộ lọc.

### Ngoại lệ
- Không có sản phẩm phù hợp: hiển thị danh sách rỗng.

---

## 2.8. Xem chi tiết sản phẩm

### Luồng chính
1. Khách hàng chọn một sản phẩm hoặc mở liên kết sản phẩm.
2. Hệ thống kiểm tra sản phẩm còn tồn tại và được công bố.
3. Hệ thống lấy thông tin sản phẩm.
4. Hệ thống lấy danh sách biến thể size/màu.
5. Hệ thống lấy tình trạng còn hàng của từng biến thể.
6. Hệ thống hiển thị thông tin chi tiết.
7. Khách hàng có thể chọn biến thể để chuẩn bị thêm vào giỏ.

### Ngoại lệ
- Sản phẩm không tồn tại/không còn công bố: thông báo không khả dụng.
- Biến thể hết hàng: vẫn hiển thị nhưng không cho chọn mua.

---

## 2.9. Quản lý giỏ hàng

### 2.9.1. Thêm sản phẩm vào giỏ
1. Khách hàng chọn sản phẩm.
2. Chọn size, màu và số lượng.
3. Chọn **Thêm vào giỏ**.
4. Hệ thống kiểm tra sản phẩm, biến thể và tồn khả dụng.
5. Nếu biến thể chưa có trong giỏ, tạo dòng mới.
6. Nếu đã có cùng biến thể, gộp số lượng.
7. Hệ thống cập nhật giỏ và số lượng hiển thị.

### 2.9.2. Cập nhật giỏ
1. Khách hàng mở giỏ hàng.
2. Thay đổi số lượng một dòng.
3. Hệ thống kiểm tra số lượng mới.
4. Hệ thống kiểm tra tồn khả dụng.
5. Hệ thống cập nhật số lượng.
6. Tính lại thành tiền và tổng tạm tính.
7. Hiển thị giỏ sau cập nhật.

### 2.9.3. Xóa sản phẩm khỏi giỏ
1. Khách hàng chọn xóa một dòng.
2. Xác nhận thao tác.
3. Hệ thống xóa dòng.
4. Tính lại tổng tiền.
5. Nếu hết sản phẩm, hiển thị giỏ trống và không cho đặt hàng.

### Ngoại lệ chung
- Biến thể hết hàng/ngừng bán/số lượng vượt tồn: yêu cầu điều chỉnh.
- Số lượng <= 0: không cập nhật.
- Hủy xác nhận xóa: giữ nguyên giỏ.

### Quy tắc quan trọng
Thêm vào giỏ **không làm giảm tồn kho ngay**. Tồn được kiểm tra/phân bổ tại quy trình đặt hàng.

---

## 2.10. Đặt hàng

### Luồng chính
1. Khách hàng đã đăng nhập mở giỏ hàng.
2. Chọn **Xác nhận giỏ hàng/Đặt hàng**.
3. Hệ thống kiểm tra từng sản phẩm, biến thể, số lượng khả dụng, giá và khuyến mãi.
4. Khách hàng kiểm tra sản phẩm, số lượng và thông tin nhận hàng.
5. Hệ thống tính tiền hàng, giảm giá, phí và tổng thanh toán.
6. Khách hàng sửa thông tin cần thiết.
7. Khách hàng xác nhận nội dung cuối cùng.
8. Khách hàng chọn **Tạo đơn hàng**.
9. Hệ thống kiểm tra lại dữ liệu lần cuối.
10. Hệ thống tạo đơn có mã duy nhất.
11. Hệ thống tạo các dòng chi tiết đơn từ giỏ hàng.
12. Hệ thống phân bổ/giữ tồn kho tương ứng.
13. Hệ thống lưu giá và thông tin sản phẩm tại thời điểm mua.
14. Hệ thống hiển thị mã đơn và tổng tiền.
15. Hệ thống chuyển khách hàng đến bước thanh toán.

### Ngoại lệ
- Giỏ rỗng: từ chối.
- Giá/khuyến mãi/tồn thay đổi: hiển thị dữ liệu mới và yêu cầu xác nhận lại.
- Thông tin nhận hàng thiếu/sai: yêu cầu sửa.
- Lưu đơn thất bại: hoàn tác dữ liệu dở dang và không để sai tồn kho.
- Yêu cầu được gửi lặp: trả đơn đã tạo thay vì tạo trùng.

---

## 2.11. Thanh toán đơn hàng

### Luồng chung
1. Hệ thống hiển thị đơn cần thanh toán.
2. Khách hàng chọn phương thức.
3. Hệ thống xử lý theo nhánh.

### Nhánh COD
1. Khách hàng chọn COD.
2. Hệ thống ghi nhận phương thức thanh toán khi nhận hàng.
3. Đơn chuyển sang trạng thái chờ xác nhận/xử lý.
4. Chưa ghi nhận là đã thu tiền.

### Nhánh thanh toán trực tuyến
1. Khách hàng chọn thanh toán trực tuyến.
2. Hệ thống tạo yêu cầu giao dịch.
3. Chuyển khách hàng/yêu cầu đến cổng thanh toán.
4. Cổng thanh toán trả kết quả.
5. Nếu thành công, hệ thống ghi giao dịch và đánh dấu đã thanh toán.
6. Hệ thống thông báo kết quả.

### Nhánh chuyển khoản
1. Hệ thống cung cấp hướng dẫn/chứng từ cần thiết.
2. Giao dịch được ghi nhận ở trạng thái chờ xác nhận.
3. Sau khi có xác nhận hợp lệ, trạng thái thanh toán được cập nhật.

### Ngoại lệ
- Khách hàng hủy: giữ trạng thái chưa thanh toán.
- Giao dịch thất bại/hết hạn: thông báo và cho thử lại.
- Đơn đã hủy/đã thanh toán: từ chối thực hiện lại.
- Một thanh toán thành công chỉ được ghi nhận một lần.

---

## 2.12. Xem lịch sử mua hàng

### Luồng chính
1. Khách hàng đăng nhập.
2. Chọn **Lịch sử mua hàng**.
3. Hệ thống lấy các đơn thuộc khách hàng.
4. Hiển thị danh sách đơn.
5. Khách hàng có thể chọn một đơn để xem chi tiết.

### Ngoại lệ
- Chưa có đơn: hiển thị danh sách trống.

---

## 2.13. Xem chi tiết đơn hàng

1. Khách hàng chọn một đơn từ lịch sử.
2. Hệ thống kiểm tra quyền sở hữu đơn.
3. Hệ thống lấy thông tin đơn và các dòng hàng.
4. Hiển thị sản phẩm, size, màu, số lượng, địa chỉ giao hàng, thanh toán và trạng thái.

### Ngoại lệ
- Không tìm thấy hoặc không thuộc khách hàng: không cho truy cập.

---

## 2.14. Theo dõi đơn hàng

1. Khách hàng đăng nhập.
2. Chọn đơn đang xử lý.
3. Hệ thống đọc trạng thái hiện tại và lịch sử trạng thái.
4. Hiển thị tiến trình xử lý/giao hàng.
5. Khi Shop Manager cập nhật trạng thái, thông tin theo dõi phản ánh trạng thái mới.

> Chương 2 khai báo đây là chức năng độc lập, nhưng Chương 3 đang chủ yếu thể hiện qua lịch sử/chi tiết đơn.

---

## 2.15. Hủy đơn hàng

1. Khách hàng chọn đơn cần hủy.
2. Hệ thống kiểm tra đơn thuộc khách hàng.
3. Hệ thống kiểm tra trạng thái có cho phép hủy.
4. Hiển thị thông tin và yêu cầu xác nhận.
5. Khách hàng xác nhận, có thể nhập lý do.
6. Hệ thống kiểm tra trạng thái lần cuối để tránh xung đột.
7. Hệ thống chuyển đơn sang **Đã hủy**.
8. Hệ thống giải phóng phần tồn kho đã phân bổ.
9. Nếu đơn đã thanh toán, hệ thống tạo/ghi nhận yêu cầu hoàn tiền ở trạng thái chờ xử lý.
10. Hệ thống ghi lịch sử trạng thái và thông báo kết quả.

### Ngoại lệ
- Đang giao/đã hoàn thành/trạng thái không cho hủy: từ chối.
- Đã hủy: không xử lý lặp.
- Không được hoàn tiền hai lần.

---

## 2.16. Gửi yêu cầu đổi/trả hàng

### Luồng nghiệp vụ suy ra trực tiếp từ yêu cầu và mô hình dữ liệu
1. Khách hàng đăng nhập và mở đơn đủ điều kiện đổi/trả.
2. Chọn dòng hàng cần đổi hoặc trả.
3. Chọn loại yêu cầu **Đổi** hoặc **Trả**.
4. Nhập số lượng, lý do và thông tin liên quan.
5. Có thể đính kèm hình ảnh bằng chứng.
6. Nếu đổi, chọn/đề xuất biến thể muốn đổi sang khi chính sách cho phép.
7. Hệ thống kiểm tra đơn thuộc khách hàng và số lượng yêu cầu không vượt số lượng đã mua.
8. Hệ thống tạo yêu cầu đổi/trả ở trạng thái chờ duyệt.
9. Hệ thống thông báo mã yêu cầu/trạng thái cho khách hàng.
10. Yêu cầu được chuyển sang quy trình xử lý của Shop Manager.

### Ngoại lệ
- Đơn không hợp lệ/không thuộc khách hàng: từ chối.
- Số lượng yêu cầu vượt số đã mua: từ chối.
- Yêu cầu gửi lặp: không tạo bản ghi trùng.

> Báo cáo có yêu cầu chức năng và lớp dữ liệu cho việc khách gửi yêu cầu, nhưng chưa có đặc tả Use Case Customer độc lập ở Chương 3.

---

## 2.17. Đánh giá sản phẩm

### Luồng nghiệp vụ theo yêu cầu và lớp `DanhGia`
1. Khách hàng đăng nhập.
2. Mở đơn hàng đã đủ điều kiện đánh giá.
3. Chọn dòng sản phẩm đã mua.
4. Hệ thống kiểm tra quyền đánh giá.
5. Khách hàng chọn số sao 1–5 và nhập nhận xét.
6. Hệ thống kiểm tra chưa có đánh giá trùng cho dòng hàng.
7. Hệ thống lưu đánh giá.
8. Hệ thống hiển thị đánh giá theo trạng thái cho phép.

### Ngoại lệ
- Đơn/dòng hàng không thuộc khách hàng: từ chối.
- Chưa đủ điều kiện đánh giá: từ chối.
- Dòng hàng đã được đánh giá: không tạo đánh giá thứ hai.
- Điểm ngoài 1–5: từ chối.

> Chương 2 và lớp `DanhGia` có chức năng này nhưng Chương 3 chưa có Use Case/đặc tả/Sequence riêng.

---

# 3. Chức năng dành cho Shop Manager

## 3.1. Quản lý sản phẩm

### Thêm sản phẩm
1. Manager đăng nhập và mở quản lý sản phẩm.
2. Hệ thống hiển thị danh sách.
3. Manager chọn thêm sản phẩm.
4. Nhập thông tin sản phẩm, danh mục, thương hiệu, giá, hình ảnh.
5. Hệ thống kiểm tra dữ liệu và mã duy nhất.
6. Nếu hợp lệ, tạo sản phẩm.
7. Hệ thống thông báo kết quả.

### Cập nhật sản phẩm
1. Chọn sản phẩm.
2. Hệ thống hiển thị dữ liệu hiện tại.
3. Manager chỉnh sửa trường được phép.
4. Hệ thống kiểm tra dữ liệu.
5. Cập nhật sản phẩm.
6. Thông báo kết quả.

### Ẩn/ngừng kinh doanh
1. Chọn sản phẩm.
2. Chọn ẩn/ngừng kinh doanh.
3. Xác nhận.
4. Hệ thống đổi trạng thái thay vì xóa vật lý nếu đã có giao dịch.
5. Sản phẩm không còn được bán cho giao dịch mới.

### Ngoại lệ
- Trùng mã/thiếu thông tin.
- Giá <= 0.
- Không tìm thấy sản phẩm.
- Sản phẩm đã có giao dịch không được xóa cứng.

---

## 3.2. Quản lý biến thể sản phẩm

### Xem danh sách biến thể
1. Manager chọn sản phẩm.
2. Hệ thống lấy danh sách biến thể.
3. Hiển thị size, màu, trạng thái và thông tin liên quan.

### Thêm biến thể
1. Chọn thêm biến thể.
2. Nhập size và màu.
3. Hệ thống kiểm tra tổ hợp size + màu trong cùng sản phẩm.
4. Nếu chưa trùng, tạo biến thể.
5. Thông báo kết quả.

### Cập nhật biến thể
1. Chọn biến thể.
2. Chỉnh sửa thông tin cho phép.
3. Hệ thống kiểm tra trùng.
4. Lưu thay đổi.

### Ẩn biến thể
1. Chọn biến thể.
2. Xác nhận ẩn.
3. Hệ thống chuyển trạng thái không kinh doanh.
4. Không xóa cứng biến thể đã từng xuất hiện trong đơn.

### Quy tắc
Thay đổi số lượng tồn kho **không thuộc** quản lý biến thể, mà thuộc quản lý tồn kho.

---

## 3.3. Quản lý danh mục

1. Manager mở danh sách danh mục.
2. Hệ thống hiển thị danh mục hiện có.
3. Manager chọn một trong các thao tác:
   - Thêm danh mục.
   - Cập nhật danh mục.
   - Ẩn/ngừng sử dụng danh mục.
4. Hệ thống kiểm tra tên trùng và ràng buộc liên kết.
5. Nếu danh mục đã được dùng bởi sản phẩm/giao dịch, không xóa cứng.
6. Hệ thống lưu kết quả.

---

## 3.4. Quản lý thương hiệu

1. Manager mở danh sách thương hiệu.
2. Hệ thống hiển thị dữ liệu.
3. Manager thêm/cập nhật/ẩn thương hiệu.
4. Hệ thống kiểm tra trùng tên.
5. Nếu thương hiệu đã có sản phẩm liên kết, không xóa cứng.
6. Lưu và thông báo kết quả.

---

## 3.5. Quản lý tồn kho

### Xem tồn kho
1. Manager chọn xem tồn kho.
2. Hệ thống lấy tồn theo từng biến thể.
3. Hiển thị tồn vật lý, lượng đã phân bổ và lượng khả dụng.
4. Biến thể chưa có dữ liệu tồn được hiểu là 0.

### Nhập kho
1. Manager chọn nhập kho.
2. Chọn biến thể, số lượng và thông tin/chứng từ tham chiếu.
3. Hệ thống kiểm tra biến thể và số lượng phải là số nguyên dương.
4. Kiểm tra chống ghi nhận cùng chứng từ hai lần.
5. Tạo phiếu/dòng nhập kho.
6. Tăng tồn vật lý.
7. Ghi biến động kho, người thực hiện và thời điểm.
8. Hiển thị tồn mới.

### Điều chỉnh tồn kho
1. Manager chọn điều chỉnh.
2. Chọn biến thể và cung cấp số lượng kiểm đếm/lý do.
3. Hệ thống kiểm tra dữ liệu.
4. Không cho điều chỉnh làm tồn khả dụng âm hoặc mâu thuẫn lượng đã phân bổ.
5. Ghi phiếu điều chỉnh.
6. Cập nhật tồn.
7. Ghi lịch sử biến động.
8. Hiển thị kết quả.

### Ngoại lệ
- Thiếu lý do.
- Số lượng không hợp lệ.
- Dữ liệu thay đổi đồng thời: tải lại/kiểm tra lại trước khi ghi.
- Không có chức năng xuất kho thủ công độc lập cho bán hàng.

---

## 3.6. Quản lý và xử lý đơn hàng

### Xem danh sách/chi tiết
1. Manager mở quản lý đơn.
2. Hệ thống hiển thị danh sách.
3. Manager chọn một đơn.
4. Hệ thống hiển thị thông tin chi tiết và trạng thái.

### Xác nhận đơn
1. Manager chọn đơn ở trạng thái **Chờ xác nhận**.
2. Kiểm tra thông tin cần thiết.
3. Chọn xác nhận.
4. Hệ thống kiểm tra trạng thái hiện tại.
5. Chuyển sang **Đã xác nhận**.
6. Ghi lịch sử trạng thái và người xử lý.

### Cập nhật trạng thái
1. Manager chọn trạng thái tiếp theo.
2. Hệ thống kiểm tra phép chuyển trạng thái.
3. Chuỗi nghiệp vụ chính: `Đã xác nhận -> Đang chuẩn bị -> Đang giao -> Đã giao -> Hoàn thành`.
4. Hệ thống cập nhật trạng thái.
5. Ghi lịch sử.
6. Trạng thái mới được phản ánh cho khách hàng.

### Hủy đơn bởi Manager
1. Manager chọn hủy và nhập lý do.
2. Hệ thống kiểm tra trạng thái cho phép.
3. Chuyển đơn sang Đã hủy.
4. Giải phóng tồn đã phân bổ nếu có.
5. Nếu có thanh toán, chuyển sang quy trình hoàn tiền cần thiết.
6. Ghi lịch sử và thông báo kết quả.

### Ngoại lệ
- Trạng thái không cho phép xác nhận/hủy.
- Chuyển trạng thái sai quy trình.
- Dữ liệu vừa được người khác thay đổi.
- Lưu thất bại: không để trạng thái và kho mất nhất quán.

---

## 3.7. Quản lý yêu cầu đổi/trả

### Tiếp nhận
1. Hệ thống có yêu cầu đổi/trả do khách gửi.
2. Manager mở danh sách chờ xử lý.
3. Chọn một yêu cầu.
4. Hệ thống hiển thị đơn hàng, dòng sản phẩm, số lượng, lý do và ảnh bằng chứng.

### Phê duyệt/từ chối
1. Manager kiểm tra điều kiện nghiệp vụ.
2. Chọn **Phê duyệt** hoặc **Từ chối**.
3. Nhập lý do xử lý.
4. Nếu từ chối: lưu quyết định, ghi lịch sử, thông báo khách hàng và kết thúc.
5. Nếu phê duyệt: xác định quy trình hoàn tiền và/hoặc nhận hàng hoàn.

### Hoàn tiền
1. Xác định số tiền được phép hoàn dựa trên phần hàng được duyệt.
2. Manager xác nhận số tiền.
3. Hệ thống tạo giao dịch hoàn tiền.
4. Khi có xác nhận thành công, cập nhật trạng thái hoàn tiền.
5. Không cho hoàn vượt số đã thanh toán hoặc hoàn lặp.

### Nhận hàng hoàn
1. Manager xác nhận đã nhận hàng.
2. Kiểm tra tình trạng hàng.
3. Ghi nhận số lượng đã nhận.
4. Nếu hàng đủ điều kiện bán lại, tạo nghiệp vụ nhận trả/nhập lại kho.
5. Nếu không đủ điều kiện, không tăng tồn khả dụng.
6. Cập nhật trạng thái yêu cầu.
7. Ghi lịch sử và hoàn tất khi các bước cần thiết đã xong.

### Ngoại lệ
- Yêu cầu/đơn không tồn tại.
- Số lượng yêu cầu > số đã mua.
- Hoàn tiền thất bại/chờ xác nhận.
- Yêu cầu đã xử lý: không thực hiện lần hai.

---

## 3.8. Quản lý khuyến mãi

### Tạo chương trình
1. Manager chọn tạo khuyến mãi.
2. Nhập nội dung, hình thức giảm, giá trị, thời gian, phạm vi và điều kiện áp dụng.
3. Hệ thống kiểm tra dữ liệu.
4. Lưu ở trạng thái chưa kích hoạt.
5. Ghi lịch sử tạo.

### Sửa chương trình
1. Chọn chương trình.
2. Thay đổi dữ liệu được phép.
3. Hệ thống kiểm tra.
4. Lưu dữ liệu mới.
5. Dữ liệu mới chỉ ảnh hưởng giao dịch sau thời điểm có hiệu lực, không làm thay đổi đơn cũ.

### Kích hoạt
1. Manager chọn kích hoạt.
2. Hệ thống kiểm tra thời gian và điều kiện.
3. Manager xác nhận.
4. Hệ thống chuyển sang đang áp dụng.
5. Ghi lịch sử.

### Kết thúc
1. Manager chọn kết thúc chương trình.
2. Xác nhận.
3. Hệ thống đổi trạng thái kết thúc.
4. Không áp dụng cho đơn mới.
5. Không xử lý lặp nếu đã kết thúc.

### Ngoại lệ
- Thời gian kết thúc < bắt đầu.
- Giá trị giảm sai.
- Chương trình hết hạn không thể kích hoạt.

---

## 3.9. Thống kê và lập báo cáo kinh doanh

1. Manager mở chức năng thống kê/báo cáo.
2. Chọn khoảng thời gian và tiêu chí.
3. Chọn loại báo cáo:
   - Tổng hợp doanh thu.
   - Thống kê số lượng đơn theo trạng thái.
   - Báo cáo kinh doanh tổng hợp.
   - Yêu cầu Chương 2 còn nêu sản phẩm bán chạy.
4. Hệ thống lấy dữ liệu đơn hàng, thanh toán và hoàn tiền.
5. Doanh thu chính thức chỉ tính các khoản thu đã xác nhận trừ các khoản hoàn đã xác nhận.
6. COD chưa thu/chờ xác nhận không cộng vào doanh thu chính thức.
7. Hệ thống tổng hợp kết quả.
8. Hiển thị khoảng thời gian, tiêu chí và thời điểm tổng hợp.
9. Nếu chọn xuất báo cáo, tạo tệp/kết quả tải về.

### Ngoại lệ
- Khoảng thời gian không hợp lệ: yêu cầu nhập lại.
- Không có dữ liệu: hiển thị 0/danh sách rỗng, không tạo dữ liệu giả.
- Không lấy đủ dữ liệu: không hiển thị báo cáo không hoàn chỉnh.

---

## 3.10. Xem thông tin khách hàng

1. Manager đăng nhập.
2. Mở chức năng tra cứu khách hàng.
3. Nhập điều kiện tìm kiếm/chọn khách hàng.
4. Hệ thống tìm tài khoản khách hàng.
5. Hiển thị thông tin phục vụ nghiệp vụ và lịch sử giao dịch liên quan.
6. Manager dùng dữ liệu để hỗ trợ xử lý đơn, đổi/trả hoặc chăm sóc khách hàng.
7. Manager không sử dụng chức năng này để tự ý thay đổi quyền quản trị tài khoản.

> Chương 2 có chức năng này nhưng Chương 3 chưa có Use Case/đặc tả/Sequence độc lập tương ứng.

---

# 4. Chức năng dành cho System Admin

## 4.1. Quản lý tài khoản người dùng

### Xem danh sách
1. Admin đăng nhập với quyền quản trị.
2. Mở quản lý người dùng.
3. Hệ thống hiển thị danh sách tài khoản và trạng thái.

### Thêm tài khoản
1. Admin chọn thêm.
2. Nhập thông tin tài khoản.
3. Chọn vai trò phù hợp.
4. Hệ thống kiểm tra thông tin duy nhất.
5. Tạo tài khoản.
6. Ghi nhật ký thao tác.

### Sửa thông tin tài khoản
1. Admin chọn tài khoản.
2. Chỉnh sửa dữ liệu được phép.
3. Hệ thống kiểm tra.
4. Lưu thay đổi.
5. Ghi nhật ký.

### Vô hiệu hóa/mở khóa
1. Admin chọn tài khoản.
2. Chọn trạng thái mong muốn.
3. Hệ thống kiểm tra quy tắc an toàn.
4. Nếu hợp lệ, cập nhật trạng thái.
5. Ghi nhật ký.

### Ngoại lệ
- Tài khoản không tồn tại.
- Thông tin định danh trùng.
- Không cho vô hiệu hóa quản trị viên hoạt động cuối cùng.
- Tài khoản đã phát sinh dữ liệu không bị xóa vật lý.

---

## 4.2. Phân quyền tài khoản

1. Admin chọn tài khoản.
2. Hệ thống hiển thị vai trò/quyền hiện tại.
3. Admin gán hoặc thu hồi vai trò.
4. Hệ thống kiểm tra phạm vi quyền của Admin.
5. Không cho người dùng tự nâng quyền vượt phạm vi.
6. Hệ thống lưu quan hệ tài khoản-vai trò.
7. Quyền bị thu hồi có hiệu lực ở lần kiểm tra tiếp theo.
8. Hệ thống ghi người gán, thời điểm và nhật ký.
9. Thông báo kết quả.

---

## 4.3. Ghi nhận nhật ký quản trị

1. Khi Admin thực hiện thao tác quan trọng, hệ thống tạo sự kiện nhật ký.
2. Ghi người thực hiện, thời điểm, hành động, đối tượng bị tác động và kết quả.
3. Có thể ghi mã truy vết/IP phục vụ kiểm tra.
4. Không ghi mật khẩu/token dạng rõ.
5. Nếu thao tác thất bại, vẫn ghi nhận kết quả thất bại khi phù hợp.

---

# 5. Các luồng nghiệp vụ xuyên suốt hệ thống

## 5.1. Luồng mua hàng hoàn chỉnh

```text
Khách hàng
  -> Đăng ký/Đăng nhập
  -> Tìm kiếm/Lọc sản phẩm
  -> Xem chi tiết sản phẩm
  -> Chọn size + màu
  -> Thêm vào giỏ
  -> Cập nhật giỏ
  -> Đặt hàng
  -> Kiểm tra giá + khuyến mãi + tồn kho
  -> Tạo đơn
  -> Phân bổ tồn kho
  -> Chọn phương thức thanh toán
     -> COD
     hoặc
     -> Thanh toán trực tuyến/chuyển khoản
  -> Shop Manager tiếp nhận đơn
  -> Xác nhận đơn
  -> Đang chuẩn bị
  -> Đang giao
  -> Đã giao
  -> Hoàn thành
  -> Khách hàng xem lịch sử/chi tiết
  -> Đánh giá sản phẩm (nếu đủ điều kiện)
```

## 5.2. Luồng hủy đơn

```text
Khách hàng hoặc Shop Manager yêu cầu hủy
  -> Kiểm tra quyền
  -> Kiểm tra trạng thái đơn
  -> [Không hợp lệ] từ chối
  -> [Hợp lệ] cập nhật Đã hủy
  -> Giải phóng tồn đã phân bổ
  -> [Đã thanh toán] tạo/ghi nhận hoàn tiền
  -> Ghi lịch sử trạng thái
  -> Trả kết quả
```

## 5.3. Luồng đổi/trả

```text
Khách hàng chọn đơn đã mua
  -> Gửi yêu cầu đổi/trả
  -> Hệ thống tạo yêu cầu Chờ duyệt
  -> Shop Manager tiếp nhận
  -> Kiểm tra đơn + dòng hàng + số lượng + lý do
  -> Phê duyệt hoặc Từ chối
     -> [Từ chối] ghi lý do -> thông báo khách
     -> [Phê duyệt]
          -> Nhận hàng hoàn
          -> Kiểm tra tình trạng
          -> [Bán lại được] nhập lại kho
          -> [Cần hoàn tiền] xử lý hoàn tiền
          -> [Đổi hàng] xử lý biến thể/đơn thay thế nếu áp dụng
          -> Hoàn tất yêu cầu
  -> Ghi lịch sử
```

## 5.4. Luồng tồn kho

```text
Nhập kho/Điều chỉnh
  -> Kiểm tra biến thể
  -> Kiểm tra số lượng/chứng từ/lý do
  -> Ghi phiếu kho
  -> Thay đổi tồn vật lý
  -> Ghi biến động kho

Đặt hàng
  -> Kiểm tra tồn khả dụng
  -> Phân bổ giữ hàng

Hủy đơn
  -> Giải phóng lượng đã phân bổ

Hoàn thành xuất bán
  -> Ghi nhận biến động bán hàng

Hàng trả đủ điều kiện
  -> Phiếu nhận trả
  -> Tăng tồn kho
```

## 5.5. Luồng khuyến mãi gắn với đặt hàng

```text
Shop Manager tạo khuyến mãi
  -> Kiểm tra điều kiện
  -> Kích hoạt
  -> Khách hàng đặt hàng
  -> Hệ thống tìm khuyến mãi còn hiệu lực
  -> Kiểm tra phạm vi/điều kiện
  -> Tính tiền giảm
  -> Ghi mức giảm thực tế lên đơn
  -> Đơn cũ giữ nguyên snapshot giảm giá
  -> Khi chương trình kết thúc, không áp dụng cho đơn mới
```

## 5.6. Luồng thanh toán và doanh thu

```text
Đơn hàng được tạo
  -> Khách chọn phương thức
  -> [COD] chưa ghi doanh thu đã thu cho đến khi giao thành công
  -> [Online] xử lý giao dịch -> xác nhận thành công/thất bại
  -> [Chuyển khoản] chờ xác nhận
  -> Giao dịch thành công được ghi nhận
  -> Báo cáo doanh thu lấy khoản thu đã xác nhận
  -> Nếu có hoàn tiền, trừ khoản hoàn đã xác nhận
```

---

# 6. Ma trận chức năng theo tác nhân

## Khách hàng
1. Đăng ký.
2. Đăng nhập.
3. Đăng xuất.
4. Khôi phục mật khẩu.
5. Quản lý thông tin cá nhân/tài khoản.
6. Tìm kiếm sản phẩm.
7. Lọc sản phẩm.
8. Xem chi tiết sản phẩm.
9. Thêm sản phẩm vào giỏ.
10. Cập nhật giỏ hàng.
11. Xóa sản phẩm khỏi giỏ.
12. Đặt hàng.
13. Thanh toán.
14. Xem lịch sử mua hàng.
15. Xem chi tiết đơn hàng.
16. Theo dõi trạng thái đơn hàng.
17. Hủy đơn hàng.
18. Gửi yêu cầu đổi/trả.
19. Đánh giá sản phẩm.

## Shop Manager
1. Quản lý sản phẩm.
2. Quản lý biến thể.
3. Quản lý danh mục.
4. Quản lý thương hiệu.
5. Xem tồn kho.
6. Nhập kho.
7. Điều chỉnh tồn kho.
8. Xem danh sách/chi tiết đơn hàng.
9. Xác nhận đơn.
10. Cập nhật trạng thái đơn.
11. Hủy đơn phía cửa hàng.
12. Tiếp nhận và xử lý đổi/trả.
13. Xử lý hoàn tiền/nhận hàng hoàn trong phạm vi đổi trả.
14. Tạo/sửa/kích hoạt/kết thúc khuyến mãi.
15. Thống kê doanh thu và đơn hàng.
16. Lập/xuất báo cáo kinh doanh.
17. Xem thông tin khách hàng phục vụ nghiệp vụ.

## System Admin
1. Xem danh sách tài khoản.
2. Thêm tài khoản.
3. Sửa thông tin tài khoản.
4. Vô hiệu hóa/mở khóa tài khoản.
5. Gán/thu hồi vai trò và phân quyền.
6. Ghi/kiểm soát nhật ký thao tác quản trị.

---

# 7. Điểm chưa đồng bộ trong báo cáo cần lưu ý khi dùng cho BFD/DFD

1. Chương 2 nói hệ thống có **ba** nhóm người dùng nhưng một câu vẫn ghi “hai phân quyền chính”.
2. `Quản lý thông tin tài khoản`, `Đánh giá sản phẩm`, `Đổi/trả hàng` ở phía Customer có trong yêu cầu Chương 2 nhưng chưa có đầy đủ Use Case + đặc tả + Sequence độc lập ở Chương 3.
3. `Theo dõi đơn hàng` được liệt kê riêng trong yêu cầu, nhưng Chương 3 chủ yếu thể hiện qua lịch sử và chi tiết đơn.
4. Use Case quản lý giỏ hàng đang gộp Thêm/Cập nhật/Xóa trong một đặc tả dù diagram thể hiện nhiều use case con.
5. Quản lý sản phẩm, biến thể, tồn kho, đơn hàng, khuyến mãi và quản trị người dùng cũng có các chức năng con. Khi xây dựng BFD/DFD nên xem chúng là các tiến trình con khi cần phân rã.
6. Tham chiếu `Quản lý tồn kho (UC-08)` trong đặc tả biến thể không còn khớp với cấu trúc đánh số hiện tại.
7. Báo cáo quy định thêm vào giỏ không giảm tồn kho; tồn được kiểm tra/phân bổ khi tạo đơn. Đây nên là quy tắc xuyên suốt DFD và các quy trình xử lý.

---

# 8. Gợi ý nhóm chức năng cấp cao để dựng BFD

```text
0. HỆ THỐNG QUẢN LÝ BÁN GIÀY
|
+-- 1. Quản lý tài khoản và người dùng
|   +-- Đăng ký
|   +-- Đăng nhập/Đăng xuất
|   +-- Khôi phục mật khẩu
|   +-- Quản lý hồ sơ
|   +-- Quản lý tài khoản và phân quyền
|
+-- 2. Quản lý tra cứu sản phẩm
|   +-- Tìm kiếm
|   +-- Lọc
|   +-- Xem chi tiết
|
+-- 3. Quản lý giỏ hàng và đặt hàng
|   +-- Thêm/Cập nhật/Xóa giỏ
|   +-- Kiểm tra giỏ
|   +-- Tạo đơn
|   +-- Thanh toán
|
+-- 4. Quản lý đơn hàng và hậu mãi
|   +-- Theo dõi/Lịch sử
|   +-- Xác nhận/Cập nhật trạng thái
|   +-- Hủy đơn
|   +-- Đổi/trả
|   +-- Hoàn tiền
|   +-- Đánh giá
|
+-- 5. Quản lý sản phẩm
|   +-- Sản phẩm
|   +-- Biến thể
|   +-- Danh mục
|   +-- Thương hiệu
|
+-- 6. Quản lý kho
|   +-- Xem tồn
|   +-- Nhập kho
|   +-- Điều chỉnh kho
|   +-- Phân bổ/Giải phóng tồn theo đơn
|
+-- 7. Quản lý khuyến mãi
|   +-- Tạo/Sửa
|   +-- Kích hoạt/Kết thúc
|   +-- Áp dụng vào đơn
|
+-- 8. Thống kê và báo cáo
    +-- Doanh thu
    +-- Đơn hàng
    +-- Báo cáo kinh doanh
```

---

# 9. Kết quả đối chiếu với báo cáo hiện tại

## 9.1. Kết luận đối chiếu

File chức năng và luồng hoạt động đã bao phủ phần lớn nghiệp vụ trong báo cáo, gồm xác thực, tra cứu sản phẩm, giỏ hàng, đặt hàng, thanh toán, theo dõi và hủy đơn, đổi/trả, sản phẩm, biến thể, kho, khuyến mãi, báo cáo và quản trị người dùng.

Tuy nhiên, khi đối chiếu với báo cáo hiện tại, vẫn cần bổ sung hoặc làm rõ các điểm sau:

1. **Quản lý thông tin tài khoản của khách hàng:** có trong mô tả nghiệp vụ và mô hình dữ liệu nhưng chưa có đầy đủ Use Case, đặc tả và Sequence Diagram trong báo cáo.
2. **Đánh giá sản phẩm:** có trong mục tiêu, lớp `DanhGia` và cơ sở dữ liệu nhưng chưa có đầy đủ Use Case, đặc tả và Sequence Diagram.
3. **Xem thông tin khách hàng:** được nêu ở yêu cầu của Shop Manager và Admin nhưng chưa có đầy đủ Use Case, đặc tả và Sequence Diagram độc lập.
4. **Thống kê sản phẩm bán chạy:** có trong bảng yêu cầu chức năng nhưng chưa được thể hiện rõ trong Use Case Diagram, đặc tả và Sequence Diagram báo cáo.
5. **Sửa chương trình khuyến mãi:** có trong yêu cầu và đặc tả tổng quát, nhưng danh sách Sequence Diagram hiện chỉ thể hiện Tạo, Kích hoạt và Kết thúc; cần bổ sung Sequence Diagram Sửa khuyến mãi nếu đây là Use Case riêng.
6. **Cập nhật trạng thái đơn hàng:** có trong đặc tả quản lý đơn nhưng Use Case Diagram/Sequence cần đặt tên và phạm vi thống nhất.
7. **Thiết kế giao diện:** phần “Mô tả giao diện” trong báo cáo chưa có nội dung. Cần thiết kế giao diện cho toàn bộ chức năng, kèm control và event.
8. **Kết luận, ưu điểm, hạn chế và hướng phát triển:** cần có ở cuối báo cáo theo yêu cầu cuối kỳ.

## 9.2. Các lỗi không nhất quán cần sửa trong báo cáo

- Câu “Hệ thống gồm hai phân quyền chính” phải đổi thành **ba nhóm người sử dụng chính**: Customer, Shop Manager và System Admin.
- Bảng yêu cầu Customer cần bổ sung **Đăng xuất** và **Khôi phục mật khẩu**, vì Chương 3 đã có Use Case, đặc tả và Sequence tương ứng.
- Đổi tên **Xem sản phẩm** thành **Xem chi tiết sản phẩm** để thống nhất toàn báo cáo.
- Quản lý danh mục và thương hiệu phải dùng **Ẩn/ngừng sử dụng**, không dùng “xóa cứng” khi dữ liệu đã được liên kết.
- Bảng đặc tả dưới hình Quản lý danh mục đang ghi nhầm tên **Quản lý thương hiệu**; phải thay bằng đặc tả Quản lý danh mục.
- Bảng mô tả tổng quát cơ sở dữ liệu đang lặp tên `ThanhToan`; dòng phương thức thanh toán phải là `PhuongThucThanhToan`.
- “Thống kê và lập doanh thu” nên đổi thành **Thống kê và lập báo cáo kinh doanh** hoặc tách rõ Doanh thu, Đơn hàng, Sản phẩm bán chạy và Xuất báo cáo.
- Sequence Diagram Quản lý khuyến mãi cần bổ sung **Sửa khuyến mãi** nếu Use Case Diagram xác định đây là một Use Case riêng.
- Use Case Diagram Gửi yêu cầu đổi/trả phía Customer và Xử lý yêu cầu đổi/trả phía Shop Manager phải dùng đúng actor và đúng phạm vi, không dùng cùng một sơ đồ cho hai phía.

---

# 10. Các chức năng bổ sung cần hoàn thiện

## 10.1. Xem và cập nhật thông tin tài khoản

### Mục tiêu

Cho phép khách hàng xem và cập nhật hồ sơ cá nhân thuộc tài khoản của mình.

### Luồng chính

1. Khách hàng đăng nhập.
2. Khách hàng mở trang **Thông tin tài khoản**.
3. Hệ thống kiểm tra phiên và hiển thị hồ sơ hiện tại.
4. Khách hàng chỉnh sửa họ tên, số điện thoại, ngày sinh, giới tính, địa chỉ hoặc ảnh đại diện theo phạm vi cho phép.
5. Hệ thống kiểm tra định dạng, dữ liệu bắt buộc và tính duy nhất của email/số điện thoại nếu có thay đổi.
6. Hệ thống yêu cầu xác minh lại đối với thay đổi nhạy cảm khi cần.
7. Khách hàng xác nhận lưu.
8. Hệ thống cập nhật hồ sơ và thời điểm cập nhật.
9. Hệ thống thông báo kết quả.

### Luồng thay thế/ngoại lệ

- Phiên hết hạn: yêu cầu đăng nhập lại.
- Dữ liệu sai định dạng: đánh dấu trường lỗi và không lưu.
- Email/số điện thoại đã được sử dụng: từ chối cập nhật.
- Dữ liệu bị thay đổi đồng thời: yêu cầu tải lại hồ sơ trước khi lưu.

### Quy tắc nghiệp vụ

- Khách hàng chỉ được cập nhật hồ sơ của chính mình.
- Vai trò, trạng thái khóa và quyền truy cập không được thay đổi tại chức năng này.
- Thay đổi mật khẩu thực hiện bằng luồng riêng và yêu cầu xác minh phù hợp.

## 10.2. Đánh giá sản phẩm

### Mục tiêu

Cho phép khách hàng đánh giá sản phẩm đã mua sau khi đơn hàng đủ điều kiện.

### Luồng chính

1. Khách hàng đăng nhập và mở lịch sử mua hàng.
2. Khách hàng chọn đơn đã hoàn thành.
3. Hệ thống hiển thị các dòng hàng có thể đánh giá.
4. Khách hàng chọn một dòng hàng.
5. Hệ thống kiểm tra quyền sở hữu đơn, trạng thái đơn và đánh giá đã tồn tại hay chưa.
6. Khách hàng chọn điểm từ 1 đến 5 sao và nhập nhận xét.
7. Khách hàng xác nhận gửi đánh giá.
8. Hệ thống lưu đánh giá gắn với dòng đơn hàng và khách hàng.
9. Hệ thống hiển thị kết quả theo trạng thái hiển thị được quy định.

### Luồng thay thế/ngoại lệ

- Đơn không thuộc khách hàng: từ chối.
- Đơn chưa hoàn thành: chưa cho phép đánh giá.
- Dòng hàng đã được đánh giá: không tạo đánh giá thứ hai; có thể chuyển sang chỉnh sửa nếu hệ thống hỗ trợ.
- Điểm không thuộc 1–5 hoặc nội dung vượt giới hạn: yêu cầu sửa.

### Quy tắc nghiệp vụ

- Mỗi dòng đơn hàng tối đa một đánh giá của khách hàng sở hữu đơn.
- Đánh giá phải gắn với giao dịch mua thật.
- Đánh giá bị ẩn không bị xóa khỏi lịch sử dữ liệu nếu cần kiểm soát.

## 10.3. Xem thông tin khách hàng cho Admin

### Mục tiêu

Cho phép System Admin tra cứu tài khoản khách hàng để hỗ trợ xử lý sự cố kỹ thuật và bảo mật, không thay thế nghiệp vụ chăm sóc khách hàng của Shop Manager.

### Luồng chính

1. Admin đăng nhập bằng tài khoản có quyền.
2. Mở chức năng quản lý tài khoản khách hàng.
3. Nhập tên đăng nhập, email, số điện thoại hoặc mã tài khoản.
4. Hệ thống trả về danh sách phù hợp.
5. Admin chọn tài khoản.
6. Hệ thống hiển thị thông tin tài khoản, vai trò, trạng thái, lần đăng nhập cuối và lịch sử khóa/mở khóa cần thiết.
7. Admin có thể chuyển sang thao tác khóa, mở khóa hoặc hỗ trợ khôi phục theo quyền.
8. Hệ thống ghi nhật ký mọi thao tác quan trọng.

### Giới hạn phạm vi

- Admin không tự ý sửa dữ liệu giao dịch kinh doanh của khách hàng.
- Shop Manager chỉ xem thông tin phục vụ đơn hàng, đổi/trả và chăm sóc khách hàng.
- Không hiển thị mật khẩu, token hoặc dữ liệu xác thực dạng rõ.

## 10.4. Thống kê sản phẩm bán chạy

### Mục tiêu

Xếp hạng sản phẩm hoặc biến thể bán chạy theo khoảng thời gian và tiêu chí lựa chọn.

### Luồng chính

1. Shop Manager mở chức năng báo cáo.
2. Chọn **Sản phẩm bán chạy**.
3. Chọn thời gian bắt đầu, thời gian kết thúc và tiêu chí xếp hạng.
4. Hệ thống kiểm tra khoảng thời gian.
5. Hệ thống lấy các dòng đơn hàng đủ điều kiện.
6. Hệ thống loại trừ đơn bị hủy và điều chỉnh theo hàng trả/hoàn tiền đã xác nhận.
7. Hệ thống tổng hợp theo sản phẩm hoặc biến thể.
8. Hệ thống xếp hạng theo số lượng bán hoặc giá trị thực thu.
9. Hệ thống hiển thị kết quả và cho phép xuất báo cáo.

### Quy tắc nghiệp vụ

- Không tính đơn bị hủy.
- Chỉ tính giao dịch đủ điều kiện theo chính sách doanh thu.
- Báo cáo phải ghi rõ khoảng thời gian, tiêu chí và thời điểm tổng hợp.

## 10.5. Sửa chương trình khuyến mãi

### Mục tiêu

Cho phép Shop Manager cập nhật nội dung chương trình khuyến mãi trước hoặc trong thời gian hiệu lực theo quy tắc cho phép.

### Luồng chính

1. Shop Manager mở danh sách khuyến mãi.
2. Chọn chương trình cần sửa.
3. Hệ thống hiển thị dữ liệu và trạng thái hiện tại.
4. Shop Manager chỉnh sửa thông tin được phép.
5. Hệ thống kiểm tra thời gian, giá trị giảm, phạm vi và điều kiện áp dụng.
6. Shop Manager xác nhận lưu.
7. Hệ thống cập nhật chương trình và ghi lịch sử thay đổi.
8. Hệ thống thông báo kết quả.

### Luồng thay thế/ngoại lệ

- Chương trình đã kết thúc: không cho sửa các trường làm chương trình áp dụng trở lại.
- Thời gian hoặc giá trị giảm không hợp lệ: yêu cầu sửa.
- Dữ liệu mới ảnh hưởng giao dịch cũ: chỉ áp dụng cho giao dịch mới; đơn đã tạo giữ nguyên dữ liệu giảm giá tại thời điểm đặt.

---

# 11. Bổ sung yêu cầu thiết kế giao diện

Phần giao diện phải được xây dựng cho toàn bộ chức năng đã nêu. Mỗi giao diện cần có: mục đích, actor sử dụng, hình wireframe/mockup, danh sách control và danh sách event.

## 11.1. Giao diện Customer

1. Đăng ký tài khoản.
2. Đăng nhập.
3. Khôi phục mật khẩu.
4. Thông tin tài khoản.
5. Danh sách và tìm kiếm/lọc sản phẩm.
6. Chi tiết sản phẩm.
7. Giỏ hàng.
8. Xác nhận đặt hàng.
9. Thanh toán.
10. Lịch sử và theo dõi đơn hàng.
11. Gửi/theo dõi yêu cầu đổi trả.
12. Đánh giá sản phẩm.

## 11.2. Giao diện Shop Manager

1. Quản lý sản phẩm.
2. Quản lý biến thể.
3. Quản lý danh mục.
4. Quản lý thương hiệu.
5. Quản lý tồn kho.
6. Quản lý đơn hàng.
7. Xử lý đổi/trả và hoàn tiền.
8. Quản lý khuyến mãi.
9. Thống kê và báo cáo.
10. Xem thông tin khách hàng.

## 11.3. Giao diện System Admin

1. Danh sách tài khoản.
2. Thêm/sửa tài khoản.
3. Khóa/mở khóa tài khoản.
4. Phân quyền.
5. Tra cứu khách hàng.
6. Xem nhật ký quản trị nếu chức năng này được đưa vào phạm vi.

## 11.4. Mẫu mô tả một giao diện

| Thành phần | Nội dung cần mô tả |
|---|---|
| Tên giao diện | Ví dụ: Quản lý tồn kho |
| Actor | Shop Manager |
| Mục đích | Xem tồn, nhập kho và điều chỉnh tồn theo biến thể |
| Control | Ô tìm kiếm, bộ lọc, bảng tồn kho, nút Nhập kho, nút Điều chỉnh, hộp thoại xác nhận |
| Event | Search, FilterChanged, ClickImport, ClickAdjust, Confirm, Cancel |
| Kết quả | Hiển thị dữ liệu mới hoặc thông báo lỗi cụ thể |
| Use Case liên quan | Xem tồn kho, Nhập kho, Điều chỉnh tồn kho |

---

# 12. Ma trận chức năng cập nhật sau đối chiếu

## Khách hàng

1. Đăng ký.
2. Đăng nhập.
3. Đăng xuất.
4. Khôi phục mật khẩu.
5. Xem/cập nhật thông tin tài khoản.
6. Tìm kiếm sản phẩm.
7. Lọc sản phẩm.
8. Xem chi tiết sản phẩm.
9. Thêm sản phẩm vào giỏ.
10. Cập nhật giỏ hàng.
11. Xóa sản phẩm khỏi giỏ.
12. Đặt hàng.
13. Thanh toán.
14. Xem lịch sử mua hàng.
15. Xem chi tiết/theo dõi đơn hàng.
16. Hủy đơn hàng.
17. Gửi yêu cầu đổi/trả.
18. Theo dõi kết quả đổi/trả.
19. Đánh giá sản phẩm.

## Shop Manager

1. Thêm/cập nhật/ẩn sản phẩm.
2. Xem/thêm/cập nhật/ẩn biến thể.
3. Thêm/cập nhật/ẩn danh mục.
4. Thêm/cập nhật/ẩn thương hiệu.
5. Xem tồn kho.
6. Nhập kho.
7. Điều chỉnh tồn kho.
8. Xem danh sách/chi tiết đơn hàng.
9. Xác nhận đơn.
10. Cập nhật trạng thái đơn.
11. Hủy đơn phía cửa hàng.
12. Tiếp nhận, duyệt/từ chối đổi trả.
13. Xử lý hoàn tiền và nhận hàng hoàn.
14. Tạo/sửa/kích hoạt/kết thúc khuyến mãi.
15. Tổng hợp doanh thu.
16. Thống kê số đơn.
17. Thống kê sản phẩm bán chạy.
18. Lập/xuất báo cáo kinh doanh.
19. Xem thông tin khách hàng phục vụ nghiệp vụ.

## System Admin

1. Xem danh sách tài khoản.
2. Thêm tài khoản.
3. Sửa tài khoản.
4. Vô hiệu hóa/mở khóa tài khoản.
5. Phân quyền tài khoản.
6. Tra cứu thông tin tài khoản khách hàng để hỗ trợ kỹ thuật.
7. Xem/kiểm soát nhật ký quản trị nếu thuộc phạm vi.

---

# 13. BFD cập nhật đề xuất

```text
0. HỆ THỐNG QUẢN LÝ BÁN GIÀY
|
+-- 1. Quản lý tài khoản và xác thực
|   +-- Đăng ký
|   +-- Đăng nhập
|   +-- Đăng xuất
|   +-- Khôi phục mật khẩu
|   +-- Xem/cập nhật hồ sơ
|   +-- Quản lý tài khoản và phân quyền
|
+-- 2. Tra cứu sản phẩm
|   +-- Tìm kiếm
|   +-- Lọc
|   +-- Xem chi tiết
|
+-- 3. Quản lý giỏ hàng và đặt hàng
|   +-- Thêm/Cập nhật/Xóa giỏ
|   +-- Kiểm tra giỏ
|   +-- Tạo đơn
|   +-- Thanh toán
|
+-- 4. Quản lý đơn hàng và hậu mãi
|   +-- Lịch sử/Theo dõi đơn
|   +-- Xác nhận/Cập nhật trạng thái
|   +-- Hủy đơn
|   +-- Gửi/Xử lý đổi trả
|   +-- Hoàn tiền/Nhận hàng hoàn
|   +-- Đánh giá sản phẩm
|
+-- 5. Quản lý sản phẩm
|   +-- Sản phẩm
|   +-- Biến thể
|   +-- Danh mục
|   +-- Thương hiệu
|
+-- 6. Quản lý kho
|   +-- Xem tồn
|   +-- Nhập kho
|   +-- Điều chỉnh kho
|   +-- Phân bổ/Giải phóng tồn theo đơn
|
+-- 7. Quản lý khuyến mãi
|   +-- Tạo
|   +-- Sửa
|   +-- Kích hoạt
|   +-- Kết thúc
|   +-- Áp dụng vào đơn
|
+-- 8. Thống kê và báo cáo
|   +-- Doanh thu
|   +-- Đơn hàng
|   +-- Sản phẩm bán chạy
|   +-- Lập/Xuất báo cáo
|
+-- 9. Quản trị hệ thống
    +-- Quản lý tài khoản
    +-- Phân quyền
    +-- Tra cứu tài khoản khách hàng
    +-- Nhật ký quản trị
```
