using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using MySqlConnector;
using ShopGiay.Entity;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.CatalogQueries;

namespace ShopGiay.Control;

public sealed class CatalogManagement(Sql sql,ImageStore images,CatalogRepository catalog)
{
    public Task<List<dynamic>> Products(CancellationToken token) => sql.Run(db => db.Rows(Q.Products),token);
    public Task<List<dynamic>> Variants(string? productId,CancellationToken token) =>
        sql.Run(db => db.Rows(Q.Variants.Replace("{slot0}",productId is null ? "" : "WHERE b.SanPhamID=?"),
            productId is null ? [] : [Id(productId)]),token);

    public async Task<object> Lookups(CancellationToken token) => await sql.Run(async db => new {
        categories = await db.Rows(Q.AllCategories), brands = await db.Rows(Q.AllBrands),
        sizes = await db.Rows(Q.Sizes), colors = await db.Rows(Q.AllColors)
    },token);

    public Task<object> Size(JsonElement body,CancellationToken token)
    {
        var raw = body.Text("name").Replace(',','.');
        if (!Regex.IsMatch(raw,@"\A[0-9]{1,3}(\.[0-9]{1,2})?\z") || Number(raw) <= 0)
            throw new ApiException("Size EU phải là số dương, ví dụ 42 hoặc 42.5.");
        var number = Number(raw);
        var value = number.ToString("0.##",CultureInfo.InvariantCulture);
        return sql.Transaction<object>(async db => {
            await db.Execute(Q.AddSize,value,decimal.Floor(number));
            return (object)(await db.One(Q.SizeByValue,value))!;
        },token);
    }

    public Task<object> Color(JsonElement body,CancellationToken token)
    {
        var name = Regex.Replace(body.Text("name"),@"\s+"," ");
        var hex = body.Text("hex");
        if (name.Length is < 1 or > 100) throw new ApiException("Tên màu phải từ 1 đến 100 ký tự.");
        if (hex.Length > 0 && !Regex.IsMatch(hex,@"\A#[a-fA-F0-9]{6}\z")) throw new ApiException("Mã màu cần có dạng #RRGGBB.");
        return sql.Transaction<object>(async db => {
            await db.Execute(Q.AddColor,name,hex.NullIfEmpty());
            return (object)(await db.One(Q.ColorByName,name))!;
        },token);
    }

    // Kiểm tra giá nhập rồi làm tròn theo đơn vị VNĐ trước khi lưu.
    private static decimal Price(object value) => SanPham.ChuanHoaGia(Number(value));

    // Dựng sản phẩm, kiểm tra trạng thái, lưu rồi thêm ảnh và nhật ký.
    public async Task<object> CreateProduct(long actor,JsonElement body,Upload? image,CancellationToken token)
    {
        var name = body.Required("name","tên sản phẩm",250);
        return await ProductWrite(() => images.WithUpload("products",image,save => sql.Transaction<object>(async db => {
            var url = ImageStore.Url(body.Text("image",1001),true);
            var product = new SanPham { NguoiTaoID = actor };
            product.CapNhatThongTin(body.Required("code","mã sản phẩm",50),name,body.Text("description",8000),
                Price(body.Field("price")),(int)Id(body.Field("categoryId")),(int)Id(body.Field("brandId")));
            product.DatTrangThai(body.Text("status").NullIfEmpty() ?? "DANG_BAN");
            var id = await catalog.ThemSanPham(db,product);
            if (image is not null) url = "/api/product-images/" + await save();
            if (url.Length > 0) await db.Execute(Q.InsertImage,id,url);
            await db.Audit(actor,"TAO_SAN_PHAM","SanPham",id,name);
            return new { id };
        },token),token));
    }

    // Khóa sản phẩm, hợp nhất dữ liệu mới, lưu qua Entity rồi dọn ảnh cũ sau khi commit.
    public async Task<object> UpdateProduct(long id,long actor,JsonElement body,Upload? image,CancellationToken token)
    {
        string? replaced = null;
        var result = await ProductWrite(() => images.WithUpload("products",image,save => sql.Transaction(async db => {
            var p = await catalog.KhoaSanPham(db,id) ?? throw new ApiException("Không tìm thấy sản phẩm.",404);
            var photo = await db.One(Q.LockImage,id);
            string original = photo is null ? "" : (string)photo.image;
            // Giữ dữ liệu đang lưu nếu yêu cầu chỉ cập nhật một phần sản phẩm.
            object? Field(string key,object? current) => body.TryGetProperty(key,out var value) ? value : current;
            p.DatTrangThai(Clean(Field("status",p.TrangThai)));
            var remove = body.Text("removeImage").Equals("true",StringComparison.OrdinalIgnoreCase);
            var url = remove ? "" : body.TryGetProperty("image",out _) ? ImageStore.Url(body.Text("image",1001),true) : original;
            (string Key,object? Value)[] fields = [("code",p.MaSanPham),("name",p.TenSanPham),("description",p.MoTa),
                ("price",p.GiaBan),("categoryId",p.DanhMucID),("brandId",p.ThuongHieuID)];
            var changed = image is not null || url != original || fields.Any(f => Clean(Field(f.Key,f.Value)) != Clean(f.Value));
            var name = Clean(Field("name",p.TenSanPham),250);
            if (changed) {
                var code = Clean(Field("code",p.MaSanPham),50);
                if (code == "" || name == "") throw new ApiException("Vui lòng nhập mã sản phẩm và tên.");
                p.CapNhatThongTin(code,name,Clean(Field("description",p.MoTa),8000),Price(Field("price",p.GiaBan)!),
                    (int)Id(Field("categoryId",p.DanhMucID)),(int)Id(Field("brandId",p.ThuongHieuID)));
                await catalog.LuuThongTin(db,p);
                if (image is not null) url = "/api/product-images/" + await save();
                if (url != original) {
                    if (photo is not null && photo.variant is null) {
                        replaced = original;
                        if (url.Length > 0) await db.Execute(Q.UpdateImage,url,(long)photo.id);
                        else await db.Execute(Q.RemoveImage,(long)photo.id);
                    } else if (url.Length > 0) {
                        await db.Execute(Q.ClearPrimaryImage,id);
                        await db.Execute(Q.InsertImage,id,url);
                    }
                }
            }
            await catalog.LuuTrangThai(db,p);
            await db.Audit(actor,"SUA_SAN_PHAM","SanPham",id,name);
            return Ok;
        },token),token));
        images.RemoveProduct(replaced);
        return result;
    }

    // Dịch lỗi trùng mã sản phẩm từ MySQL để giao diện báo đúng trường.
    private static async Task<object> ProductWrite(Func<Task<object>> write)
    {
        try { return await write(); }
        catch (MySqlException e) when (e.Number == 1062 && e.Message.Contains("MaSanPham")) {
            throw new ApiException("Mã sản phẩm đã tồn tại.",409);
        }
    }

    // Dịch lỗi trùng SKU hoặc trùng tổ hợp size và màu.
    private static async Task<object> VariantWrite(Func<Task<object>> write)
    {
        try { return await write(); }
        catch (MySqlException e) when (e.Number == 1062) {
            throw new ApiException(e.Message.Contains("SKU") ? "SKU đã tồn tại." : "Sản phẩm đã có biến thể với size và màu này.",409);
        }
    }

    // Giá bỏ trống hoặc bằng 0 dùng giá của sản phẩm.
    private static decimal? VariantPrice(JsonElement body) => body.Text("price") is "" or "0" ? null : Price(body.Field("price"));

    // Dựng biến thể, lưu tổ hợp size và màu, sau đó tạo dòng tồn và nhật ký.
    public Task<object> CreateVariant(long actor,JsonElement body,CancellationToken token) => VariantWrite(() =>
        sql.Transaction<object>(async db => {
            var sku = body.Required("sku","SKU",80);
            var variant = new BienTheSanPham { SanPhamID = Id(body.Field("productId")), SKU = sku };
            variant.CapNhatThongTin((int)Id(body.Field("sizeId")),(int)Id(body.Field("colorId")),VariantPrice(body),false);
            var id = await catalog.ThemBienThe(db,variant);
            await db.Execute(Q.CreateStock,id);
            await db.Audit(actor,"TAO_BIEN_THE","BienTheSanPham",id,sku);
            return new { id };
        },token));

    // Khóa biến thể, kiểm tra điều kiện sửa và giao dịch cũ rồi lưu giá, size, màu và trạng thái bán.
    public Task<object> UpdateVariant(long id,long actor,JsonElement body,CancellationToken token) => VariantWrite(() =>
        sql.Transaction(async db => {
            var old = await catalog.KhoaBienThe(db,id) ?? throw new ApiException("Không tìm thấy biến thể.",404);
            var active = body.Field("active").ValueKind switch {
                JsonValueKind.True => 1, JsonValueKind.False => 0, _ => Integer(body.Field("active"),0,1,"Trạng thái không hợp lệ.")
            };
            if (old.DangKinhDoanh && new[] {"sizeId","colorId","price","sku"}.Any(k=>body.TryGetProperty(k,out _)))
                throw new ApiException("Hãy ẩn biến thể trước khi sửa.",409);
            if (body.Text("sizeId").Length > 0 && body.Text("colorId").Length > 0) {
                var size = Id(body.Field("sizeId"));
                var color = Id(body.Field("colorId"));
                var daGiaoDich = await db.Value<long>(Q.CountVariantOrders,id) > 0;
                old.CapNhatThongTin((int)size,(int)color,VariantPrice(body),daGiaoDich);
                await catalog.LuuThongTin(db,old);
            }
            old.DatKinhDoanh(active == 1);
            await catalog.LuuTrangThai(db,old);
            await db.Audit(actor,"AN_HIEN_BIEN_THE","BienTheSanPham",id,active == 0 ? "false" : "true");
            return Ok;
        },token));

    // Chỉ xóa biến thể đã ẩn và đủ điều kiện về tồn, rồi ghi nhật ký.
    public Task<object> DeleteVariant(long id,long actor,CancellationToken token) =>
        sql.Transaction(async db => {
            var variant = await catalog.KhoaBienThe(db,id) ?? throw new ApiException("Không tìm thấy biến thể.",404);
            if (variant.DangKinhDoanh) throw new ApiException("Hãy ẩn biến thể trước khi xóa.",409);
            var stock = await db.One(Q.LockStock,id);
            if (stock is not null && ((int)stock.SoLuongTon != 0 || (int)stock.SoLuongDaPhanBo != 0))
                throw new ApiException("Biến thể còn tồn kho hoặc đang giữ hàng.",409);
            await db.Execute(Q.RemoveStock,id);
            await db.Execute(Q.RemoveVariant,id);
            await db.Audit(actor,"XOA_BIEN_THE","BienTheSanPham",id,variant.SKU);
            return Ok;
        },token);

    // Chỉ ánh xạ các nhóm phân loại được phép truy cập.
    private static (string Table,string Column) Taxonomy(string key) => key switch {
        "categories" => ("DanhMuc","DanhMucID"), "brands" => ("ThuongHieu","ThuongHieuID"),
        _ => throw new ApiException("Không tìm thấy mục.",404)
    };

    // Chuẩn hóa trạng thái, tạo hoặc cập nhật Entity phân loại rồi ghi nhật ký.
    public Task<object> SaveTaxonomy(string key,long? id,long actor,JsonElement body,CancellationToken token)
    {
        var (table,_) = Taxonomy(key);
        var name = body.Required("name","tên");
        var status = body.Text("status").NullIfEmpty() ?? (body.IsSet("active")
            ? body.Text("active") is "true" or "True" or "1" ? "HOAT_DONG" : "AN"
            : id is null ? "HOAT_DONG" : "AN");
        if (status is not ("NHAP" or "HOAT_DONG" or "AN")) throw new ApiException("Trạng thái không hợp lệ.");
        var active = status == "HOAT_DONG";
        return sql.Transaction<object>(async db => {
            var description = body.Text("description");
            long saved;
            if (key == "categories") {
                var category = new DanhMuc { DanhMucID = (int)(id ?? 0), MoTa = description };
                category.CapNhat(name,active);
                saved = await catalog.LuuDanhMuc(db,category);
            } else {
                var brand = new ThuongHieu { ThuongHieuID = (int)(id ?? 0), MoTa = description };
                brand.CapNhat(name,active);
                saved = await catalog.LuuThuongHieu(db,brand);
            }
            await db.Audit(actor,id is null ? "TAO_DANH_MUC" : "SUA_DANH_MUC",table,saved,name);
            return id is null ? new { id = saved } : Ok;
        },token);
    }

    // Kiểm tra trạng thái ẩn và điều kiện tồn trước khi xóa dữ liệu, sau đó dọn ảnh.
    public async Task<object> DeleteItem(string key,long id,long actor,CancellationToken token)
    {
        var product = key == "products";
        var (table,column) = product ? ("SanPham","SanPhamID") : Taxonomy(key);
        var flag = product ? "TrangThai='AN'" : "DangHoatDong=0";
        var urls = await sql.Transaction(async db => {
            var hidden = await db.Value<int?>($"SELECT ({flag}) FROM {table} WHERE {column}=? FOR UPDATE",id);
            if (hidden is null) throw new ApiException("Không tìm thấy mục cần xóa.",404);
            if (hidden == 0) throw new ApiException(product ? "Hãy ẩn sản phẩm trước khi xóa." : "Hãy ẩn mục này trước khi xóa.",409);
            var saved = new List<string>();
            if (product) {
                var stock = await db.Rows(Q.ProductStock,id);
                if (stock.Any(k=>(int)k.SoLuongTon != 0 || (int)k.SoLuongDaPhanBo != 0))
                    throw new ApiException("Sản phẩm còn tồn kho hoặc đang giữ hàng.",409);
                saved.AddRange((await db.Rows(Q.ProductImages,id)).Select(x=>(string)x.url));
                await db.Execute(Q.DeleteImages,id);
                await db.Execute(Q.DeleteStock,id);
                await db.Execute(Q.DeleteVariants,id);
            }
            await db.Execute($"DELETE FROM {table} WHERE {column}=?",id);
            await db.Audit(actor,product ? "XOA_SAN_PHAM" : "XOA_BAN_NHAP",table,id,product ? "Xóa sản phẩm ẩn" : "Xóa bản nháp");
            return saved;
        },token);
        foreach (var url in urls) images.RemoveProduct(url);
        return Ok;
    }
}
