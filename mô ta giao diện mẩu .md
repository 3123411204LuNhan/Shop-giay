# MÔ TẢ GIAO DIỆN HỆ THỐNG QUẢN LÝ BÁN GIÀY

> Tài liệu được xây dựng theo cấu trúc phần **Mô tả giao diện** trong báo cáo tham khảo `OOAD_Nhóm 12.docx`: mỗi mục gồm mục đích, tác nhân, vị trí chèn hình giao diện, bảng thành phần và bảng biến cố. Nội dung nghiệp vụ được điều chỉnh theo báo cáo hệ thống quản lý bán giày của Nhóm 19.

## Quy ước trình bày

- Mỗi giao diện cần chèn một hình wireframe/mockup ngay dưới tiêu đề.
- Bảng thành phần dùng 7 cột: STT, Tên, Kiểu, Ý nghĩa, Miền giá trị, Giá trị mặc định, Ghi chú.
- Bảng biến cố dùng 3 cột: STT, Điều kiện kích hoạt, Xử lý.
- Tên control nên thống nhất với thiết kế và mã nguồn, ví dụ `btnDangNhap`, `txtTimKiem`, `tblDonHang`.
- Các màn hình quản trị phải ẩn hoặc vô hiệu hóa control nếu người dùng không có quyền.

## Danh sách giao diện

1. Đăng nhập — Customer, Shop Manager, System Admin
2. Đăng ký tài khoản — Customer
3. Khôi phục mật khẩu — Customer
4. Trang chủ và danh sách sản phẩm — Customer
5. Xem chi tiết sản phẩm — Customer
6. Giỏ hàng — Customer
7. Xác nhận đặt hàng — Customer
8. Thanh toán — Customer
9. Lịch sử và theo dõi đơn hàng — Customer
10. Gửi yêu cầu đổi/trả — Customer
11. Đánh giá sản phẩm — Customer
12. Quản lý sản phẩm — Shop Manager
13. Thêm/Cập nhật sản phẩm — Shop Manager
14. Quản lý biến thể — Shop Manager
15. Quản lý danh mục và thương hiệu — Shop Manager
16. Quản lý tồn kho — Shop Manager
17. Quản lý đơn hàng — Shop Manager
18. Xử lý yêu cầu đổi/trả — Shop Manager
19. Quản lý khuyến mãi — Shop Manager
20. Thống kê và báo cáo — Shop Manager
21. Quản lý tài khoản và phân quyền — System Admin

---

## 1. Giao diện Đăng nhập

**Tác nhân sử dụng:** Customer, Shop Manager, System Admin  
**Mục đích:** Cho phép người dùng xác thực để truy cập đúng phân hệ.

> **Vị trí chèn hình:** Hình UI.1. Giao diện Đăng nhập.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTenDangNhap | TextBox | Nhập tên đăng nhập hoặc email | Chuỗi 3–100 ký tự | Rỗng | Bắt buộc |
| 2 | txtMatKhau | PasswordBox | Nhập mật khẩu | Chuỗi 8–255 ký tự | Rỗng | Ẩn ký tự |
| 3 | chkGhiNho | CheckBox | Ghi nhớ phiên đăng nhập | Có/Không | Không chọn | Tùy chọn |
| 4 | btnDangNhap | Button | Gửi yêu cầu đăng nhập | Không áp dụng | Khả dụng | Khóa tạm khi đang xử lý |
| 5 | lnkQuenMatKhau | Link | Mở giao diện khôi phục mật khẩu | Không áp dụng | Khả dụng |  |
| 6 | lnkDangKy | Link | Mở giao diện đăng ký khách hàng | Không áp dụng | Khả dụng | Chỉ hiện ở cổng khách hàng |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Đăng nhập | Kiểm tra dữ liệu; nếu hợp lệ thì xác thực tài khoản, tạo phiên và chuyển đến giao diện phù hợp với vai trò. |
| 2 | Nhấn Quên mật khẩu | Chuyển sang giao diện khôi phục mật khẩu. |
| 3 | Nhấn Đăng ký | Chuyển sang giao diện đăng ký tài khoản. |
| 4 | Nhấn Enter trong ô mật khẩu | Thực hiện tương tự nút Đăng nhập. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Đăng nhập** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 2. Giao diện Đăng ký tài khoản

**Tác nhân sử dụng:** Customer  
**Mục đích:** Cho phép khách hàng tạo tài khoản mới.

> **Vị trí chèn hình:** Hình UI.2. Giao diện Đăng ký tài khoản.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtHoTen | TextBox | Nhập họ tên khách hàng | Chuỗi 2–100 ký tự | Rỗng | Bắt buộc |
| 2 | txtEmail | TextBox | Nhập email | Định dạng email | Rỗng | Bắt buộc, không trùng |
| 3 | txtSoDienThoai | TextBox | Nhập số điện thoại | 10–11 chữ số | Rỗng | Bắt buộc, không trùng |
| 4 | txtMatKhau | PasswordBox | Nhập mật khẩu | Tối thiểu 8 ký tự | Rỗng | Bắt buộc |
| 5 | txtXacNhanMatKhau | PasswordBox | Nhập lại mật khẩu | Trùng mật khẩu | Rỗng | Bắt buộc |
| 6 | chkDongYDieuKhoan | CheckBox | Xác nhận đồng ý điều khoản | Có/Không | Không chọn | Bắt buộc |
| 7 | btnDangKy | Button | Tạo tài khoản | Không áp dụng | Khả dụng |  |
| 8 | btnQuayLai | Button | Quay lại đăng nhập | Không áp dụng | Khả dụng |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Đăng ký | Kiểm tra trường bắt buộc, định dạng, tính duy nhất và mật khẩu xác nhận; nếu hợp lệ thì tạo tài khoản. |
| 2 | Bỏ chọn điều khoản | Không cho gửi đăng ký. |
| 3 | Nhấn Quay lại | Trở về giao diện đăng nhập. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Đăng ký tài khoản** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 3. Giao diện Khôi phục mật khẩu

**Tác nhân sử dụng:** Customer  
**Mục đích:** Cho phép xác minh tài khoản và thiết lập mật khẩu mới.

> **Vị trí chèn hình:** Hình UI.3. Giao diện Khôi phục mật khẩu.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtEmail | TextBox | Nhập email nhận mã/liên kết | Định dạng email | Rỗng | Bắt buộc |
| 2 | btnGuiMa | Button | Yêu cầu mã xác minh | Không áp dụng | Khả dụng | Có thời gian chờ gửi lại |
| 3 | txtMaXacMinh | TextBox | Nhập mã xác minh | Chuỗi số/ký tự | Rỗng | Chỉ hiện sau khi gửi mã |
| 4 | txtMatKhauMoi | PasswordBox | Nhập mật khẩu mới | Tối thiểu 8 ký tự | Rỗng |  |
| 5 | txtXacNhanMatKhau | PasswordBox | Nhập lại mật khẩu mới | Trùng mật khẩu mới | Rỗng |  |
| 6 | btnDatLai | Button | Đặt lại mật khẩu | Không áp dụng | Vô hiệu hóa ban đầu | Chỉ bật khi đủ dữ liệu |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Gửi mã | Tạo mã xác minh có thời hạn và gửi hướng dẫn; thông báo chung không tiết lộ tài khoản tồn tại. |
| 2 | Nhấn Đặt lại | Kiểm tra mã, thời hạn và chính sách mật khẩu; nếu hợp lệ thì cập nhật mật khẩu. |
| 3 | Mã hết hạn/sai | Hiển thị lỗi và cho phép yêu cầu mã mới. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Khôi phục mật khẩu** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 4. Giao diện Trang chủ và danh sách sản phẩm

**Tác nhân sử dụng:** Customer  
**Mục đích:** Tra cứu, tìm kiếm và lọc sản phẩm đang kinh doanh.

> **Vị trí chèn hình:** Hình UI.4. Giao diện Trang chủ và danh sách sản phẩm.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTimKiem | SearchBox | Nhập tên, mã hoặc từ khóa sản phẩm | Chuỗi 0–100 ký tự | Rỗng |  |
| 2 | cboDanhMuc | ComboBox | Lọc theo danh mục | Danh mục hoạt động | Tất cả |  |
| 3 | cboThuongHieu | ComboBox | Lọc theo thương hiệu | Thương hiệu hoạt động | Tất cả |  |
| 4 | rngGia | RangeSlider | Chọn khoảng giá | Giá >= 0 | Toàn bộ |  |
| 5 | cboKichCo | ComboBox | Lọc theo kích cỡ | Danh sách kích cỡ | Tất cả |  |
| 6 | cboMauSac | ComboBox | Lọc theo màu | Danh sách màu | Tất cả |  |
| 7 | lstSanPham | ProductGrid | Hiển thị các thẻ sản phẩm | Danh sách sản phẩm | Danh sách mới nhất |  |
| 8 | btnBoLoc | Button | Xóa toàn bộ điều kiện lọc | Không áp dụng | Khả dụng |  |
| 9 | btnGioHang | Button/Icon | Mở giỏ hàng | Không áp dụng | Khả dụng | Hiển thị số dòng hàng |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhập từ khóa/nhấn Enter | Tìm các sản phẩm đang công bố phù hợp và cập nhật danh sách. |
| 2 | Thay đổi bộ lọc | Kết hợp từ khóa và các điều kiện lọc rồi tải lại kết quả. |
| 3 | Nhấn thẻ sản phẩm | Mở giao diện Xem chi tiết sản phẩm. |
| 4 | Nhấn Bỏ lọc | Đưa các bộ lọc về mặc định và hiển thị lại toàn bộ sản phẩm. |
| 5 | Không có kết quả | Hiển thị trạng thái rỗng và gợi ý thay đổi điều kiện. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Trang chủ và danh sách sản phẩm** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 5. Giao diện Xem chi tiết sản phẩm

**Tác nhân sử dụng:** Customer  
**Mục đích:** Hiển thị thông tin sản phẩm, biến thể và tình trạng còn hàng.

> **Vị trí chèn hình:** Hình UI.5. Giao diện Xem chi tiết sản phẩm.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | galHinhAnh | ImageGallery | Hiển thị hình ảnh sản phẩm | Danh sách ảnh | Ảnh đại diện |  |
| 2 | lblTenSanPham | Label | Hiển thị tên sản phẩm | Chuỗi | Dữ liệu sản phẩm | Chỉ đọc |
| 3 | lblGia | Label | Hiển thị giá hiện hành | Số tiền >= 0 | Dữ liệu sản phẩm | Chỉ đọc |
| 4 | grpKichCo | RadioGroup | Chọn kích cỡ | Biến thể khả dụng | Chưa chọn |  |
| 5 | grpMauSac | RadioGroup | Chọn màu sắc | Biến thể khả dụng | Chưa chọn |  |
| 6 | spnSoLuong | NumberSpinner | Chọn số lượng mua | 1 đến tồn khả dụng | 1 |  |
| 7 | lblTonKho | Label | Hiển thị còn hàng/hết hàng | Trạng thái | Theo biến thể | Chỉ đọc |
| 8 | btnThemGio | Button | Thêm biến thể vào giỏ | Không áp dụng | Vô hiệu hóa khi chưa chọn/hết hàng |  |
| 9 | tabMoTaDanhGia | Tabs | Hiển thị mô tả và đánh giá | Mô tả/Đánh giá | Mô tả |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Chọn kích cỡ/màu | Xác định biến thể và cập nhật tồn khả dụng, giá nếu có. |
| 2 | Thay đổi số lượng | Giới hạn số lượng theo tồn khả dụng. |
| 3 | Nhấn Thêm vào giỏ | Kiểm tra biến thể, số lượng; tạo mới hoặc gộp dòng hàng trong giỏ. |
| 4 | Biến thể hết hàng | Không cho chọn mua và hiển thị trạng thái hết hàng. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Xem chi tiết sản phẩm** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 6. Giao diện Giỏ hàng

**Tác nhân sử dụng:** Customer  
**Mục đích:** Xem và cập nhật các sản phẩm dự kiến mua.

> **Vị trí chèn hình:** Hình UI.6. Giao diện Giỏ hàng.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | tblGioHang | Table/List | Hiển thị các dòng giỏ hàng | Danh sách dòng hàng | Dữ liệu giỏ |  |
| 2 | spnSoLuong | NumberSpinner | Thay đổi số lượng từng dòng | 1 đến tồn khả dụng | Số lượng hiện tại |  |
| 3 | btnXoaDong | Button/Icon | Xóa dòng hàng | Không áp dụng | Khả dụng | Yêu cầu xác nhận |
| 4 | lblTamTinh | Label | Hiển thị tổng tạm tính | Số tiền >= 0 | 0 | Chỉ đọc |
| 5 | btnTiepTucMua | Button | Quay lại danh sách sản phẩm | Không áp dụng | Khả dụng |  |
| 6 | btnDatHang | Button | Mở bước xác nhận đơn | Không áp dụng | Vô hiệu hóa khi giỏ trống |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Thay đổi số lượng | Kiểm tra tồn khả dụng, cập nhật dòng hàng và tính lại tổng tiền. |
| 2 | Nhấn Xóa | Hiển thị hộp xác nhận; nếu đồng ý thì xóa dòng và tính lại tổng. |
| 3 | Nhấn Đặt hàng | Kiểm tra giỏ và chuyển sang giao diện Xác nhận đặt hàng. |
| 4 | Giỏ trống | Hiển thị trạng thái giỏ trống và vô hiệu hóa Đặt hàng. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Giỏ hàng** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 7. Giao diện Xác nhận đặt hàng

**Tác nhân sử dụng:** Customer  
**Mục đích:** Xác nhận giỏ hàng và thông tin nhận hàng trước khi tạo đơn.

> **Vị trí chèn hình:** Hình UI.7. Giao diện Xác nhận đặt hàng.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | tblSanPham | Table/List | Hiển thị các dòng hàng sẽ đặt | Danh sách dòng hàng | Dữ liệu giỏ | Chỉ đọc |
| 2 | txtHoTenNhan | TextBox | Họ tên người nhận | Chuỗi 2–100 ký tự | Hồ sơ khách hàng | Bắt buộc |
| 3 | txtSoDienThoaiNhan | TextBox | Số điện thoại nhận hàng | 10–11 chữ số | Hồ sơ khách hàng | Bắt buộc |
| 4 | txtDiaChiNhan | TextArea | Địa chỉ giao hàng | Chuỗi 10–255 ký tự | Địa chỉ mặc định | Bắt buộc |
| 5 | txtMaKhuyenMai | TextBox | Nhập mã khuyến mãi | Chuỗi | Rỗng | Tùy chọn |
| 6 | btnApDung | Button | Kiểm tra và áp dụng khuyến mãi | Không áp dụng | Khả dụng |  |
| 7 | lblTongThanhToan | Label | Tổng tiền sau giảm và phí | Số tiền >= 0 | Theo đơn | Chỉ đọc |
| 8 | btnTaoDon | Button | Xác nhận tạo đơn hàng | Không áp dụng | Khả dụng |  |
| 9 | btnQuayLaiGio | Button | Trở về giỏ hàng | Không áp dụng | Khả dụng |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Áp dụng | Kiểm tra mã, hiệu lực, điều kiện và phạm vi; tính lại tổng thanh toán. |
| 2 | Nhấn Tạo đơn | Kiểm tra lại giá, khuyến mãi, tồn kho và thông tin nhận hàng; tạo đơn và phân bổ tồn. |
| 3 | Giá/tồn thay đổi | Hiển thị dữ liệu mới và yêu cầu khách hàng xác nhận lại. |
| 4 | Tạo đơn thành công | Hiển thị mã đơn và chuyển sang Thanh toán. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Xác nhận đặt hàng** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 8. Giao diện Thanh toán

**Tác nhân sử dụng:** Customer  
**Mục đích:** Cho phép chọn và thực hiện phương thức thanh toán cho đơn hàng.

> **Vị trí chèn hình:** Hình UI.8. Giao diện Thanh toán.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | lblMaDon | Label | Hiển thị mã đơn | Chuỗi | Đơn hiện tại | Chỉ đọc |
| 2 | lblSoTien | Label | Hiển thị số tiền cần thanh toán | Số tiền >= 0 | Theo đơn | Chỉ đọc |
| 3 | grpPhuongThuc | RadioGroup | Chọn COD, chuyển khoản hoặc trực tuyến | Danh sách phương thức hoạt động | COD |  |
| 4 | pnlHuongDan | Panel | Hiển thị hướng dẫn theo phương thức | Nội dung động | Theo lựa chọn |  |
| 5 | btnThanhToan | Button | Xác nhận/thực hiện thanh toán | Không áp dụng | Khả dụng |  |
| 6 | btnThuLai | Button | Thử lại giao dịch thất bại | Không áp dụng | Ẩn ban đầu |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Chọn COD | Ghi nhận COD và chuyển đơn sang chờ xác nhận, chưa đánh dấu đã thu tiền. |
| 2 | Chọn trực tuyến | Tạo giao dịch, chuyển đến cổng thanh toán và xử lý kết quả trả về. |
| 3 | Chọn chuyển khoản | Hiển thị thông tin chuyển khoản và ghi nhận trạng thái chờ xác nhận. |
| 4 | Thanh toán thất bại | Hiển thị lỗi và nút Thử lại; không ghi nhận thanh toán trùng. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Thanh toán** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 9. Giao diện Lịch sử và theo dõi đơn hàng

**Tác nhân sử dụng:** Customer  
**Mục đích:** Xem các đơn đã đặt, chi tiết và tiến trình xử lý.

> **Vị trí chèn hình:** Hình UI.9. Giao diện Lịch sử và theo dõi đơn hàng.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTimDon | SearchBox | Tìm theo mã đơn | Chuỗi | Rỗng |  |
| 2 | cboTrangThai | ComboBox | Lọc theo trạng thái | Các trạng thái đơn | Tất cả |  |
| 3 | dteTuNgay | DatePicker | Ngày bắt đầu | Ngày | Đầu tháng |  |
| 4 | dteDenNgay | DatePicker | Ngày kết thúc | Ngày | Hôm nay |  |
| 5 | tblDonHang | Table/List | Hiển thị đơn hàng của khách | Danh sách đơn | Rỗng |  |
| 6 | pnlTienTrinh | Timeline | Hiển thị lịch sử trạng thái | Danh sách trạng thái | Theo đơn chọn | Chỉ đọc |
| 7 | btnHuyDon | Button | Yêu cầu hủy đơn | Không áp dụng | Theo trạng thái |  |
| 8 | btnDoiTra | Button | Mở giao diện gửi yêu cầu đổi/trả | Không áp dụng | Theo điều kiện |  |
| 9 | btnDanhGia | Button | Mở giao diện đánh giá sản phẩm | Không áp dụng | Theo điều kiện |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Tìm/lọc | Tải lại danh sách đơn thuộc khách hàng theo điều kiện. |
| 2 | Chọn đơn | Hiển thị chi tiết và timeline trạng thái. |
| 3 | Nhấn Hủy đơn | Kiểm tra trạng thái; nếu hợp lệ thì yêu cầu xác nhận và thực hiện hủy. |
| 4 | Nhấn Đổi/Trả | Mở giao diện tạo yêu cầu cho các dòng hàng đủ điều kiện. |
| 5 | Nhấn Đánh giá | Mở giao diện đánh giá các dòng hàng đủ điều kiện. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Lịch sử và theo dõi đơn hàng** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 10. Giao diện Gửi yêu cầu đổi/trả

**Tác nhân sử dụng:** Customer  
**Mục đích:** Tạo yêu cầu đổi hoặc trả cho sản phẩm thuộc đơn hàng hợp lệ.

> **Vị trí chèn hình:** Hình UI.10. Giao diện Gửi yêu cầu đổi/trả.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | lblMaDon | Label | Hiển thị mã đơn | Chuỗi | Đơn đã chọn | Chỉ đọc |
| 2 | cboDongHang | ComboBox | Chọn dòng sản phẩm | Các dòng đủ điều kiện | Chưa chọn | Bắt buộc |
| 3 | grpLoaiYeuCau | RadioGroup | Chọn Đổi hoặc Trả | Đổi/Trả | Trả | Bắt buộc |
| 4 | spnSoLuong | NumberSpinner | Số lượng yêu cầu | 1 đến số lượng còn đủ điều kiện | 1 |  |
| 5 | cboBienTheDoi | ComboBox | Biến thể muốn đổi sang | Biến thể khả dụng | Chưa chọn | Chỉ hiện khi chọn Đổi |
| 6 | txtLyDo | TextArea | Nhập lý do và mô tả | Chuỗi 10–1000 ký tự | Rỗng | Bắt buộc |
| 7 | uplHinhAnh | FileUpload | Tải ảnh bằng chứng | JPG/PNG, tối đa 5 MB/ảnh | Rỗng | Theo chính sách |
| 8 | btnGuiYeuCau | Button | Gửi yêu cầu | Không áp dụng | Khả dụng |  |
| 9 | btnHuy | Button | Hủy nhập và quay lại | Không áp dụng | Khả dụng |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Chọn loại Đổi | Hiển thị trường chọn biến thể thay thế. |
| 2 | Tải hình ảnh | Kiểm tra định dạng, kích thước và hiển thị ảnh xem trước. |
| 3 | Nhấn Gửi yêu cầu | Kiểm tra quyền sở hữu, trạng thái, thời hạn, số lượng và yêu cầu trùng; nếu hợp lệ thì tạo trạng thái Chờ duyệt. |
| 4 | Nhấn Hủy | Quay lại chi tiết đơn mà không lưu. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Gửi yêu cầu đổi/trả** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 11. Giao diện Đánh giá sản phẩm

**Tác nhân sử dụng:** Customer  
**Mục đích:** Gửi điểm sao và nhận xét cho sản phẩm đã mua.

> **Vị trí chèn hình:** Hình UI.11. Giao diện Đánh giá sản phẩm.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | lblSanPham | ProductSummary | Hiển thị sản phẩm đã mua | Dữ liệu dòng hàng | Dòng đã chọn | Chỉ đọc |
| 2 | rtgSoSao | Rating | Chọn điểm đánh giá | 1–5 sao | Chưa chọn | Bắt buộc |
| 3 | txtNhanXet | TextArea | Nhập nhận xét | 0–1000 ký tự | Rỗng |  |
| 4 | btnGuiDanhGia | Button | Lưu đánh giá | Không áp dụng | Khả dụng |  |
| 5 | btnHuy | Button | Hủy đánh giá | Không áp dụng | Khả dụng |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Chọn số sao | Cập nhật điểm đánh giá trên giao diện. |
| 2 | Nhấn Gửi đánh giá | Kiểm tra quyền, trạng thái đơn, điểm 1–5 và đánh giá trùng; nếu hợp lệ thì lưu. |
| 3 | Đã đánh giá | Không tạo đánh giá thứ hai; hiển thị đánh giá hiện có hoặc chức năng sửa nếu được hỗ trợ. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Đánh giá sản phẩm** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 12. Giao diện Quản lý sản phẩm

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Xem, tìm kiếm, thêm, cập nhật và ẩn sản phẩm.

> **Vị trí chèn hình:** Hình UI.12. Giao diện Quản lý sản phẩm.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTimKiem | SearchBox | Tìm sản phẩm theo tên/mã | Chuỗi | Rỗng |  |
| 2 | cboTrangThai | ComboBox | Lọc theo trạng thái | Đang bán/Đã ẩn/Tất cả | Tất cả |  |
| 3 | tblSanPham | Table | Danh sách sản phẩm | Danh sách | Rỗng |  |
| 4 | btnThem | Button | Mở form thêm sản phẩm | Không áp dụng | Khả dụng |  |
| 5 | btnSua | Button | Mở form cập nhật | Không áp dụng | Theo dòng chọn |  |
| 6 | btnAn | Button | Ẩn/ngừng kinh doanh sản phẩm | Không áp dụng | Theo dòng chọn | Yêu cầu xác nhận |
| 7 | btnBienThe | Button | Mở quản lý biến thể | Không áp dụng | Theo dòng chọn |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Tìm/lọc | Tải lại danh sách sản phẩm. |
| 2 | Nhấn Thêm | Mở form nhập sản phẩm mới. |
| 3 | Nhấn Sửa | Hiển thị dữ liệu hiện tại để cập nhật. |
| 4 | Nhấn Ẩn | Xác nhận rồi chuyển trạng thái; không xóa cứng sản phẩm có giao dịch. |
| 5 | Nhấn Biến thể | Mở giao diện Quản lý biến thể của sản phẩm đã chọn. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Quản lý sản phẩm** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 13. Giao diện Thêm/Cập nhật sản phẩm

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Nhập và kiểm tra thông tin sản phẩm trước khi lưu.

> **Vị trí chèn hình:** Hình UI.13. Giao diện Thêm/Cập nhật sản phẩm.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtMaSanPham | TextBox | Mã sản phẩm | Chuỗi duy nhất | Tự sinh/Rỗng | Bắt buộc |
| 2 | txtTenSanPham | TextBox | Tên sản phẩm | Chuỗi 2–200 ký tự | Rỗng | Bắt buộc |
| 3 | cboDanhMuc | ComboBox | Chọn danh mục | Danh mục hoạt động | Chưa chọn | Bắt buộc |
| 4 | cboThuongHieu | ComboBox | Chọn thương hiệu | Thương hiệu hoạt động | Chưa chọn | Bắt buộc |
| 5 | numGia | NumberInput | Giá bán | Số > 0 | 0 | Bắt buộc |
| 6 | txtMoTa | RichTextArea | Mô tả sản phẩm | Chuỗi | Rỗng |  |
| 7 | uplHinhAnh | FileUpload | Tải hình sản phẩm | JPG/PNG | Rỗng |  |
| 8 | btnLuu | Button | Lưu sản phẩm | Không áp dụng | Khả dụng |  |
| 9 | btnHuy | Button | Hủy thao tác | Không áp dụng | Khả dụng |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Lưu | Kiểm tra bắt buộc, mã duy nhất, giá > 0 và tệp ảnh; sau đó tạo/cập nhật sản phẩm. |
| 2 | Nhấn Hủy | Quay lại danh sách mà không lưu. |
| 3 | Dữ liệu không hợp lệ | Giữ dữ liệu đã nhập và đánh dấu trường lỗi. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Thêm/Cập nhật sản phẩm** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 14. Giao diện Quản lý biến thể

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Xem, thêm, cập nhật và ẩn biến thể theo kích cỡ và màu sắc.

> **Vị trí chèn hình:** Hình UI.14. Giao diện Quản lý biến thể.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | lblSanPham | Label | Hiển thị sản phẩm gốc | Dữ liệu sản phẩm | Sản phẩm đã chọn | Chỉ đọc |
| 2 | tblBienThe | Table | Danh sách biến thể | Danh sách | Rỗng |  |
| 3 | cboKichCo | ComboBox | Chọn kích cỡ | Danh sách kích cỡ | Chưa chọn |  |
| 4 | cboMauSac | ComboBox | Chọn màu sắc | Danh sách màu | Chưa chọn |  |
| 5 | btnThem | Button | Thêm biến thể | Không áp dụng | Khả dụng |  |
| 6 | btnSua | Button | Cập nhật biến thể | Không áp dụng | Theo dòng chọn |  |
| 7 | btnAn | Button | Ẩn biến thể | Không áp dụng | Theo dòng chọn |  |
| 8 | lnkTonKho | Link/Button | Mở quản lý tồn kho của biến thể | Không áp dụng | Theo dòng chọn | Không chỉnh tồn trực tiếp tại màn hình này |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Thêm/Sửa | Kiểm tra tổ hợp sản phẩm + kích cỡ + màu sắc không trùng rồi lưu. |
| 2 | Nhấn Ẩn | Chuyển trạng thái ngừng kinh doanh; không xóa cứng biến thể có giao dịch. |
| 3 | Nhấn Quản lý tồn | Chuyển sang giao diện tồn kho; không sửa số lượng tại giao diện biến thể. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Quản lý biến thể** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 15. Giao diện Quản lý danh mục và thương hiệu

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Thêm, cập nhật và ẩn/ngừng sử dụng danh mục hoặc thương hiệu.

> **Vị trí chèn hình:** Hình UI.15. Giao diện Quản lý danh mục và thương hiệu.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | tabDanhMucThuongHieu | Tabs | Chọn dữ liệu cần quản lý | Danh mục/Thương hiệu | Danh mục |  |
| 2 | txtTimKiem | SearchBox | Tìm kiếm theo tên/mã | Chuỗi | Rỗng |  |
| 3 | tblDuLieu | Table | Hiển thị danh sách hiện tại | Danh sách | Rỗng |  |
| 4 | txtTen | TextBox | Nhập tên danh mục/thương hiệu | Chuỗi duy nhất | Rỗng | Bắt buộc |
| 5 | btnThem | Button | Thêm mới | Không áp dụng | Khả dụng |  |
| 6 | btnCapNhat | Button | Cập nhật | Không áp dụng | Theo dòng chọn |  |
| 7 | btnAn | Button | Ẩn/ngừng sử dụng | Không áp dụng | Theo dòng chọn | Không xóa cứng dữ liệu đã liên kết |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Chuyển tab | Tải danh sách tương ứng. |
| 2 | Nhấn Thêm/Cập nhật | Kiểm tra tên/mã trùng và lưu dữ liệu. |
| 3 | Nhấn Ẩn | Yêu cầu xác nhận; chuyển trạng thái và ngăn sử dụng cho sản phẩm mới. |
| 4 | Dữ liệu đã liên kết | Không cho xóa vật lý. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Quản lý danh mục và thương hiệu** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 16. Giao diện Quản lý tồn kho

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Xem tồn theo biến thể, nhập kho và điều chỉnh tồn.

> **Vị trí chèn hình:** Hình UI.16. Giao diện Quản lý tồn kho.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTimKiem | SearchBox | Tìm theo sản phẩm/biến thể | Chuỗi | Rỗng |  |
| 2 | cboTrangThaiTon | ComboBox | Lọc theo tình trạng tồn | Còn hàng/Sắp hết/Hết hàng | Tất cả |  |
| 3 | tblTonKho | Table | Hiển thị tồn vật lý, phân bổ và khả dụng | Danh sách tồn | Rỗng |  |
| 4 | btnNhapKho | Button | Mở phiếu nhập kho | Không áp dụng | Theo biến thể chọn |  |
| 5 | btnDieuChinh | Button | Mở phiếu điều chỉnh | Không áp dụng | Theo biến thể chọn |  |
| 6 | numSoLuong | NumberInput | Số lượng nhập/kiểm đếm | Số nguyên >= 0 | 0 | Theo loại thao tác |
| 7 | txtChungTu | TextBox | Mã chứng từ tham chiếu | Chuỗi | Rỗng | Bắt buộc khi nhập |
| 8 | txtLyDo | TextArea | Lý do điều chỉnh | Chuỗi | Rỗng | Bắt buộc khi điều chỉnh |
| 9 | btnXacNhan | Button | Ghi nhận biến động kho | Không áp dụng | Khả dụng |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Tìm/lọc | Tải lại bảng tồn kho. |
| 2 | Nhấn Nhập kho | Mở biểu mẫu nhập, yêu cầu số lượng dương và chứng từ không trùng. |
| 3 | Nhấn Điều chỉnh | Mở biểu mẫu kiểm đếm/lý do; không cho làm tồn khả dụng âm. |
| 4 | Nhấn Xác nhận | Tạo phiếu, cập nhật tồn và ghi lịch sử biến động. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Quản lý tồn kho** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 17. Giao diện Quản lý đơn hàng

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Xem, xác nhận, cập nhật trạng thái và hủy đơn theo quy trình.

> **Vị trí chèn hình:** Hình UI.17. Giao diện Quản lý đơn hàng.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTimDon | SearchBox | Tìm theo mã đơn/khách hàng | Chuỗi | Rỗng |  |
| 2 | cboTrangThai | ComboBox | Lọc theo trạng thái | Các trạng thái đơn | Tất cả |  |
| 3 | dteTuNgay | DatePicker | Ngày bắt đầu | Ngày | Đầu tháng |  |
| 4 | dteDenNgay | DatePicker | Ngày kết thúc | Ngày | Hôm nay |  |
| 5 | tblDonHang | Table | Danh sách đơn | Danh sách | Rỗng |  |
| 6 | pnlChiTiet | DetailPanel | Chi tiết đơn đã chọn | Thông tin đơn | Rỗng | Chỉ đọc |
| 7 | btnXacNhan | Button | Xác nhận đơn chờ duyệt | Không áp dụng | Theo trạng thái |  |
| 8 | cboTrangThaiMoi | ComboBox | Chọn trạng thái tiếp theo | Trạng thái hợp lệ | Chưa chọn |  |
| 9 | btnCapNhatTrangThai | Button | Lưu trạng thái mới | Không áp dụng | Theo trạng thái |  |
| 10 | btnHuyDon | Button | Hủy đơn phía cửa hàng | Không áp dụng | Theo trạng thái | Yêu cầu lý do |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Tìm/lọc | Tải lại danh sách đơn. |
| 2 | Chọn đơn | Hiển thị chi tiết và lịch sử trạng thái. |
| 3 | Nhấn Xác nhận | Kiểm tra đơn còn Chờ xác nhận rồi chuyển sang Đã xác nhận. |
| 4 | Nhấn Cập nhật trạng thái | Kiểm tra phép chuyển trạng thái, cập nhật và ghi lịch sử. |
| 5 | Nhấn Hủy | Yêu cầu lý do, cập nhật Đã hủy, giải phóng tồn và tạo yêu cầu hoàn tiền nếu cần. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Quản lý đơn hàng** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 18. Giao diện Xử lý yêu cầu đổi/trả

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Tiếp nhận, phê duyệt/từ chối, nhận hàng hoàn và xử lý hoàn tiền.

> **Vị trí chèn hình:** Hình UI.18. Giao diện Xử lý yêu cầu đổi/trả.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | cboTrangThaiYeuCau | ComboBox | Lọc trạng thái yêu cầu | Chờ duyệt/Đã duyệt/Từ chối/Hoàn tất | Chờ duyệt |  |
| 2 | tblYeuCau | Table | Danh sách yêu cầu | Danh sách | Rỗng |  |
| 3 | pnlChiTiet | DetailPanel | Đơn, dòng hàng, lý do, bằng chứng | Thông tin yêu cầu | Rỗng | Chỉ đọc |
| 4 | txtLyDoXuLy | TextArea | Nhập lý do duyệt/từ chối | Chuỗi | Rỗng | Bắt buộc |
| 5 | btnPheDuyet | Button | Phê duyệt yêu cầu | Không áp dụng | Theo trạng thái |  |
| 6 | btnTuChoi | Button | Từ chối yêu cầu | Không áp dụng | Theo trạng thái |  |
| 7 | btnNhanHangHoan | Button | Ghi nhận hàng hoàn | Không áp dụng | Sau phê duyệt |  |
| 8 | chkBanLaiDuoc | CheckBox | Xác định hàng có thể nhập lại kho | Có/Không | Không chọn |  |
| 9 | numTienHoan | NumberInput | Số tiền hoàn | 0 đến số đã thanh toán còn được hoàn | 0 |  |
| 10 | btnHoanTien | Button | Tạo/ghi nhận hoàn tiền | Không áp dụng | Theo điều kiện |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Chọn yêu cầu | Hiển thị đầy đủ thông tin và lịch sử. |
| 2 | Nhấn Phê duyệt/Từ chối | Kiểm tra điều kiện, yêu cầu lý do và cập nhật trạng thái. |
| 3 | Nhấn Nhận hàng hoàn | Ghi số lượng/tình trạng; nếu bán lại được thì tạo nghiệp vụ nhập lại kho. |
| 4 | Nhấn Hoàn tiền | Không cho hoàn vượt số đã thanh toán hoặc hoàn lặp; ghi trạng thái giao dịch. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Xử lý yêu cầu đổi/trả** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 19. Giao diện Quản lý khuyến mãi

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Tạo, sửa, kích hoạt và kết thúc chương trình khuyến mãi.

> **Vị trí chèn hình:** Hình UI.19. Giao diện Quản lý khuyến mãi.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTimKiem | SearchBox | Tìm chương trình | Chuỗi | Rỗng |  |
| 2 | cboTrangThai | ComboBox | Lọc trạng thái | Nháp/Đang áp dụng/Kết thúc | Tất cả |  |
| 3 | tblKhuyenMai | Table | Danh sách chương trình | Danh sách | Rỗng |  |
| 4 | btnTao | Button | Tạo chương trình | Không áp dụng | Khả dụng |  |
| 5 | btnSua | Button | Sửa chương trình | Không áp dụng | Theo trạng thái |  |
| 6 | btnKichHoat | Button | Kích hoạt | Không áp dụng | Theo trạng thái |  |
| 7 | btnKetThuc | Button | Kết thúc | Không áp dụng | Theo trạng thái |  |
| 8 | frmKhuyenMai | Form | Thông tin/điều kiện/phạm vi/thời gian | Dữ liệu khuyến mãi | Rỗng | Được kiểm tra trước khi lưu |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Tạo/Sửa | Mở form, kiểm tra giá trị giảm, thời gian, điều kiện và phạm vi rồi lưu. |
| 2 | Nhấn Kích hoạt | Kiểm tra chương trình còn hiệu lực và đầy đủ dữ liệu; chuyển sang Đang áp dụng. |
| 3 | Nhấn Kết thúc | Xác nhận và ngừng áp dụng cho đơn mới. |
| 4 | Sửa chương trình đang chạy | Chỉ áp dụng dữ liệu mới cho giao dịch mới; đơn cũ giữ snapshot đã ghi. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Quản lý khuyến mãi** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 20. Giao diện Thống kê và báo cáo

**Tác nhân sử dụng:** Shop Manager  
**Mục đích:** Tổng hợp doanh thu, đơn hàng, sản phẩm bán chạy và xuất báo cáo.

> **Vị trí chèn hình:** Hình UI.20. Giao diện Thống kê và báo cáo.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | dteTuNgay | DatePicker | Ngày bắt đầu | Ngày | Đầu tháng |  |
| 2 | dteDenNgay | DatePicker | Ngày kết thúc | Ngày | Hôm nay |  |
| 3 | cboLoaiBaoCao | ComboBox | Chọn loại báo cáo | Doanh thu/Đơn hàng/Sản phẩm bán chạy | Doanh thu |  |
| 4 | cboTieuChi | ComboBox | Chọn tiêu chí xếp hạng | Số lượng/Giá trị thực thu | Số lượng | Dùng cho sản phẩm bán chạy |
| 5 | btnThongKe | Button | Tổng hợp dữ liệu | Không áp dụng | Khả dụng |  |
| 6 | pnlBieuDo | ChartPanel | Hiển thị biểu đồ | Dữ liệu báo cáo | Rỗng |  |
| 7 | tblKetQua | Table | Hiển thị số liệu chi tiết | Danh sách | Rỗng |  |
| 8 | btnXuatBaoCao | Button | Xuất báo cáo | XLSX/PDF tùy phạm vi | Khả dụng |  |
| 9 | btnDatLai | Button | Xóa điều kiện và tải lại | Không áp dụng | Khả dụng |  |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Nhấn Thống kê | Kiểm tra khoảng thời gian, tổng hợp dữ liệu hợp lệ và hiển thị bảng/biểu đồ. |
| 2 | Chọn Sản phẩm bán chạy | Loại trừ đơn hủy, điều chỉnh theo hàng trả/hoàn tiền đã xác nhận và xếp hạng. |
| 3 | Nhấn Xuất báo cáo | Tạo tệp theo điều kiện hiện tại; không tạo dữ liệu giả nếu kết quả rỗng. |
| 4 | Khoảng thời gian sai | Hiển thị lỗi và không thực hiện tổng hợp. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Thống kê và báo cáo** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


## 21. Giao diện Quản lý tài khoản và phân quyền

**Tác nhân sử dụng:** System Admin  
**Mục đích:** Tạo/sửa tài khoản, khóa/mở khóa và gán/thu hồi vai trò.

> **Vị trí chèn hình:** Hình UI.21. Giao diện Quản lý tài khoản và phân quyền.

### Danh sách các thành phần của giao diện

| STT | Tên | Kiểu | Ý nghĩa | Miền giá trị | Giá trị mặc định | Ghi chú |
|---|---|---|---|---|---|---|
| 1 | txtTimKiem | SearchBox | Tìm tài khoản | Tên đăng nhập/email/số điện thoại | Rỗng |  |
| 2 | cboVaiTro | ComboBox | Lọc/chọn vai trò | Customer/Shop Manager/System Admin | Tất cả |  |
| 3 | cboTrangThai | ComboBox | Lọc/chọn trạng thái | Hoạt động/Khóa/Vô hiệu hóa | Tất cả |  |
| 4 | tblTaiKhoan | Table | Danh sách tài khoản | Danh sách | Rỗng |  |
| 5 | btnThem | Button | Tạo tài khoản | Không áp dụng | Khả dụng |  |
| 6 | btnSua | Button | Sửa thông tin được phép | Không áp dụng | Theo dòng chọn |  |
| 7 | btnKhoaMo | Button | Khóa hoặc mở khóa | Không áp dụng | Theo dòng chọn |  |
| 8 | btnPhanQuyen | Button | Gán/thu hồi vai trò | Không áp dụng | Theo dòng chọn |  |
| 9 | btnXemNhatKy | Button | Mở nhật ký quản trị | Không áp dụng | Theo quyền | Nếu thuộc phạm vi |

### Danh sách các biến cố

| STT | Điều kiện kích hoạt | Xử lý |
|---|---|---|
| 1 | Tìm/lọc | Tải lại danh sách tài khoản. |
| 2 | Nhấn Thêm/Sửa | Kiểm tra định danh duy nhất và lưu dữ liệu. |
| 3 | Nhấn Khóa/Mở khóa | Yêu cầu xác nhận; không cho vô hiệu hóa quản trị viên hoạt động cuối cùng. |
| 4 | Nhấn Phân quyền | Kiểm tra phạm vi quyền, cập nhật vai trò và ghi nhật ký. |
| 5 | Mọi thao tác quản trị | Ghi người thực hiện, thời điểm, đối tượng và kết quả; không ghi mật khẩu/token rõ. |

### Liên kết Use Case

Giao diện này hiện thực hóa các thao tác thuộc chức năng **Quản lý tài khoản và phân quyền** và phải được đối chiếu với Use Case, đặc tả Use Case và Sequence Diagram tương ứng trong báo cáo.


---

# Checklist hoàn thiện phần mô tả giao diện

- [ ] Chèn wireframe/mockup cho từng giao diện vào đúng vị trí đã đánh dấu.
- [ ] Đánh số hình liên tục và cập nhật Danh sách hình ảnh trong Word.
- [ ] Đảm bảo mọi Use Case đã đặc tả đều có ít nhất một giao diện hoặc được bao phủ rõ bởi một giao diện tổng hợp.
- [ ] Đối chiếu tên nút, trường nhập và sự kiện với luồng chính/ngoại lệ trong đặc tả Use Case.
- [ ] Bổ sung trạng thái lỗi, trạng thái rỗng, trạng thái đang tải và hộp xác nhận cho thao tác quan trọng.
- [ ] Không mô tả control không xuất hiện trong hình; không để hình có control nhưng bảng không mô tả.
- [ ] Chuẩn hóa thuật ngữ: Ẩn/ngừng sử dụng thay cho xóa cứng danh mục, thương hiệu, sản phẩm và biến thể đã phát sinh dữ liệu.
- [ ] Kiểm tra phân quyền hiển thị giữa Customer, Shop Manager và System Admin.
- [ ] Cập nhật mục lục và số trang sau khi chèn phần giao diện vào báo cáo.