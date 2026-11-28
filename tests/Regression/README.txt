KIỂM THỬ HỒI QUY CHO BACKEND C#

Các test JavaScript gửi HTTP tới ASP.NET Core và dùng mysql2 để đối chiếu database.
Không có Express hoặc nghiệp vụ backend Node.js trong bộ chạy này.

Chỉ chạy trên database TEST có tên bắt đầu bằng sole_studio_nhom19_csharp_qa.
Không chạy trên database chứa dữ liệu thật: các test có tạo/sửa/xóa dữ liệu thử.

Tại thư mục solution, PowerShell:
  $env:ConnectionStrings__ShopGiay='Server=127.0.0.1;Port=3306;Database=sole_studio_nhom19_csharp_qa;User ID=root;Password=MAT_KHAU;DateTimeKind=Utc'
  dotnet run --project src/ShopGiay.Web -- --db-setup
  dotnet run --project src/ShopGiay.Web -- --db-seed
  dotnet run --project src/ShopGiay.Web

Mở terminal thứ hai tại thư mục solution:
  $env:DB_HOST='127.0.0.1'
  $env:DB_PORT='3306'
  $env:DB_NAME='sole_studio_nhom19_csharp_qa'
  $env:DB_USER='root'
  $env:DB_PASSWORD='MAT_KHAU'
  $env:PORT='5173'
  npm ci --prefix tests/Regression
  npm test --prefix tests/Regression

Bộ test được chuyển từ test hồi quy dự án gốc. Các điều chỉnh cho C#:
- So tên thuộc tính cookie không phân biệt hoa/thường, đúng quy tắc HTTP.
- Kiểm tra hết hạn chờ worker C# chạy (15 giây), không import hàm Node.js.
- Đường dẫn hộp thư local trỏ tới ứng dụng C#.
Test không tự dọn toàn bộ database; nên tạo schema mới cho mỗi lần chạy chính thức.
