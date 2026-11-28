SHOP GIÀY NHÓM 19 — PHIÊN BẢN C#

Backend: ASP.NET Core .NET 10, C#, MySQL.
Giao diện: React được giữ từ dự án gốc. Node.js chỉ cần khi sửa/build giao diện
hoặc chạy bộ kiểm thử HTTP; khi chạy website đã build, backend hoàn toàn là C#.
Theo yêu cầu, giao diện đơn hàng tạm ẩn nút, biểu mẫu và dòng hiển thị mã vận đơn.
Trạng thái giao hàng và luồng duyệt/xử lý đơn vẫn được giữ.
Ảnh sản phẩm dùng chung khung giữ tỷ lệ ở quản lý và trang khách; có xem trước
khi sửa ảnh và hình thay thế nếu thiếu ảnh/URL lỗi. Lưu sản phẩm thành công sẽ
báo các tab cửa hàng trong cùng trình duyệt làm mới; thiết bị khác kiểm tra mỗi 30 giây.

BẮT ĐẦU TRÊN VISUAL STUDIO 2026
1. Cài workload “ASP.NET and web development” và SDK .NET 10.
2. Mở ShopGiay.sln, chọn ShopGiay.Web làm Startup Project.
3. Chạy configure-mysql.cmd hoặc lệnh sau tại thư mục solution:
     dotnet run --project src/ShopGiay.Web -- --db-config
   Nhập host, port, user, database và mật khẩu MySQL (gõ ẩn).
   Chương trình chỉ lưu appsettings.Local.json sau khi đăng nhập MySQL thành công.
   Sau đó chạy --db-check: xác minh server chưa có nghĩa database/bảng đã tồn tại.
   Có thể cấu hình thủ công: copy appsettings.Local.example.json thành
   appsettings.Local.json trong src/ShopGiay.Web rồi điền thông tin.
4. Nếu dùng database đã có của Shop-giay-main: giữ nguyên tên database,
   KHÔNG chạy seed. Bản mới đọc cùng 37 bảng và xác minh được mật khẩu cũ.
   Nếu schema cũ thiếu cột quy trình, sao lưu trước rồi chạy lệnh migrate bên dưới.
5. Nhấn F5/Ctrl+F5. URL mặc định: http://localhost:5173.
   Giao diện đã build sẵn trong src/ShopGiay.Web/wwwroot.

DATABASE MỚI
Kiểm tra cấu hình bằng lệnh chỉ đọc:
  dotnet run --project src/ShopGiay.Web -- --db-check
Trong appsettings.Local.json, chọn tên database mới, ví dụ sole_studio_nhom19_csharp.
Mở terminal tại thư mục có ShopGiay.sln:
  dotnet run --project src/ShopGiay.Web -- --db-setup
  dotnet run --project src/ShopGiay.Web -- --db-seed
Setup từ chối database đã có bảng; seed từ chối khi đã có tài khoản.
Seed tạo 5 tài khoản, 12 sản phẩm, 108 biến thể, 12 đơn và dữ liệu trả hàng mẫu.
Khách: khach@sole.vn | Quản lý: shop@sole.vn | Kỹ thuật: admin@sole.vn
Mật khẩu các tài khoản demo: SoleDemo@123
Đăng nhập đúng khu vực: trang mua sắm /manager hoặc /admin.
Khách: http://localhost:5173/ — chỉ CUSTOMER.
Quản lý: http://localhost:5173/manager — SHOP_MANAGER.
Kỹ thuật: http://localhost:5173/admin — SYSTEM_ADMIN.
Đúng mật khẩu nhưng sai khu vực vẫn bị từ chối theo phân quyền.
Sai mật khẩu/khu vực được trả thành HTTP 401/403, cho phép nhập lại trên web.

MIGRATE SCHEMA CŨ KHI CẦN
  dotnet run --project src/ShopGiay.Web -- --db-migrate
Lệnh bổ sung cột/ràng buộc quy trình và chuẩn hóa trạng thái cũ; có cập nhật dữ liệu.
Ứng dụng không tự migrate khi nhấn F5.

CÁCH ĐỌC VÀ CONTROL CODE
Mở docs/HUONG_DAN_CONTROL_CODE.html bằng trình duyệt.
Tài liệu gồm sơ đồ gọi, đối chiếu 17 nhóm use case, 75 API, tên hàm và số dòng.
Mã C# có // trước hàm và từng câu lệnh để lần theo luồng xử lý.
F9 đặt breakpoint; F10 đi qua dòng; F11 đi vào hàm; Shift+F11 đi ra;
F12 tới định nghĩa; Shift+F12 tìm nơi gọi; Ctrl+G tới số dòng.

KIỂM TRA
  dotnet build ShopGiay.sln
  dotnet run --project tests/ShopGiay.Tests
Kiểm tra hồi quy có MySQL: xem tests/Regression/README.txt.
Kết quả thực tế và giới hạn kiểm thử: docs/KET_QUA_KIEM_THU.txt.

SỬA GIAO DIỆN
  cd frontend
  npm ci
  npm run build
Sau build, chạy lại/refresh website C#.
Muốn hot reload React: chạy C# tại localhost:5173, rồi npm run dev trong frontend.
Mở localhost:5174. Vite chuyển /api sang C#; không có server Express.

EMAIL OTP
Để trống Mail:Host thì thư được lưu ở src/ShopGiay.Web/.local/mail.
Thư không nằm trong wwwroot và không có API đọc OTP.
SMTP của bản này dùng STARTTLS (thường cổng 587); không dùng TLS ngầm cổng 465.
Nếu trước đây dùng SMTP_APP_PASSWORD, chuyển giá trị sang Mail:Password.

LỖI ACCESS DENIED / USING PASSWORD: NO
NO nghĩa là ứng dụng không gửi mật khẩu MySQL, thường do chưa tạo file local
hoặc Password để trống. Dùng --db-config và nhập mật khẩu MySQL đang dùng.
YES nghĩa là đã gửi mật khẩu nhưng MySQL từ chối tài khoản/thông tin đăng nhập.
Không dùng mật khẩu tài khoản shop@sole.vn hay mật khẩu đăng nhập Windows.
Sau khi --db-check thành công: dùng database cũ thì chạy web; database mới mới
cần --db-setup rồi --db-seed. Không chạy seed sau khi setup thất bại.

GIỚI HẠN ĐƯỢC GIỮ TỪ BẢN GỐC
Chuyển khoản được đối soát thủ công; chưa tích hợp cổng thanh toán thật.
Đổi size theo quy trình trả hàng → hoàn tiền → đặt lại, chưa có đổi trực tiếp.
Chi tiết khác biệt so với các nhánh mở rộng trong tài liệu Word được ghi trong hướng dẫn.

Nguồn tham khảo môi trường:
https://learn.microsoft.com/en-us/dotnet/core/install/windows
https://mysqlconnector.net/
