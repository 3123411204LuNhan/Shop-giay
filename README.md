# Shop-giay — SOLE STUDIO Nhóm 19

Ứng dụng web dùng **Node.js + React + MySQL**, xây dựng từ các chức năng trong `S2_Nhom19_Lan3.docx`. Giao diện tiếng Việt, thích ứng máy tính và điện thoại. Dữ liệu được lưu thật vào MySQL; giỏ khách được lưu riêng theo tab, không tự gộp vào tài khoản; danh sách yêu thích được lưu trên thiết bị.

## Các khu vực của ứng dụng

- Cửa hàng: [http://localhost:5173](http://localhost:5173)
- Chủ shop: [http://localhost:5173/manager](http://localhost:5173/manager)
- Kỹ thuật: [http://localhost:5173/admin](http://localhost:5173/admin)

Nếu máy chủ Node.js đã dừng, mở terminal trong thư mục dự án và chạy:

```powershell
npm.cmd run dev
```

Hoặc chạy `start.cmd`. Giữ cửa sổ terminal đang chạy để tiếp tục sử dụng. Dừng bằng Ctrl+C. Nếu cổng 5173 đã có ứng dụng này chạy thì dùng luôn địa chỉ ở trên, không cần khởi động thêm một bản.

## Tài khoản mẫu

| Vai trò | Email đăng nhập | Mật khẩu mẫu |
|---|---|---|
| Khách hàng | `khach@sole.vn` | `SoleDemo@123` |
| Chủ shop | `shop@sole.vn` | `SoleDemo@123` |
| Nhân viên kỹ thuật | `admin@sole.vn` | `SoleDemo@123` |

Có thêm khách mẫu `linh@sole.vn`, `kiet@sole.vn` dùng cùng mật khẩu. Các tài khoản và mật khẩu này dành cho môi trường đồ án cục bộ. Mật khẩu trong database là hash scrypt có salt, không phải mật khẩu nguyên văn.

Trong hộp đăng nhập, mở **Tài khoản trải nghiệm** để điền nhanh một vai trò; vẫn cần bấm Đăng nhập.

## Cấu hình môi trường

Ứng dụng yêu cầu Node.js từ 22.12 và MySQL từ 8.0.16. Cấu hình mặc định ưu tiên MySQL cổng **3306** và website cổng **5173**. Tài khoản và mật khẩu MySQL do người chạy cấu hình riêng trong `.env`; repository chỉ cung cấp `.env.example`.

Không commit `.env`, dữ liệu MySQL cục bộ hoặc thư viện `node_modules`. Nếu dịch vụ MySQL nằm trên cùng máy, dùng `DB_HOST=127.0.0.1`; nếu dùng máy chủ khác, điền host phù hợp với quyền MySQL đã được cấp.

## Cài đặt và chạy từ GitHub

1. Clone repository:

```powershell
git clone https://github.com/3123411204LuNhan/Shop-giay.git
cd Shop-giay
```

2. Cài Node.js từ 22.12 trở lên và MySQL **8.0.16 trở lên**. Khuyến nghị MySQL 8.0 hoặc 8.4. Cần phiên bản hỗ trợ CHECK và cột tính toán lưu trữ.
3. Mở terminal tại thư mục dự án và cài thư viện:

```powershell
npm.cmd ci
if (!(Test-Path .env)) { Copy-Item .env.example .env }
```

4. Điền cấu hình MySQL trong `.env`. Với máy chủ của đồ án, đặt `DB_HOST=Admin-pc`, cổng `3306` và người dùng `root`. Ví dụ dưới đây dùng MySQL trên cùng máy.

```dotenv
PORT=5173
HOST=127.0.0.1
DB_HOST=127.0.0.1
DB_PORT=3306
DB_NAME=sole_studio_nhom19
DB_USER=root
DB_PASSWORD=mat_khau_mysql_cua_ban
APP_ORIGIN=http://localhost:5173
NODE_ENV=development
```

5. Khi database đích còn mới, tạo cấu trúc rồi nạp dữ liệu:

```powershell
npm.cmd run db:setup
npm.cmd run db:seed
npm.cmd run dev
```

`db:setup` từ chối ghi đè khi database đã có bảng. `db:seed` từ chối chạy nếu đã có tài khoản, để tránh nạp trùng hoặc xóa dữ liệu đã sử dụng. Sau lần khởi tạo thành công, bình thường chỉ chạy `npm.cmd run dev`.

Khi cập nhật một bản đã có dữ liệu, giữ cấu hình trong `.env`, chạy `npm.cmd ci`, tiếp theo chạy `npm.cmd run db:migrate` rồi `npm.cmd run dev`. Các lần mở lại chỉ cần bật MySQL và chạy `npm.cmd run dev` trong thư mục chứa `package.json`.

Nếu dùng database mới, đổi `DB_NAME` rồi chạy lại hai bước. MySQL DDL tự commit; nếu lần tạo schema bị gián đoạn, kiểm tra lỗi và dùng tên database mới thay vì xóa database đang có dữ liệu.

### Dùng Docker thay cho MySQL đã cài

Thư mục có `docker-compose.yml` cho máy đã cài Docker Desktop. Dùng tài khoản ứng dụng riêng và đặt trong `.env`:

```dotenv
DB_HOST=127.0.0.1
DB_PORT=3306
DB_USER=sole_app
DB_PASSWORD=tu_dat_mat_khau_cho_user_ung_dung
```

```powershell
docker compose up -d
npm.cmd run db:setup
npm.cmd run db:seed
```

Không khởi động container trùng cổng 3306 với MySQL đang chạy. Docker là phương án cài mới, không cần dùng nếu đã có MySQL hoạt động. Volume giữ dữ liệu sau khi tắt container.

## Các màn hình và chức năng

### Khách hàng

- Trang chủ, bộ sưu tập sản phẩm, tìm theo tên/mã/thương hiệu.
- Lọc danh mục, thương hiệu, kích cỡ, màu, mức giá; sắp xếp giá và sản phẩm mới.
- Xem chi tiết, giá, mô tả, đánh giá và lựa chọn size/màu còn hàng.
- Giỏ hàng: thêm, đổi số lượng, xóa. Giỏ tài khoản lưu riêng trong MySQL theo ID khách; giỏ khách dùng sessionStorage theo tab. Đăng nhập/đăng xuất dọn giỏ khách, không tự chuyển sản phẩm vào tài khoản khác.
- Đăng ký, đăng nhập, đăng xuất, sửa họ tên/điện thoại/địa chỉ.
- Quên mật khẩu bằng OTP 6 số qua Gmail/SMTP; nếu chưa cấu hình hoặc gửi thất bại thì lưu hộp thư `.local/mail`. Xác minh mã trước khi đặt mật khẩu mới.
- Đặt hàng bằng COD hoặc chuyển khoản; áp mã giảm giá; miễn phí giao hàng từ 1.500.000đ, còn lại 30.000đ.
- Xem đơn, lịch sử trạng thái; hủy trước khi giao; gửi đánh giá cho đơn hoàn thành.
- Gửi yêu cầu đổi hoặc trả kèm số lượng, lý do và ảnh bằng chứng tùy chọn từ tệp hoặc URL. Đổi sang biến thể khác cùng sản phẩm và cùng đơn giá lúc mua. Sau khi nhận hàng và kiểm tra điều kiện, shop tạo đơn thay thế.
- Yêu thích sản phẩm trên thiết bị, là tiện ích bổ sung không tạo bảng ngoài 37 bảng chính.

### Chủ shop

- Dashboard: khoản thu đã xác nhận trừ khoản hoàn đã xác nhận, đơn trong kỳ, đơn chờ xử lý, cảnh báo tồn thấp, dòng tiền theo ngày, sản phẩm bán chạy.
- Chọn khoảng ngày và xuất CSV tổng hợp.
- Tạo/sửa/ẩn/ngừng bán sản phẩm; cấu hình danh mục, thương hiệu, giá và ảnh tùy chọn từ tệp hoặc URL.
- Tạo biến thể; sửa size/màu khi chưa giao dịch; sửa giá riêng, ẩn/hiện biến thể. Biến thể đã giao dịch không được đổi size/màu.
- Xem tồn thực tế, lượng đang giữ, tồn khả dụng; nhập kho theo chứng từ hoặc nhập số kiểm đếm để tạo phiếu điều chỉnh kèm lý do.
- Xem chi tiết đơn, chuyển bước theo quy trình; xác nhận thu tiền bằng bằng chứng; hủy đơn và giải phóng kho.
- Tạo/sửa thông tin ưu đãi, kích hoạt và kết thúc. Giao diện hiện tạo phạm vi toàn bộ sản phẩm; backend còn đọc được phạm vi sản phẩm/danh mục/thương hiệu trong database.
- Xem khách hàng và tổng hợp lịch sử đơn.
- Duyệt/từ chối yêu cầu trả; xác nhận nhận hàng, chọn có đủ điều kiện bán lại; tạo khoản hoàn và xác nhận bằng chứng; hoàn tất xử lý.

### Nhân viên kỹ thuật

- Dashboard hệ thống: database, phiên bản MySQL, số bảng, tài khoản và phiên đăng nhập còn hiệu lực.
- Tạo tài khoản khách/chủ shop/admin.
- Sửa họ tên, gán vai trò, khóa hoặc vô hiệu hóa tài khoản; thu hồi phiên khi thay đổi quyền.
- Không tự sửa vai trò/khóa tài khoản đang dùng; bảo vệ quản trị viên hoạt động cuối cùng.
- Xem nhật ký các thao tác quản trị.
- Backend phân quyền riêng: admin kỹ thuật không tự động có quyền xử lý nghiệp vụ shop và chủ shop không có API quản trị kỹ thuật.

## Dữ liệu mẫu

- 37 bảng MySQL tương ứng 37 bảng chính từ thiết kế SQL Server trước đó.
- 5 tài khoản ban đầu, 3 vai trò, 3 phương thức thanh toán; phương thức ONLINE chưa tích hợp nên bị vô hiệu hóa.
- 12 sản phẩm, 108 biến thể size/màu, danh mục/thương hiệu và số dư kho có sổ kho đối chiếu được.
- 12 đơn ban đầu ở nhiều trạng thái; khoản thu giả lập, đánh giá, 2 ưu đãi và 1 yêu cầu trả mẫu.
- Mã `WELCOME10`: giảm 10%, tối đa 300.000đ, đơn tối thiểu 1.500.000đ.
- Mã `SOLE200`: giảm 200.000đ, đơn tối thiểu 2.500.000đ.
- Khi chạy kiểm thử tích hợp, có thêm tài khoản `QA Kiểm thử` và đơn/hoàn tiền mô phỏng; chúng được giữ để đối chiếu, không phải giao dịch thực.

Giá, ảnh và số liệu là dữ liệu minh họa cho đồ án. Không chuyển tiền thật theo dữ liệu mẫu. Các ảnh giày được lưu trong `public/images`, lấy từ Unsplash và dùng để minh họa phong cách, không khẳng định là ảnh chính xác của từng model sản phẩm.

## Kiểm tra và build

Máy chủ ứng dụng phải đang chạy trước khi kiểm thử API:

```powershell
npm.cmd run check
npm.cmd test
npm.cmd run build
npm.cmd start
```

`npm.cmd test` kiểm tra các luồng thật qua API và MySQL: xác thực, chống tự nâng quyền, phân quyền, đặt hàng có ưu đãi, gửi lại cùng request, giữ/xuất/giải phóng kho, thanh toán, trả hàng, hoàn tiền và đối chiếu sổ kho. Chỉ chạy trên database riêng có tên bắt đầu bằng `sole_studio_nhom19`.

`npm.cmd start` phục vụ bản đã build. Chỉ chạy một máy chủ cho cùng cổng. Cookie tự dùng Secure khi request đi qua HTTPS; NODE_ENV không quyết định cờ này. Khi dùng reverse proxy HTTPS, cấu hình TRUST_PROXY_HOPS theo hạ tầng thực tế.

## Cấu trúc mã nguồn

```text
Shop-giay/
  src/main.jsx          Giao diện React của ba khu vực
  src/styles.css        Bố cục và giao diện responsive
  src/extras.css        Các thành phần bổ sung
  server/index.js       Máy chủ Express và bảo vệ yêu cầu
  server/routes.js      API nghiệp vụ, giao dịch và phân quyền
  server/auth.js        Hash mật khẩu, phiên và nhật ký
  server/db.js          Kết nối MySQL và transaction
  database/schema.sql   Đủ 37 bảng MySQL, không có DROP
  scripts/setup-db.js   Khởi tạo schema trên database mới
  scripts/seed.js       Dữ liệu mẫu, không ghi đè dữ liệu cũ
  tests/                Kiểm thử tích hợp API và MySQL
  public/images/        Ảnh minh họa cục bộ
  .env.example          Mẫu cấu hình không chứa mật khẩu thật
  .env                  Cấu hình riêng của máy, không chia sẻ
```

## Quy tắc nghiệp vụ quan trọng

- Một sản phẩm có nhiều biến thể; mỗi biến thể có tồn riêng theo size/màu. Tồn khả dụng = tồn thực tế − đang giữ.
- Thêm giỏ chưa giữ kho. Tạo đơn kiểm tra giá từ database, khóa số dư kho và ghi đơn/phân bổ/sổ kho trong một transaction.
- Gửi lại cùng `requestId` tạo đơn trả về đơn cũ; không giữ kho hai lần.
- Luồng đơn: chờ xác nhận → đã xác nhận → đang chuẩn bị → đang giao → đã giao → hoàn thành. Muốn hoàn thành phải thu đủ tiền.
- COD chỉ được xác nhận thu sau khi đã giao. Chuyển khoản do chủ shop xác nhận bằng bằng chứng; chưa tích hợp ngân hàng/cổng thanh toán.
- Hủy trước khi xuất chỉ giảm lượng đang giữ, không cộng tồn vật lý. Đơn đã thu sẽ tạo yêu cầu hoàn tiền chờ xử lý.
- Chỉ nhập lại hàng trả đủ điều kiện bán lại, chỉ xử lý nhận hàng một lần. Khoản hoàn xác nhận cần bằng chứng.
- Giá và giảm giá đã chụp trên đơn không thay đổi theo việc sửa sản phẩm hoặc chương trình sau này.
- Khi chọn nhiều mã, bản hiện tại chỉ nhận **một mã ưu đãi cho một đơn**. Cột kết hợp trong schema được giữ cho mở rộng nhưng chưa có giao diện cộng dồn mã.
- `rowversion` của SQL Server được chuyển thành `PhienBan bigint` trên MySQL. Các thao tác kho và đơn quan trọng dùng khóa `FOR UPDATE`; đây không phải tính năng rowversion tự động của MySQL.
- Không có tác vụ tự động hủy đơn hết hạn. Gửi OTP email thật cần cấu hình SMTP theo EMAIL_SETUP.md. Ảnh sản phẩm và ảnh bằng chứng hỗ trợ một tệp JPEG, PNG hoặc WebP tối đa 5 MB, hoặc một URL. Tệp mặc định lưu tại `.local/uploads` và có thể đổi thư mục qua biến `IMAGE_UPLOAD_DIR`. Ảnh bằng chứng tải lên được phục vụ qua API kiểm tra quyền.

## Nguồn ảnh minh họa

Ảnh cục bộ tải từ các tài nguyên Unsplash: `photo-1542291026-7eec264c27ff`, `photo-1600269452121-4f2416e55c28`, `photo-1549298916-b41d501d3772`, `photo-1600185365926-3a2ce3cdb9eb`, `photo-1539185441755-769473a23570`, `photo-1543508282-6319a3e2621f`. Script `scripts/download-images.js` tải lại nếu cần. Font giao diện là Be Vietnam Pro qua Google Fonts, có font hệ thống dự phòng khi không có mạng.

## Phiên đăng nhập độc lập giữa ba khu vực

Khách hàng, chủ shop và kỹ thuật dùng ba cookie HttpOnly riêng: `sole_customer_session`, `sole_manager_session`, `sole_admin_session`. Có thể đăng nhập ba tài khoản trong cùng trình duyệt. Mở một khu vực khác không dùng nhầm vai trò; đăng xuất chỉ thu hồi phiên của khu vực hiện tại.

Sau khi cập nhật từ bản dùng chung `sole_session`, đăng nhập lại một lần ở từng khu vực. Cookie chung cũ sẽ được xóa. Backend vẫn kiểm tra vai trò từ database, không cấp quyền chỉ dựa vào tên cookie hoặc header.

Giao diện gửi `X-Sole-Area: customer`, `manager` hoặc `admin` theo khu vực đang mở. Client gọi API trực tiếp cần gửi header này cho `/api/me` và các API xác thực. Đăng nhập chỉ thành công ở đúng khu vực: khách tại `/`, chủ shop tại `/manager`, kỹ thuật tại `/admin`. Không tự chuyển tài khoản nhân sự từ trang đăng nhập khách sang workspace.

Cookie Secure được quyết định theo HTTPS của request, không theo NODE_ENV, để đăng nhập HTTP local vẫn hoạt động với bản build production. Nếu thực sự triển khai sau reverse proxy HTTPS, đặt `TRUST_PROXY_HOPS` theo số proxy tin cậy thực tế; mặc định 0 là không tin header proxy.

Nút “Xem cửa hàng” đã được bỏ khỏi sidebar của chủ shop và admin kỹ thuật.

## OTP Gmail và fallback local

Xem [hướng dẫn gửi OTP](EMAIL_SETUP.md). Bản nâng cấp dùng mã 6 số, hiệu lực 10 phút, tối đa 5 lần nhập sai. Database cũ cần chạy `npm.cmd run db:migrate` một lần; database mới đã có cột cần thiết.

## Giỏ hàng và tài khoản dùng chung máy

Giỏ của A không được đưa vào localStorage để rồi gộp sang B. Giỏ khách cũ dùng khóa `sole-cart` được dọn; giỏ khách mới lưu riêng theo tab và được xóa khi đăng nhập/đăng xuất. Giỏ đã đăng nhập luôn lấy từ database của tài khoản hiện tại, được giữ lại khi đăng xuất để chính chủ có thể mua tiếp.

Các tab nhận thông báo khi đổi tài khoản và kiểm tra lại phiên khi quay về tab. Request riêng tư có `X-Sole-Account`; nếu tab còn hiển thị A nhưng cookie đã đổi sang B, server trả SESSION_CHANGED và không cập nhật giỏ B. Không tự gửi lại thao tác cũ dưới tài khoản mới.

Tài khoản có vai trò SHOP_MANAGER hoặc SYSTEM_ADMIN không được đăng nhập khu vực khách, kể cả khi còn có cờ CUSTOMER. API giỏ, đặt hàng và đổi/trả của khách cũng chặn tài khoản nhân sự; chủ shop vẫn xem chi tiết đơn qua quyền quản lý.


## Cập nhật luồng nghiệp vụ phát triển ngày 18/11/2026

Đọc [Hướng dẫn luồng phát triển](HUONG_DAN_LUONG_PHAT_TRIEN.md) để sử dụng phiếu kho nháp/xác nhận, cảnh báo tồn, kiểm tra tổng tiền trước đặt, tự hết hạn giữ hàng chuyển khoản, đối soát tiền đến muộn, giao thất bại và hoàn tiền có thử lại. Quản lý kiêm kho; admin giữ phạm vi kỹ thuật. Chạy `npm.cmd run db:migrate` trước khi chạy bản mới; không tạo lại database hoặc seed lại.


## Giao diện khách hàng ngày 19/11/2026

- Khám phá (`/`): chủ đề theo danh mục và 6 mẫu giày được gợi ý ngẫu nhiên; nút Đổi gợi ý chọn lại. Bấm chủ đề mở trang sản phẩm với bộ lọc tương ứng.
- Sản phẩm (`/products`): danh sách riêng với tìm kiếm, danh mục, thương hiệu, size, màu, giá và sắp xếp. Bộ lọc nằm trong URL, giữ khi tải lại hoặc dùng nút quay lại của trình duyệt.
- Running Club (`/running-club`): banner chiến dịch, thông tin giao hàng và mã đang hiệu lực/chưa hết tổng lượt dùng. Hiển thị điều kiện, mức giảm tối đa và hạn áp dụng. Quản lý tiếp tục sửa mã qua mục Khuyến mãi.
- Sao chép mã hoặc Mua với mã này để chuyển sang sản phẩm, điền sẵn mã vào checkout. Mức giảm chỉ được xác nhận sau khi máy chủ kiểm tra điều kiện.
- Banner chiến dịch là nội dung giao diện cục bộ; chưa có chức năng quản lý banner riêng trong workspace.
