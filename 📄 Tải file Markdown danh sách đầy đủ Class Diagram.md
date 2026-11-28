# Danh sách đầy đủ Class Diagram

> Nguồn: Phần 3.4 Class Diagram trong báo cáo `S2_Nhom19_Lan4.docx`.
> Phạm vi: 37 lớp, toàn bộ thuộc tính và phương thức được mô tả trong báo cáo.

## Mục lục

1. [VaiTro](#vaitro)
2. [TaiKhoan](#taikhoan)
3. [TaiKhoanVaiTro](#taikhoanvaitro)
4. [PhienDangNhap](#phiendangnhap)
5. [DatLaiMatKhau](#datlaimatkhau)
6. [NhatKyHeThong](#nhatkyhethong)
7. [DanhMuc](#danhmuc)
8. [ThuongHieu](#thuonghieu)
9. [SanPham](#sanpham)
10. [KichCo](#kichco)
11. [MauSac](#mausac)
12. [BienTheSanPham](#bienthesanpham)
13. [HinhAnhSanPham](#hinhanhsanpham)
14. [TonKho](#tonkho)
15. [PhieuKho](#phieukho)
16. [ChiTietPhieuKho](#chitietphieukho)
17. [PhanBoTonKho](#phanbotonkho)
18. [BienDongKho](#biendongkho)
19. [GioHang](#giohang)
20. [ChiTietGioHang](#chitietgiohang)
21. [PhuongThucThanhToan](#phuongthucthanhtoan)
22. [DonHang](#donhang)
23. [ChiTietDonHang](#chitietdonhang)
24. [LichSuDonHang](#lichsudonhang)
25. [ThanhToan](#thanhtoan)
26. [KhuyenMai](#khuyenmai)
27. [KhuyenMaiSanPham](#khuyenmaisanpham)
28. [KhuyenMaiDanhMuc](#khuyenmaidanhmuc)
29. [KhuyenMaiThuongHieu](#khuyenmaithuonghieu)
30. [KhuyenMaiDonHang](#khuyenmaidonhang)
31. [LichSuKhuyenMai](#lichsukhuyenmai)
32. [YeuCauDoiTra](#yeucaudoitra)
33. [ChiTietDoiTra](#chitietdoitra)
34. [HinhAnhDoiTra](#hinhanhdoitra)
35. [LichSuDoiTra](#lichsudoitra)
36. [HoanTien](#hoantien)
37. [DanhGia](#danhgia)

---

## 1. VaiTro

**Chức năng:** Lưu các vai trò được sử dụng để phân quyền cho người dùng.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | VaiTroID | int | Mã định danh vai trò (PK) |
| 2 | MaVaiTro | string | Mã vai trò duy nhất: CUSTOMER, SHOP_MANAGER, SYSTEM_ADMIN (UK) |
| 3 | TenVaiTro | string | Tên hiển thị của vai trò |
| 4 | MoTa | string | Mô tả quyền hạn và trách nhiệm của vai trò |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | kiemTraQuyen() | bool | Kiểm tra quyền tương ứng với vai trò |

---

## 2. TaiKhoan

**Chức năng:** Lưu thông tin đăng nhập, hồ sơ cá nhân và trạng thái bảo mật của tài khoản.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | TaiKhoanID | long | Mã định danh tài khoản (PK) |
| 2 | TenDangNhap | string | Tên đăng nhập duy nhất (UK) |
| 3 | SoDienThoai | string | Số điện thoại |
| 4 | Email | string | Email đăng ký |
| 5 | MatKhauHash | string | Mật khẩu đã được băm |
| 6 | HoTen | string | Họ và tên người dùng |
| 7 | NgaySinh | date | Ngày sinh |
| 8 | GioiTinh | string | Giới tính |
| 9 | DiaChi | string | Địa chỉ |
| 10 | AnhDaiDien | string | Đường dẫn ảnh đại diện |
| 11 | TrangThai | string | Trạng thái tài khoản (Hoạt động / Khóa) |
| 12 | EmailDaXacMinh | bool | Email đã xác minh hay chưa |
| 13 | SoLanDangNhapSai | int | Số lần đăng nhập sai liên tiếp |
| 14 | KhoaDenLuc | DateTime | Thời điểm khóa tài khoản đến |
| 15 | LanDangNhapCuoi | DateTime | Lần đăng nhập gần nhất |
| 16 | NgayDoiMatKhau | DateTime | Thời điểm đổi mật khẩu gần nhất |
| 17 | NgayTao | DateTime | Thời điểm tạo tài khoản |
| 18 | NgayCapNhat | DateTime | Thời điểm cập nhật gần nhất |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | dangNhap() | bool | Xác thực tài khoản |
| 2 | doiMatKhau() | bool | Thay đổi mật khẩu |
| 3 | khoaTaiKhoan() | void | Khóa tài khoản |
| 4 | moKhoaTaiKhoan() | void | Mở khóa tài khoản |

---

## 3. TaiKhoanVaiTro

**Chức năng:** Bảng liên kết tài khoản với vai trò, đồng thời ghi nhận người và thời điểm gán.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | TaiKhoanID | long | Tài khoản được gán vai trò (PK, FK) |
| 2 | VaiTroID | int | Vai trò được gán (PK, FK) |
| 3 | NguoiGanID | long | Tài khoản thực hiện gán (FK) |
| 4 | NgayGan | DateTime | Thời điểm gán vai trò |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 4. PhienDangNhap

**Chức năng:** Quản lý phiên xác thực, thời hạn token và thời điểm thu hồi phiên.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | PhienID | Guid | Mã phiên đăng nhập (PK) |
| 2 | TaiKhoanID | long | Tài khoản sở hữu phiên (FK) |
| 3 | TokenHash | byte[] | Giá trị băm token (UK) |
| 4 | NgayTao | DateTime | Thời điểm tạo phiên |
| 5 | HetHanLuc | DateTime | Thời điểm hết hạn |
| 6 | ThuHoiLuc | DateTime | Thời điểm thu hồi phiên |
| 7 | DiaChiIP | string | Địa chỉ IP |
| 8 | UserAgent | string | Thông tin trình duyệt hoặc thiết bị |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | conHieuLuc() | bool | Kiểm tra phiên còn hiệu lực |
| 2 | thuHoi() | void | Thu hồi phiên đăng nhập |

---

## 5. DatLaiMatKhau

**Chức năng:** Quản lý yêu cầu khôi phục mật khẩu bằng token giới hạn thời gian và số lần thử.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | YeuCauID | long | Mã yêu cầu đặt lại mật khẩu (PK) |
| 2 | TaiKhoanID | long | Tài khoản yêu cầu khôi phục (FK) |
| 3 | TokenHash | byte[] | Token đã băm (UK) |
| 4 | NgayTao | DateTime | Thời điểm tạo yêu cầu |
| 5 | HetHanLuc | DateTime | Thời điểm token hết hạn |
| 6 | DaSuDungLuc | DateTime | Thời điểm token được sử dụng |
| 7 | SoLanThu | int | Số lượt thử sử dụng token |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | conHieuLuc() | bool | Kiểm tra token còn hiệu lực (chưa hết hạn, chưa dùng, chưa vượt số lần thử) |
| 2 | danhDauDaSuDung() | void | Đánh dấu token đã được sử dụng |

---

## 6. NhatKyHeThong

**Chức năng:** Lưu nhật ký thao tác, kết quả thực hiện và thông tin truy vết của hệ thống.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | NhatKyID | long | Mã nhật ký hệ thống (PK) |
| 2 | NguoiThucHienID | long | Tài khoản thực hiện thao tác, có thể trống nếu chưa đăng nhập (FK) |
| 3 | HanhDong | string | Tên hành động được ghi nhận |
| 4 | DoiTuong | string | Loại đối tượng bị tác động |
| 5 | MaDoiTuong | string | Mã đối tượng cụ thể bị tác động |
| 6 | NoiDung | string | Thông tin diễn giải hành động, không chứa mật khẩu/token |
| 7 | KetQua | string | Kết quả: THANH_CONG hoặc THAT_BAI |
| 8 | DiaChiIP | string | Địa chỉ IP phát sinh thao tác |
| 9 | MaYeuCau | Guid | Mã truy vết yêu cầu xử lý |
| 10 | ThoiDiem | DateTime | Thời điểm ghi nhật ký |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 7. DanhMuc

**Chức năng:** Lưu danh sách danh mục dùng để phân loại sản phẩm giày.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | DanhMucID | int | Mã danh mục (PK) |
| 2 | TenDanhMuc | string | Tên danh mục duy nhất (UK) |
| 3 | MoTa | string | Mô tả danh mục |
| 4 | HinhAnh | string | Đường dẫn ảnh |
| 5 | DangHoatDong | bool | Trạng thái sử dụng |
| 6 | NgayTao | DateTime | Ngày tạo |
| 7 | NgayCapNhat | DateTime | Ngày cập nhật |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | anDanhMuc() | void | Ẩn danh mục |
| 2 | kichHoat() | void | Kích hoạt danh mục |

---

## 8. ThuongHieu

**Chức năng:** Lưu danh sách thương hiệu của các sản phẩm giày.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | ThuongHieuID | int | Mã thương hiệu (PK) |
| 2 | TenThuongHieu | string | Tên thương hiệu duy nhất (UK) |
| 3 | MoTa | string | Mô tả thương hiệu |
| 4 | HinhAnh | string | Đường dẫn ảnh thương hiệu |
| 5 | DangHoatDong | bool | Cờ trạng thái hoạt động của thương hiệu |
| 6 | NgayTao | DateTime | Ngày giờ tạo thương hiệu |
| 7 | NgayCapNhat | DateTime | Ngày giờ cập nhật thương hiệu |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | anThuongHieu() | void | Ẩn thương hiệu |
| 2 | kichHoat() | void | Kích hoạt thương hiệu |

---

## 9. SanPham

**Chức năng:** Lưu thông tin sản phẩm, giá bán cơ sở, danh mục, thương hiệu và trạng thái.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | SanPhamID | long | Mã sản phẩm (PK) |
| 2 | MaSanPham | string | Mã sản phẩm duy nhất (UK) |
| 3 | TenSanPham | string | Tên sản phẩm |
| 4 | MoTa | string | Mô tả chi tiết |
| 5 | GiaBan | decimal | Giá bán cơ sở |
| 6 | DanhMucID | int | Danh mục (FK) |
| 7 | ThuongHieuID | int | Thương hiệu (FK) |
| 8 | TrangThai | string | Trạng thái sản phẩm: NHAP, DANG_BAN, AN, NGUNG_BAN |
| 9 | NguoiTaoID | long | Tài khoản tạo sản phẩm (FK) |
| 10 | NgayTao | DateTime | Thời điểm tạo |
| 11 | NgayCapNhat | DateTime | Thời điểm cập nhật |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | capNhatGia() | void | Cập nhật giá bán |
| 2 | anSanPham() | void | Ẩn sản phẩm |
| 3 | ngungKinhDoanh() | void | Ngừng kinh doanh sản phẩm |
| 4 | dangBan() | bool | Kiểm tra sản phẩm đang ở trạng thái kinh doanh |

---

## 10. KichCo

**Chức năng:** Lưu các cỡ giày theo hệ kích cỡ và thứ tự hiển thị.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | KichCoID | int | Mã kích cỡ (PK) |
| 2 | HeSize | string | Hệ cỡ EU, US hoặc UK |
| 3 | GiaTri | string | Giá trị kích cỡ |
| 4 | ThuTu | int | Thứ tự hiển thị |
| 5 | DangHoatDong | bool | Trạng thái sử dụng |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 11. MauSac

**Chức năng:** Lưu danh sách màu sắc và mã màu của sản phẩm.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | MauSacID | int | Mã màu sắc (PK) |
| 2 | TenMau | string | Tên màu duy nhất (UK) |
| 3 | MaHex | string | Mã màu dạng #RRGGBB |
| 4 | DangHoatDong | bool | Trạng thái sử dụng |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 12. BienTheSanPham

**Chức năng:** Lưu từng biến thể của sản phẩm theo tổ hợp kích cỡ và màu sắc, kèm SKU.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | BienTheID | long | Mã biến thể (PK) |
| 2 | SanPhamID | long | Sản phẩm cha (FK) |
| 3 | SKU | string | Mã quản lý tồn kho duy nhất (UK) |
| 4 | KichCoID | int | Kích cỡ (FK) |
| 5 | MauSacID | int | Màu sắc (FK) |
| 6 | GiaBanRieng | decimal | Giá riêng của biến thể |
| 7 | DangKinhDoanh | bool | Trạng thái kinh doanh |
| 8 | NgayTao | DateTime | Thời điểm tạo |
| 9 | NgayCapNhat | DateTime | Thời điểm cập nhật |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | layGiaHienTai() | decimal | Lấy giá bán hiện tại (giá riêng nếu có, ngược lại lấy giá sản phẩm) |
| 2 | anBienThe() | void | Ẩn biến thể khỏi kinh doanh |
| 3 | kiemTraConHang() | bool | Kiểm tra biến thể còn hàng khả dụng |

---

## 13. HinhAnhSanPham

**Chức năng:** Lưu hình ảnh chung của sản phẩm hoặc hình ảnh riêng của biến thể.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | HinhAnhID | long | Mã hình ảnh (PK) |
| 2 | SanPhamID | long | Sản phẩm sở hữu ảnh (FK) |
| 3 | BienTheID | long | Biến thể được minh họa (FK, có thể rỗng) |
| 4 | DuongDan | string | Đường dẫn hoặc URL ảnh |
| 5 | MoTaAnh | string | Mô tả hình ảnh |
| 6 | LaAnhChinh | bool | Đánh dấu ảnh chính |
| 7 | ThuTu | int | Thứ tự hiển thị |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | datLamAnhChinh() | void | Chọn hình làm ảnh chính |

---

## 14. TonKho

**Chức năng:** Lưu tồn kho vật lý, số lượng đã phân bổ và tồn khả dụng cho từng biến thể.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | BienTheID | long | Biến thể giày tương ứng với bản ghi tồn kho (PK, FK) |
| 2 | SoLuongTon | int | Số lượng hàng thực tế đang có trong kho |
| 3 | SoLuongDaPhanBo | int | Số lượng đã giữ/phân bổ cho các đơn hàng |
| 4 | SoLuongKhaDung | int | Số lượng có thể bán, tính bằng SoLuongTon - SoLuongDaPhanBo [thuộc tính tính toán] |
| 5 | MucCanhBao | int | Ngưỡng số lượng tồn thấp để cảnh báo |
| 6 | NgayCapNhat | DateTime | Thời điểm cập nhật dữ liệu tồn kho |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | tangTon() | void | Tăng số lượng tồn kho |
| 2 | giamTon() | void | Giảm số lượng tồn kho |
| 3 | phanBo() | void | Phân bổ (giữ) số lượng tồn cho đơn hàng |
| 4 | giaiPhong() | void | Giải phóng số lượng đã phân bổ |
| 5 | conDuHang() | bool | Kiểm tra tồn khả dụng có đủ số lượng yêu cầu hay không |

---

## 15. PhieuKho

**Chức năng:** Lưu chứng từ nhập kho, điều chỉnh kho hoặc nhận hàng trả.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | PhieuKhoID | long | Mã định danh phiếu kho (PK) |
| 2 | MaPhieu | string | Mã nghiệp vụ của phiếu kho, không trùng (UK) |
| 3 | MaChongLap | Guid | Mã chống tạo trùng phiếu kho (UK) |
| 4 | LoaiPhieu | string | Loại phiếu: NHAP, DIEU_CHINH hoặc NHAN_TRA |
| 5 | MaChungTuThamChieu | string | Mã chứng từ liên quan dùng để đối soát |
| 6 | LyDo | string | Diễn giải lý do lập phiếu kho |
| 7 | NguoiThucHienID | long | Tài khoản thực hiện nghiệp vụ kho (FK) |
| 8 | NgayTao | DateTime | Thời điểm tạo phiếu |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | hoanTatPhieu() | void | Hoàn tất phiếu kho và ghi nhận biến động tồn kho |

---

## 16. ChiTietPhieuKho

**Chức năng:** Lưu từng biến thể và số lượng biến động thuộc một phiếu kho.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | ChiTietPhieuKhoID | long | Mã định danh dòng phiếu kho (PK) |
| 2 | PhieuKhoID | long | Phiếu kho chứa dòng phát sinh (FK) |
| 3 | BienTheID | long | Biến thể được nhập/điều chỉnh/nhận trả (FK) |
| 4 | SoLuongThayDoi | int | Số lượng thay đổi (+/-), không được bằng 0 |
| 5 | DonGiaNhap | decimal | Đơn giá nhập hàng nếu áp dụng |
| 6 | ChiTietDoiTraID | long | Dòng đổi/trả liên quan khi nhận hàng trả về (FK) |
| 7 | GhiChu | string | Ghi chú xử lý đối với dòng phiếu kho |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 17. PhanBoTonKho

**Chức năng:** Lưu số lượng tồn kho giữ cho từng dòng hàng trong đơn và trạng thái phân bổ.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | ChiTietDonHangID | long | Dòng đơn hàng được giữ tồn kho (PK, FK) |
| 2 | SoLuongGiu | int | Số lượng giữ riêng cho dòng đơn hàng |
| 3 | TrangThai | string | Trạng thái giữ hàng: DANG_GIU, DA_XUAT, DA_GIAI_PHONG |
| 4 | NgayPhanBo | DateTime | Thời điểm tạo phân bổ kho |
| 5 | HanGiuLuc | DateTime | Hạn cuối giữ hàng, nếu có |
| 6 | NgayKetThuc | DateTime | Thời điểm kết thúc giữ hàng hoặc xuất kho |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | xuatHang() | void | Chuyển trạng thái giữ hàng sang đã xuất kho |
| 2 | giaiPhong() | void | Giải phóng số lượng đang giữ |

---

## 18. BienDongKho

**Chức năng:** Ghi sổ mọi thay đổi tồn vật lý và tồn đã phân bổ để đối soát.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | BienDongID | long | Mã bản ghi biến động kho (PK) |
| 2 | BienTheID | long | Biến thể có số lượng thay đổi (FK) |
| 3 | LoaiBienDong | string | Loại biến động: NHAP, DIEU_CHINH, GIU_HANG, GIAI_PHONG, BAN_HANG, NHAN_TRA |
| 4 | ThayDoiTon | int | Mức thay đổi của số lượng tồn vật lý |
| 5 | ThayDoiPhanBo | int | Mức thay đổi của số lượng đang phân bổ |
| 6 | TonSau | int | Số lượng tồn thực tế sau nghiệp vụ |
| 7 | PhanBoSau | int | Số lượng đã phân bổ sau nghiệp vụ |
| 8 | ChiTietPhieuKhoID | long | Dòng phiếu kho làm phát sinh biến động, nếu có (FK) |
| 9 | ChiTietDonHangID | long | Dòng đơn hàng làm phát sinh biến động, nếu có (FK) |
| 10 | NguoiThucHienID | long | Tài khoản thực hiện nghiệp vụ (FK) |
| 11 | MaThaoTac | Guid | Mã duy nhất của thao tác, chống ghi sổ trùng (UK) |
| 12 | LyDo | string | Nội dung diễn giải biến động |
| 13 | ThoiDiem | DateTime | Thời điểm ghi sổ kho |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 19. GioHang

**Chức năng:** Lưu giỏ hàng của tài khoản hoặc giỏ hàng khách chưa đăng nhập theo phiên.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | GioHangID | long | Mã giỏ hàng (PK) |
| 2 | TaiKhoanID | long | Chủ giỏ hàng khi người dùng đã đăng nhập (FK) |
| 3 | MaPhienHash | byte[] | Mã băm phiên của khách chưa đăng nhập |
| 4 | NgayTao | DateTime | Thời điểm tạo giỏ hàng |
| 5 | NgayCapNhat | DateTime | Thời điểm cập nhật giỏ hàng |
| 6 | HetHanLuc | DateTime | Thời điểm giỏ của khách hết hạn (nếu áp dụng) |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | themSanPham() | void | Thêm biến thể sản phẩm vào giỏ hàng |
| 2 | capNhatSoLuong() | void | Cập nhật số lượng của một mặt hàng trong giỏ |
| 3 | xoaSanPham() | void | Xóa mặt hàng khỏi giỏ hàng |
| 4 | tinhTongTienTam() | decimal | Tính tổng tiền tạm tính của giỏ hàng |
| 5 | lamTrong() | void | Xóa toàn bộ mặt hàng trong giỏ |

---

## 20. ChiTietGioHang

**Chức năng:** Lưu các biến thể giày và số lượng tương ứng trong giỏ hàng.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | GioHangID | long | Giỏ hàng chứa mặt hàng (PK, FK) |
| 2 | BienTheID | long | Biến thể giày được chọn mua (PK, FK) |
| 3 | SoLuong | int | Số lượng sản phẩm của biến thể trong giỏ, phải lớn hơn 0 |
| 4 | NgayThem | DateTime | Thời điểm thêm biến thể vào giỏ |
| 5 | NgayCapNhat | DateTime | Thời điểm cập nhật số lượng trong giỏ |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | capNhatSoLuong() | void | Cập nhật số lượng của biến thể trong giỏ |

---

## 21. PhuongThucThanhToan

**Chức năng:** Lưu các phương thức thanh toán áp dụng cho đơn hàng.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | PhuongThucID | int | Mã phương thức thanh toán (PK) |
| 2 | MaPhuongThuc | string | Mã phương thức duy nhất: COD, BANK_TRANSFER, ONLINE (UK) |
| 3 | TenPhuongThuc | string | Tên phương thức dùng để hiển thị |
| 4 | DangHoatDong | bool | Cờ phương thức đang được hỗ trợ |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 22. DonHang

**Chức năng:** Lưu đơn đặt hàng, địa chỉ nhận, trạng thái và tổng số tiền cần thanh toán.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | DonHangID | long | Mã định danh đơn hàng (PK) |
| 2 | MaDonHang | string | Mã hiển thị đơn hàng duy nhất (UK) |
| 3 | MaYeuCauTao | Guid | Mã chống tạo đơn hàng trùng khi gửi yêu cầu nhiều lần (UK) |
| 4 | KhachHangID | long | Tài khoản khách hàng sở hữu đơn hàng (FK) |
| 5 | HoTenNguoiNhan | string | Họ tên người nhận được lưu tại thời điểm đặt |
| 6 | SoDienThoaiNhan | string | Số điện thoại người nhận |
| 7 | EmailNhan | string | Email liên hệ người nhận, nếu có |
| 8 | DiaChiNhan | string | Địa chỉ nhận hàng |
| 9 | PhuongXa | string | Phường/xã của địa chỉ giao hàng |
| 10 | TinhThanh | string | Tỉnh/thành phố giao hàng |
| 11 | GhiChu | string | Ghi chú giao hàng từ khách |
| 12 | PhuongThucID | int | Phương thức thanh toán khách đã chọn (FK) |
| 13 | TrangThai | string | Trạng thái xử lý đơn từ chờ xác nhận đến hoàn thành hoặc hủy |
| 14 | TienHang | decimal | Tổng giá trị các dòng hàng trước giảm giá |
| 15 | TienGiam | decimal | Tổng số tiền khuyến mãi được trừ |
| 16 | PhiVanChuyen | decimal | Phí vận chuyển của đơn hàng |
| 17 | TongThanhToan | decimal | Tổng phải thanh toán, tính bằng TienHang - TienGiam + PhiVanChuyen [thuộc tính tính toán] |
| 18 | TienTe | string | Đơn vị tiền tệ, mặc định VND |
| 19 | NgayDat | DateTime | Thời điểm khách đặt hàng |
| 20 | HanThanhToan | DateTime | Hạn thanh toán của đơn hàng (nếu có) |
| 21 | NgayGiao | DateTime | Thời điểm đơn được ghi nhận giao hàng |
| 22 | NgayHoanThanh | DateTime | Thời điểm đơn hoàn thành |
| 23 | NgayHuy | DateTime | Thời điểm hủy đơn, bắt buộc khi trạng thái DA_HUY |
| 24 | LyDoHuy | string | Lý do đơn hàng bị hủy |
| 25 | NguoiHuyID | long | Tài khoản thực hiện thao tác hủy đơn (FK) |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | xacNhan() | void | Xác nhận đơn hàng |
| 2 | huyDon() | void | Hủy đơn hàng |
| 3 | capNhatTrangThai() | void | Cập nhật trạng thái xử lý của đơn hàng |
| 4 | tinhTongThanhToan() | decimal | Tính tổng số tiền khách phải thanh toán |
| 5 | choPhepHuy() | bool | Kiểm tra đơn hàng còn ở trạng thái cho phép hủy |

---

## 23. ChiTietDonHang

**Chức năng:** Lưu các dòng hàng và thông tin sản phẩm được chụp tại thời điểm mua.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | ChiTietDonHangID | long | Mã định danh dòng đơn hàng (PK) |
| 2 | DonHangID | long | Đơn hàng chứa dòng sản phẩm (FK) |
| 3 | BienTheID | long | Biến thể giày đã mua (FK) |
| 4 | MaSanPhamLucMua | string | Mã sản phẩm được chụp tại thời điểm mua |
| 5 | SKULucMua | string | SKU biến thể tại thời điểm mua |
| 6 | TenSanPhamLucMua | string | Tên sản phẩm tại thời điểm mua |
| 7 | KichCoLucMua | string | Kích cỡ sản phẩm tại thời điểm mua |
| 8 | MauSacLucMua | string | Màu sắc sản phẩm tại thời điểm mua |
| 9 | DanhMucLucMua | string | Tên danh mục tại thời điểm mua |
| 10 | ThuongHieuLucMua | string | Tên thương hiệu tại thời điểm mua |
| 11 | SoLuong | int | Số lượng biến thể được đặt mua |
| 12 | DonGia | decimal | Đơn giá mỗi sản phẩm tại thời điểm mua |
| 13 | TienGiamPhanBo | decimal | Phần tiền khuyến mãi được phân bổ vào dòng hàng |
| 14 | ThanhTien | decimal | Số tiền dòng hàng sau giảm giá, tính bằng SoLuong × DonGia - TienGiamPhanBo [thuộc tính tính toán] |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | tinhThanhTien() | decimal | Tính thành tiền của dòng hàng sau giảm giá |

---

## 24. LichSuDonHang

**Chức năng:** Ghi lại mỗi lần chuyển trạng thái của đơn hàng.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | LichSuID | long | Mã bản ghi lịch sử trạng thái (PK) |
| 2 | DonHangID | long | Đơn hàng có thay đổi (FK) |
| 3 | TrangThaiCu | string | Trạng thái trước khi thay đổi |
| 4 | TrangThaiMoi | string | Trạng thái sau khi thay đổi |
| 5 | NguoiThucHienID | long | Tài khoản thực hiện chuyển trạng thái (FK) |
| 6 | ThoiDiem | DateTime | Thời điểm phát sinh thay đổi |
| 7 | GhiChu | string | Ghi chú hoặc lý do cho lần chuyển trạng thái |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 25. ThanhToan

**Chức năng:** Lưu từng giao dịch thanh toán, trạng thái xác nhận và chứng từ đối soát.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | ThanhToanID | long | Mã định danh giao dịch thanh toán (PK) |
| 2 | DonHangID | long | Đơn hàng được thanh toán (FK) |
| 3 | PhuongThucID | int | Phương thức sử dụng trong giao dịch (FK) |
| 4 | MaYeuCau | Guid | Mã yêu cầu chống xử lý lại một giao dịch (UK) |
| 5 | CongThanhToan | string | Tên cổng hoặc kênh thanh toán được sử dụng |
| 6 | MaGiaoDich | string | Mã giao dịch do cổng thanh toán cung cấp |
| 7 | SoTien | decimal | Số tiền của giao dịch thanh toán, phải lớn hơn 0 |
| 8 | TrangThai | string | Trạng thái xác nhận, thất bại, hủy hoặc hết hạn |
| 9 | NgayTao | DateTime | Thời điểm tạo giao dịch thanh toán |
| 10 | NgayXacNhan | DateTime | Thời điểm giao dịch đã được xác nhận thu tiền |
| 11 | NguoiXacNhanID | long | Tài khoản xác nhận giao dịch, nếu có (FK) |
| 12 | BangChung | string | Đường dẫn/tài liệu chứng minh thanh toán |
| 13 | ThongBaoLoi | string | Thông báo lỗi khi giao dịch không thành công |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | xacNhanThanhToan() | void | Xác nhận giao dịch đã thu tiền |
| 2 | danhDauThatBai() | void | Đánh dấu giao dịch thanh toán thất bại |

---

## 26. KhuyenMai

**Chức năng:** Lưu chương trình giảm giá, điều kiện áp dụng, phạm vi và thời hạn hiệu lực.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | KhuyenMaiID | long | Mã định danh chương trình khuyến mãi (PK) |
| 2 | MaKhuyenMai | string | Mã chương trình duy nhất (UK) |
| 3 | TenKhuyenMai | string | Tên khuyến mãi |
| 4 | MoTa | string | Mô tả nội dung, điều khoản của chương trình |
| 5 | HinhThuc | string | Hình thức giảm giá: PHAN_TRAM hoặc SO_TIEN |
| 6 | GiaTri | decimal | Mức giảm theo phần trăm hoặc số tiền |
| 7 | GiamToiDa | decimal | Giới hạn số tiền giảm tối đa, nếu thiết lập |
| 8 | GiaTriDonToiThieu | decimal | Giá trị đơn tối thiểu để được áp dụng |
| 9 | PhamVi | string | Phạm vi: TOAN_BO, SAN_PHAM, DANH_MUC, THUONG_HIEU |
| 10 | BatDauLuc | DateTime | Thời điểm bắt đầu hiệu lực |
| 11 | KetThucLuc | DateTime | Thời điểm kết thúc hiệu lực |
| 12 | TrangThai | string | Trạng thái: NHAP, DANG_AP_DUNG hoặc KET_THUC |
| 13 | ChoPhepKetHop | bool | Cờ cho phép dùng kết hợp với khuyến mãi khác |
| 14 | DoUuTien | int | Mức ưu tiên xử lý giữa các khuyến mãi |
| 15 | GioiHanLuotDung | int | Giới hạn lượt sử dụng toàn chương trình |
| 16 | GioiHanMoiKhach | int | Giới hạn lượt sử dụng của mỗi khách hàng |
| 17 | NguoiTaoID | long | Tài khoản tạo chương trình khuyến mãi (FK) |
| 18 | NgayTao | DateTime | Thời điểm tạo chương trình |
| 19 | NgayCapNhat | DateTime | Thời điểm cập nhật chương trình |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | kichHoat() | void | Kích hoạt chương trình khuyến mãi |
| 2 | ketThuc() | void | Kết thúc chương trình khuyến mãi |
| 3 | conHieuLuc() | bool | Kiểm tra chương trình còn trong thời hạn và đang áp dụng |
| 4 | tinhTienGiam() | decimal | Tính số tiền giảm cho một giá trị đơn hàng |

---

## 27. KhuyenMaiSanPham

**Chức năng:** Bảng liên kết khuyến mãi với các sản phẩm thuộc phạm vi áp dụng.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | KhuyenMaiID | long | Chương trình khuyến mãi được gán sản phẩm (PK, FK) |
| 2 | SanPhamID | long | Sản phẩm nằm trong phạm vi khuyến mãi (PK, FK) |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 28. KhuyenMaiDanhMuc

**Chức năng:** Bảng liên kết khuyến mãi với các danh mục thuộc phạm vi áp dụng.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | KhuyenMaiID | long | Chương trình khuyến mãi được gán danh mục (PK, FK) |
| 2 | DanhMucID | int | Danh mục nằm trong phạm vi khuyến mãi (PK, FK) |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 29. KhuyenMaiThuongHieu

**Chức năng:** Bảng liên kết khuyến mãi với các thương hiệu thuộc phạm vi áp dụng.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | KhuyenMaiID | long | Chương trình khuyến mãi được gán thương hiệu (PK, FK) |
| 2 | ThuongHieuID | int | Thương hiệu nằm trong phạm vi khuyến mãi (PK, FK) |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 30. KhuyenMaiDonHang

**Chức năng:** Lưu chương trình đã áp dụng trên đơn hàng và số tiền được giảm thực tế.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | DonHangID | long | Đơn hàng đã áp dụng khuyến mãi (PK, FK) |
| 2 | KhuyenMaiID | long | Chương trình được dùng tại thời điểm đặt (PK, FK) |
| 3 | MaLucApDung | string | Mã khuyến mãi được lưu tại lúc áp dụng |
| 4 | TenLucApDung | string | Tên chương trình tại lúc áp dụng |
| 5 | HinhThucLucApDung | string | Hình thức giảm giá tại lúc áp dụng |
| 6 | GiaTriLucApDung | decimal | Giá trị giảm tại lúc áp dụng |
| 7 | TienGiamThucTe | decimal | Số tiền giảm thực tế đóng góp vào đơn hàng |
| 8 | NgayApDung | DateTime | Thời điểm áp dụng khuyến mãi |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 31. LichSuKhuyenMai

**Chức năng:** Ghi lịch sử tạo, sửa, kích hoạt và kết thúc chương trình khuyến mãi.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | LichSuID | long | Mã dòng lịch sử khuyến mãi (PK) |
| 2 | KhuyenMaiID | long | Chương trình xảy ra thao tác (FK) |
| 3 | NguoiThucHienID | long | Tài khoản thực hiện thao tác (FK) |
| 4 | HanhDong | string | Loại thao tác: tạo, sửa, kích hoạt hoặc kết thúc |
| 5 | NoiDung | string | Thông tin chi tiết thay đổi |
| 6 | ThoiDiem | DateTime | Thời điểm thực hiện thao tác |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 32. YeuCauDoiTra

**Chức năng:** Lưu yêu cầu đổi hoặc trả hàng, trạng thái duyệt và đơn đổi mới nếu có.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | YeuCauDoiTraID | long | Mã định danh yêu cầu đổi hoặc trả (PK) |
| 2 | MaYeuCau | string | Mã nghiệp vụ của yêu cầu đổi/trả (UK) |
| 3 | MaChongLap | Guid | Mã chống ghi nhận yêu cầu bị gửi lại nhiều lần (UK) |
| 4 | DonHangID | long | Đơn hàng gốc phát sinh yêu cầu (FK) |
| 5 | KhachHangID | long | Khách hàng gửi yêu cầu và là chủ đơn gốc (FK) |
| 6 | LoaiYeuCau | string | Loại yêu cầu: DOI hoặc TRA |
| 7 | LyDo | string | Lý do khách yêu cầu đổi/trả |
| 8 | TrangThai | string | Trạng thái xử lý từ chờ duyệt đến hoàn tất, từ chối hoặc hủy |
| 9 | NguoiDuyetID | long | Tài khoản duyệt yêu cầu (FK) |
| 10 | NgayTao | DateTime | Thời điểm tạo yêu cầu |
| 11 | NgayDuyet | DateTime | Thời điểm duyệt yêu cầu |
| 12 | LyDoXuLy | string | Nội dung giải thích việc duyệt hoặc từ chối |
| 13 | NgayHoanTat | DateTime | Thời điểm hoàn tất xử lý yêu cầu |
| 14 | DonHangDoiMoiID | long | Đơn hàng thay thế được tạo khi đổi sản phẩm (FK) |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | pheDuyet() | void | Phê duyệt yêu cầu đổi/trả |
| 2 | tuChoi() | void | Từ chối yêu cầu đổi/trả |
| 3 | hoanTat() | void | Hoàn tất xử lý yêu cầu đổi/trả |

---

## 33. ChiTietDoiTra

**Chức năng:** Lưu các dòng hàng đổi/trả, số lượng xử lý và khoản hoàn được duyệt.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | ChiTietDoiTraID | long | Mã định danh dòng hàng đổi/trả (PK) |
| 2 | YeuCauDoiTraID | long | Yêu cầu đổi/trả chứa dòng hàng (FK) |
| 3 | DonHangID | long | Mã đơn hàng gốc, tham gia các khóa ngoại ghép (FK) |
| 4 | ChiTietDonHangID | long | Dòng hàng đã mua đang được đổi/trả (FK) |
| 5 | SoLuongYeuCau | int | Số lượng khách đề nghị đổi hoặc trả |
| 6 | SoLuongDuyet | int | Số lượng được nhân viên chấp thuận |
| 7 | SoLuongDaNhan | int | Số lượng hàng cửa hàng đã nhận lại |
| 8 | SoLuongBanLai | int | Số lượng hàng nhận về đủ điều kiện nhập lại kho bán |
| 9 | TienHoanDuocDuyet | decimal | Số tiền hoàn được duyệt cho dòng hàng |
| 10 | BienTheDoiSangID | long | Biến thể khách sẽ nhận khi thực hiện đổi (FK) |
| 11 | TinhTrangHang | string | Mô tả tình trạng của hàng đổi/trả |
| 12 | NguoiNhanHangID | long | Tài khoản nhân viên nhận hàng (FK) |
| 13 | NgayNhanHang | DateTime | Thời điểm cửa hàng nhận hàng |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | capNhatSoLuongDuyet() | void | Cập nhật số lượng được duyệt cho dòng hàng |
| 2 | ghiNhanNhanHang() | void | Ghi nhận cửa hàng đã nhận hàng đổi/trả |

---

## 34. HinhAnhDoiTra

**Chức năng:** Lưu hình ảnh làm bằng chứng cho yêu cầu đổi/trả.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | HinhAnhID | long | Mã ảnh chứng minh đổi/trả (PK) |
| 2 | YeuCauDoiTraID | long | Yêu cầu đổi/trả có ảnh đính kèm (FK) |
| 3 | DuongDan | string | Đường dẫn hoặc URL ảnh bằng chứng |
| 4 | MoTa | string | Mô tả nội dung ảnh |
| 5 | NgayTao | DateTime | Thời điểm ảnh được lưu |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 35. LichSuDoiTra

**Chức năng:** Ghi nhận lịch sử xử lý và thay đổi trạng thái yêu cầu đổi/trả.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | LichSuID | long | Mã dòng lịch sử đổi/trả (PK) |
| 2 | YeuCauDoiTraID | long | Yêu cầu đổi/trả được cập nhật (FK) |
| 3 | TrangThaiCu | string | Trạng thái yêu cầu trước thao tác |
| 4 | TrangThaiMoi | string | Trạng thái yêu cầu sau thao tác |
| 5 | NguoiThucHienID | long | Tài khoản nhân viên hoặc quản trị thực hiện thao tác (FK) |
| 6 | NoiDung | string | Lý do hoặc diễn giải thao tác |
| 7 | ThoiDiem | DateTime | Thời điểm ghi nhận thao tác |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## 36. HoanTien

**Chức năng:** Lưu giao dịch hoàn tiền gắn với khoản thanh toán gốc và yêu cầu đổi/trả

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | HoanTienID | long | Mã định danh giao dịch hoàn tiền (PK) |
| 2 | DonHangID | long | Đơn hàng được hoàn tiền (FK) |
| 3 | ThanhToanID | long | Khoản thanh toán gốc nhận hoàn tiền (FK) |
| 4 | YeuCauDoiTraID | long | Yêu cầu đổi/trả liên quan, có thể rỗng khi hủy đơn (FK) |
| 5 | MaYeuCau | Guid | Mã duy nhất chống hoàn tiền nhiều lần do yêu cầu trùng (UK) |
| 6 | LyDo | string | Lý do hoàn tiền |
| 7 | SoTien | decimal | Số tiền hoàn, phải lớn hơn 0 |
| 8 | TrangThai | string | Trạng thái xử lý hoàn tiền |
| 9 | KenhHoanTien | string | Kênh hoặc cổng thực hiện hoàn tiền |
| 10 | MaGiaoDichHoan | string | Mã giao dịch hoàn tiền từ kênh xử lý |
| 11 | BangChung | string | Chứng từ xác nhận đã hoàn tiền |
| 12 | NguoiXuLyID | long | Tài khoản xử lý việc hoàn tiền (FK) |
| 13 | NgayTao | DateTime | Thời điểm tạo giao dịch hoàn |
| 14 | NgayXacNhan | DateTime | Thời điểm hoàn tiền được xác nhận thành công |

### Phương thức

| STT | Phương thức | Kiểu trả về | Mô tả |
|---:|---|---|---|
| 1 | xacNhanHoanTien() | void | Xác nhận hoàn tiền thành công |
| 2 | danhDauThatBai() | void | Đánh dấu giao dịch hoàn tiền thất bại |

---

## 37. DanhGia

**Chức năng:** Lưu đánh giá và điểm số của khách hàng cho từng dòng hàng đã mua.

### Thuộc tính

| STT | Thuộc tính | Kiểu dữ liệu | Mô tả |
|---:|---|---|---|
| 1 | DanhGiaID | long | Mã định danh đánh giá (PK) |
| 2 | DonHangID | long | Đơn hàng phát sinh đánh giá (FK) |
| 3 | ChiTietDonHangID | long | Dòng hàng đã mua được đánh giá, tối đa một đánh giá (FK, UK) |
| 4 | KhachHangID | long | Khách hàng gửi đánh giá, phải thuộc đơn hàng (FK) |
| 5 | SoSao | byte | Điểm đánh giá từ 1 đến 5 sao |
| 6 | NoiDung | string | Nội dung nhận xét của khách hàng |
| 7 | TrangThai | string | Trạng thái hiển thị: HIEN_THI hoặc AN |
| 8 | NgayTao | DateTime | Thời điểm tạo đánh giá |
| 9 | NgayCapNhat | DateTime | Thời điểm chỉnh sửa đánh giá |

### Phương thức

_Lớp không có phương thức được mô tả trong sơ đồ lớp._

---

## Ghi chú rà soát

- Phần Class Diagram của báo cáo liệt kê 37 lớp.
- Mục 3.4.3 mô tả chi tiết từ `VaiTro` đến `HoanTien` (36 lớp).
- `DanhGia` có trong danh sách lớp và phần mô tả cơ sở dữ liệu, nhưng chưa có mục mô tả lớp riêng trong 3.4.3; file này bổ sung thuộc tính từ bảng dữ liệu và ghi rõ chưa có phương thức.
- Các lớp liên kết hoặc lịch sử không có phương thức trong sơ đồ được ghi là không có phương thức.
