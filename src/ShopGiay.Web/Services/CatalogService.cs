using System.Globalization;
using System.Text.RegularExpressions;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
public sealed class CatalogService(ICatalogRepository repository, Database db, SessionContext session, Operations operations)
{
    // Khách tìm/lọc sản phẩm: chỉ lấy hàng đang công bố, danh mục/thương hiệu hoạt động.
    public async Task<object> Browse(Row q)
    {
        // Tạo biến args để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var args = new List<object?>();
        // Tạo biến filters để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var filters = new List<string>
        {
            "s.TrangThai='DANG_BAN'",
            "d.DangHoatDong=1",
            "t.DangHoatDong=1"
        };
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (q.S("q") != "")
        {
            // Thực hiện bước filters.Add trong CatalogService; tiếp tục theo luồng của hàm.
            filters.Add("(s.TenSanPham LIKE ? OR s.MaSanPham LIKE ? OR t.TenThuongHieu LIKE ?)");
            // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
            for (var i = 0; i < 3; i++)
                // Thực hiện bước args.Add trong CatalogService; tiếp tục theo luồng của hàm.
                args.Add("%" + Text(q["q"], 100) + "%");
        }

        // Thực hiện bước q.S trong CatalogService; tiếp tục theo luồng của hàm.
        foreach (var(field, col)in new[]
        {
            ("category", "s.DanhMucID"),
            ("brand", "s.ThuongHieuID")
        }

        )
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (q.S(field) != "")
            {
                // Thực hiện bước filters.Add trong CatalogService; tiếp tục theo luồng của hàm.
                filters.Add(col + "=?");
                // Thực hiện bước args.Add trong CatalogService; tiếp tục theo luồng của hàm.
                args.Add(Positive(q[field]));
            }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (q.S("max") != "")
        {
            // Thực hiện bước filters.Add trong CatalogService; tiếp tục theo luồng của hàm.
            filters.Add("s.GiaBan<=?");
            // Thực hiện bước args.Add trong CatalogService; tiếp tục theo luồng của hàm.
            args.Add(Price(q["max"]));
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (q.S("size") != "")
        {
            // Thực hiện bước filters.Add trong CatalogService; tiếp tục theo luồng của hàm.
            filters.Add("EXISTS(SELECT 1 FROM BienTheSanPham z JOIN KichCo kc ON kc.KichCoID=z.KichCoID JOIN TonKho tk ON tk.BienTheID=z.BienTheID WHERE z.SanPhamID=s.SanPhamID AND kc.GiaTri=? AND z.DangKinhDoanh=1 AND tk.SoLuongKhaDung>0)");
            // Thực hiện bước args.Add trong CatalogService; tiếp tục theo luồng của hàm.
            args.Add(Text(q["size"], 20));
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (q.S("color") != "")
        {
            // Thực hiện bước filters.Add trong CatalogService; tiếp tục theo luồng của hàm.
            filters.Add("EXISTS(SELECT 1 FROM BienTheSanPham z WHERE z.SanPhamID=s.SanPhamID AND z.MauSacID=? AND z.DangKinhDoanh=1)");
            // Thực hiện bước args.Add trong CatalogService; tiếp tục theo luồng của hàm.
            args.Add(Positive(q["color"]));
        }

        // Tạo biến order để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var order = q.S("sort") switch
        {
            "price_asc" => "s.GiaBan ASC",
            "price_desc" => "s.GiaBan DESC",
            "newest" => "s.SanPhamID DESC",
            _ => "s.SanPhamID ASC"
        };
        // Gọi CatalogRepository.Browse → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        return new
        {
            products = await repository.Browse(filters.ToArray(), order, args.ToArray()),
            categories = await repository.Categories(),
            brands = await repository.Brands(),
            sizes = await repository.Sizes(),
            colors = await repository.Colors()
        };
    }

    // Chi tiết → biến thể và đánh giá hiện hành; không đổi tồn kho.
    public async Task<object> Details(long id)
    {
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.PublicProduct([id])).One("Không tìm thấy sản phẩm.");
        // Gọi CatalogRepository.AvailableVariants → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        p["variants"] = await repository.AvailableVariants([p.N("price"), p.L("id")]);
        // Gọi CatalogRepository.Reviews → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        p["reviews"] = await repository.Reviews([p.L("id")]);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return p;
    }

    // Quản lý xem toàn bộ sản phẩm, kể cả ẩn/ngừng bán.
    public Task<DbResult> ListProducts() => repository.Products();
    // Danh sách biến thể kèm tồn thực tế, phân bổ và khả dụng.
    public Task<DbResult> ListVariants() => repository.Variants();
    // Dữ liệu lựa chọn cho biểu mẫu quản lý.
    public async Task<object> Lookups() => new
    {
        categories = await repository.AllCategories(),
        brands = await repository.AllBrands(),
        sizes = await repository.Sizes(),
        colors = await repository.AllColors()
    };
    // Size EU nhận số nguyên hoặc thập phân, chuẩn hóa dấu phẩy thành dấu chấm.
    public Task<object> CreateSize(Row body) => db.Transaction<object>(async c =>
    {
        // Tạo biến raw để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var raw = body.S("name").Trim().Replace(',', '.');
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!Regex.IsMatch(raw, @"^\d{1,3}(?:\.\d{1,2})?$"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Size EU phải là số dương, ví dụ 42 hoặc 42.5.");
        // Tạo biến n để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var n = decimal.Parse(raw, CultureInfo.InvariantCulture);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (n <= 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Size không hợp lệ.");
        // Tạo biến value để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var value = n.ToString("0.##", CultureInfo.InvariantCulture);
        // Gọi CatalogRepository.AddSize → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.AddSize([value, decimal.Floor(n)], c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return (await repository.SizeByValue([value], c)).One();
    });
    // Tạo/kích hoạt lại màu có tên và mã hex hợp lệ.
    public Task<object> CreateColor(Row body) => db.Transaction<object>(async c =>
    {
        // Tạo biến name để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var name = Regex.Replace(body.S("name").Trim(), @"\s+", " ");
        // Tạo biến hex để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var hex = body.S("hex").Trim();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (name.Length is < 1 or > 100 || (hex != "" && !Regex.IsMatch(hex, "^#[a-fA-F0-9]{6}$")))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Tên màu hoặc mã #RRGGBB không hợp lệ.");
        // Gọi CatalogRepository.AddColor → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.AddColor([name, hex == "" ? null : hex], c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return (await repository.ColorByName([name], c)).One();
    });
    // Tạo sản phẩm → ảnh chính → nhật ký trong cùng giao dịch.
    public Task<object> CreateProduct(Row b) => db.Transaction<object>(async c =>
    {
        // Gọi CatalogRepository.InsertProduct → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var a = await repository.InsertProduct([Required(b["code"], "mã sản phẩm"), Required(b["name"], "tên sản phẩm"), Text(b["description"], 8000), Price(b["price"]), Positive(b["categoryId"]), Positive(b["brandId"]), Status(b.S("status", "DANG_BAN")), session.UserId], c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.S("image") != "")
            // Gọi CatalogRepository.InsertImage → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.InsertImage([a.InsertId, Image(b["image"])], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "TAO_SAN_PHAM", "SanPham", a.InsertId, b.S("name"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            id = a.InsertId
        };
    });
    // Phải ẩn sản phẩm trước khi thay đổi nội dung; đổi trạng thái riêng vẫn được phép.
    public Task<object> UpdateProduct(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.LockProduct([id], c)).One();
        // Tạo biến status để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var status = Status(b.S("status", p.S("status")));
        // Tạo biến photo để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var photo = (await repository.LockImage([id], c)).FirstOrDefault();
        // Gán kết quả vào p["image"] để bước xử lý tiếp theo dùng dữ liệu mới.
        p["image"] = photo?.S("image") ?? "";
        // Tạo biến changed để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var changed = new[]
        {
            "code",
            "name",
            "description",
            "price",
            "categoryId",
            "brandId",
            "image"
        }.Any(k => b.ContainsKey(k) && b.S(k) != p.S(k));
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (changed && p.S("status") != "AN")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Hãy lưu trạng thái Ẩn trước khi chỉnh sửa thông tin hoặc ảnh sản phẩm.", 409);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (changed)
        {
            // Tạo biến oldImage để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var oldImage = p.S("image");
            // Xử lý từng kv trong tập dữ liệu đã lấy/kiểm tra.
            foreach (var kv in b)
                // Gán kết quả vào p[kv.Key] để bước xử lý tiếp theo dùng dữ liệu mới.
                p[kv.Key] = kv.Value;
            // Gọi CatalogRepository.UpdateProduct → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.UpdateProduct([Required(p["code"], "mã sản phẩm"), Required(p["name"], "tên"), Text(p["description"], 8000), Price(p["price"]), Positive(p["categoryId"]), Positive(p["brandId"]), id], c);
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (b.ContainsKey("image") && b.S("image") != oldImage)
            {
                // Tạo biến image để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var image = Image(b["image"]);
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (photo is not null && photo["variant"] is null)
                {
                    // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                    if (image != "")
                        // Gọi CatalogRepository.UpdateImage → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                        await repository.UpdateImage([image, photo.L("id")], c);
                    else
                        // Gọi CatalogRepository.RemoveImage → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                        await repository.RemoveImage([photo.L("id")], c);
                }
                else // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
 // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (image != "")
                {
                    // Gọi CatalogRepository.ClearPrimaryImage → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                    await repository.ClearPrimaryImage([id], c);
                    // Gọi CatalogRepository.InsertImage → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                    await repository.InsertImage([id, image], c);
                }
            }
        }

        // Gọi CatalogRepository.SetProductStatus → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.SetProductStatus([status, id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "SUA_SAN_PHAM", "SanPham", id, p.S("name"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Tạo biến thể có SKU/size/màu duy nhất và một dòng TonKho ban đầu bằng 0.
    public Task<object> CreateVariant(Row b) => db.Transaction<object>(async c =>
    {
        // Gọi CatalogRepository.InsertVariant → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var a = await repository.InsertVariant([Positive(b["productId"]), Required(b["sku"], "SKU"), Positive(b["sizeId"]), Positive(b["colorId"]), b.S("price") == "" ? null : Price(b["price"])], c);
        // Gọi CatalogRepository.CreateStock → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.CreateStock([a.InsertId], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "TAO_BIEN_THE", "BienTheSanPham", a.InsertId, b.S("sku"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            id = a.InsertId
        };
    });
    // Ẩn trước khi sửa; size/màu của biến thể đã giao dịch không được thay đổi.
    public Task<object> UpdateVariant(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến old để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var old = (await repository.LockVariant([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!b.ContainsKey("active") || !(b["active"] is bool) && b.N("active", -1)is not (0 or 1))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Trạng thái không hợp lệ.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (old.B("DangKinhDoanh") && new[]
        {
            "sizeId",
            "colorId",
            "price",
            "sku"
        }.Any(b.ContainsKey))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Hãy ẩn biến thể trước khi sửa.", 409);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.N("sizeId") > 0 && b.N("colorId") > 0)
        {
            // Tạo biến n để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var n = (await repository.CountVariantOrders([id], c)).One().L("n");
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (n > 0 && (b.L("sizeId") != old.L("KichCoID") || b.L("colorId") != old.L("MauSacID")))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Biến thể đã giao dịch không được đổi size/màu; hãy tạo mới.");
            // Gọi CatalogRepository.UpdateVariant → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.UpdateVariant([Positive(b["sizeId"]), Positive(b["colorId"]), b.S("price") == "" ? null : Price(b["price"]), id], c);
        }

        // Gọi CatalogRepository.SetVariantStatus → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.SetVariantStatus([b.B("active") ? 1 : 0, id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "AN_HIEN_BIEN_THE", "BienTheSanPham", id, b.S("active"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Xóa biến thể ẩn, hết tồn và không bị tham chiếu; FK của MySQL bảo vệ lịch sử.
    public Task<object> DeleteVariant(long id) => db.Transaction<object>(async c =>
    {
        // Tạo biến variant để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var variant = (await repository.LockVariantIdentity([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (variant.B("DangKinhDoanh"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Hãy ẩn biến thể trước khi xóa.", 409);
        // Tạo biến stock để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var stock = (await repository.LockStock([id], c)).FirstOrDefault();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (stock is not null && (stock.N("SoLuongTon") != 0 || stock.N("SoLuongDaPhanBo") != 0))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Biến thể còn tồn kho hoặc đang giữ hàng.", 409);
        // Gọi CatalogRepository.RemoveStock → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RemoveStock([id], c);
        // Gọi CatalogRepository.RemoveVariant → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RemoveVariant([id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XOA_BIEN_THE", "BienTheSanPham", id, variant.S("SKU"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Sản phẩm/danh mục/thương hiệu chỉ được xóa khi đã ẩn; bảng được Controller chọn từ danh sách cố định.
    public Task<object> DeleteItem(long id, string table, string col) => db.Transaction<object>(async c =>
    {
        // Tạo biến product để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var product = table == "SanPham";
        // Tạo biến flag để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var flag = product ? "TrangThai='AN'" : "DangHoatDong=0";
        // Tạo biến item để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var item = (await repository.LockHiddenItem(flag, table, col, [id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!item.B("draft"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Hãy ẩn mục này trước khi xóa.", 409);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (product)
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if ((await repository.ProductStock([id], c)).Any(x => x.N("SoLuongTon") != 0 || x.N("SoLuongDaPhanBo") != 0))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Sản phẩm còn tồn kho hoặc đang giữ hàng.", 409);
            // Gọi CatalogRepository.DeleteImages → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.DeleteImages([id], c);
            // Gọi CatalogRepository.DeleteStock → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.DeleteStock([id], c);
            // Gọi CatalogRepository.DeleteVariants → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.DeleteVariants([id], c);
        }

        // Gọi CatalogRepository.RemoveItem → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RemoveItem(table, col, [id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XOA_MUC", table, id, "Xóa mục đã ẩn", c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Tạo danh mục/thương hiệu; tên bảng do Controller cố định, dữ liệu luôn có tham số.
    public async Task<object> CreateTaxonomy(Row b, string table)
    {
        // Gán kết quả vào var(active,draft) để bước xử lý tiếp theo dùng dữ liệu mới.
        var(active, draft) = TaxonomyState(b, true);
        // Gọi CatalogRepository.CreateTaxonomy → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var result = await repository.CreateTaxonomy(table, [Required(b["name"], "tên"), Text(b["description"]), active, draft]);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "TAO_DANH_MUC", table, result.InsertId, b.S("name"));
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            id = result.InsertId
        };
    }

    // Sửa tên/trạng thái danh mục hoặc thương hiệu.
    public async Task<object> UpdateTaxonomy(long id, Row b, string table, string col)
    {
        // Gán kết quả vào var(active,draft) để bước xử lý tiếp theo dùng dữ liệu mới.
        var(active, draft) = TaxonomyState(b, false);
        // Gọi CatalogRepository.UpdateTaxonomy → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.UpdateTaxonomy(table, col, [Required(b["name"], "tên"), active, draft, id]);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "SUA_DANH_MUC", table, id, b.S("name"));
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    }

    // Chuẩn hóa trạng thái bản nháp cũ thành ẩn.
    private static string Status(string value) => value switch
    {
        "NHAP" => "AN",
        "AN" or "DANG_BAN" or "NGUNG_BAN" => value,
        _ => throw new ApiException("Trạng thái sản phẩm không hợp lệ.")};
    // Cờ active/draft tương thích schema hiện tại.
    private static (int, int) TaxonomyState(Row b, bool fallback)
    {
        // Tạo biến status để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var status = b.S("status", b.ContainsKey("active") ? (b.B("active") ? "HOAT_DONG" : "AN") : (fallback ? "HOAT_DONG" : "AN"));
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (status is not ("NHAP" or "HOAT_DONG" or "AN"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Trạng thái không hợp lệ.");
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return status == "HOAT_DONG" ? (1, 0) : (0, 1);
    }
}
