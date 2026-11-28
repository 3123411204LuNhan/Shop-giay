# Shop giày — phiên bản C#

Backend ASP.NET Core trên .NET 10, cơ sở dữ liệu MySQL và giao diện React/Vite. Mở `ShopGiay.sln` bằng Visual Studio 2026, cài workload **ASP.NET and web development**, chọn `ShopGiay.Web` làm Startup Project.

## Chạy dự án

1. Chạy `configure-mysql.cmd` để nhập cấu hình MySQL riêng của máy.
2. Kiểm tra kết nối và bảng bằng `dotnet run --project src/ShopGiay.Web -- --db-check`.
3. Nếu dùng database có sẵn, giữ dữ liệu hiện tại. Với database mới hoàn toàn, chạy `--db-setup` rồi `--db-seed` theo [hướng dẫn đầy đủ](README.txt).
4. Nhấn F5/Ctrl+F5 hoặc chạy `start.cmd`. Web mặc định: http://localhost:5173; quản lý: `/manager`; kỹ thuật: `/admin`.

Giao diện đã build trong `src/ShopGiay.Web/wwwroot`, nên chạy web không cần Node.js. Khi sửa giao diện, chạy `npm ci` rồi `npm run build` trong `frontend`.

## Cấu trúc

| Thư mục | Vai trò |
| --- | --- |
| `src/ShopGiay.Web/Controllers` | Nhận yêu cầu HTTP, gọi dịch vụ và trả kết quả |
| `src/ShopGiay.Web/Services` | Điều phối chức năng, kiểm tra nghiệp vụ |
| `src/ShopGiay.Web/Domain` | Đối tượng và quy tắc nghiệp vụ |
| `src/ShopGiay.Web/Repositories` | Truy cập dữ liệu MySQL qua interface và implementation |
| `src/ShopGiay.Web/Infrastructure` | Kết nối, phiên đăng nhập, mật khẩu, email và tác vụ nền |
| `frontend` | Mã nguồn giao diện React |
| `database` | Schema MySQL |
| `tests` | Kiểm thử C# và hồi quy HTTP |
| `docs` | Hướng dẫn đọc code, bảng API và kết quả kiểm thử |

Mở `docs/HUONG_DAN_CONTROL_CODE.html` bằng trình duyệt để theo dõi luồng gọi và vị trí hàm. Mã C# có chú thích `//`. Chi tiết chạy, tài khoản demo, cấu hình email và nâng cấp database nằm trong [README.txt](README.txt).

## Kiểm tra

```powershell
dotnet build ShopGiay.sln
dotnet run --project tests/ShopGiay.Tests
```

Kiểm thử HTTP cần web và MySQL đang chạy: xem `tests/Regression/README.txt`. Kết quả và giới hạn đã kiểm tra được ghi trong `docs/KET_QUA_KIEM_THU.txt`.

## Commit và push trong Visual Studio

Repository: https://github.com/3123411204LuNhan/Shop-giay — nhánh `main`, remote `origin`.

Mở solution trong thư mục đã có Git, vào **Git Changes**, kiểm tra file thay đổi, nhập nội dung commit rồi chọn **Commit All** và **Push**. Khi làm việc từ nhiều máy, dùng **Pull** trước khi sửa để lấy thay đổi mới. Nếu Visual Studio yêu cầu đăng nhập GitHub, dùng tài khoản có quyền ghi repository.

File `src/ShopGiay.Web/appsettings.Local.json`, thư mục `.local`, `.vs`, `bin`, `obj` và `node_modules` được bỏ qua. Mỗi máy tự cấu hình MySQL; mật khẩu và dữ liệu MySQL không nằm trong commit. Hướng dẫn chi tiết: [docs/GIT_VISUAL_STUDIO.txt](docs/GIT_VISUAL_STUDIO.txt).

Bản này thay thế cấu trúc cũ `backend/ShopGiay.Api` bằng solution ở thư mục gốc. Có thể xem lại bản trước trong lịch sử Git. Không chạy các lệnh npm ở thư mục gốc theo tài liệu của bản trước.
