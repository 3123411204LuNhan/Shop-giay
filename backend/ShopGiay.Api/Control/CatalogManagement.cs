using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using MySqlConnector;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.CatalogQueries;

namespace ShopGiay.Control;

public sealed class CatalogManagement(Sql sql,ImageStore images)
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

    private static decimal Price(object value)
    {
        var price = Number(value);
        if (price <= 0 || price > 10000000000m) throw new ApiException("Giá tiền không hợp lệ.");
        return Math.Round(price,MidpointRounding.AwayFromZero);
    }

    private static string ProductStatus(string value) => value switch {
        "NHAP" => "AN",
        "AN" or "DANG_BAN" or "NGUNG_BAN" => value,
        _ => throw new ApiException("Trạng thái sản phẩm không hợp lệ.")
    };

    public async Task<object> CreateProduct(long actor,JsonElement body,Upload? image,CancellationToken token)
    {
        var name = body.Required("name","tên sản phẩm",250);
        return await ProductWrite(() => images.WithUpload("products",image,save => sql.Transaction<object>(async db => {
            var url = ImageStore.Url(body.Text("image",1001),true);
            var id = await db.Insert(Q.InsertProduct,body.Required("code","mã sản phẩm",50),name,body.Text("description",8000),
                Price(body.Field("price")),Id(body.Field("categoryId")),Id(body.Field("brandId")),
                ProductStatus(body.Text("status").NullIfEmpty() ?? "DANG_BAN"),actor);
            if (image is not null) url = "/api/product-images/" + await save();
            if (url.Length > 0) await db.Execute(Q.InsertImage,id,url);
            await db.Audit(actor,"TAO_SAN_PHAM","SanPham",id,name);
            return new { id };
        },token),token));
    }

    public async Task<object> UpdateProduct(long id,long actor,JsonElement body,Upload? image,CancellationToken token)
    {
        string? replaced = null;
        var result = await ProductWrite(() => images.WithUpload("products",image,save => sql.Transaction(async db => {
            var p = await db.One(Q.LockProduct,id) as IDictionary<string,object?> ?? throw new ApiException("Không tìm thấy sản phẩm.",404);
            var photo = await db.One(Q.LockImage,id);
            string original = photo is null ? "" : (string)photo.image;
            object? Field(string key) => body.TryGetProperty(key,out var value) ? value : p[key];
            var status = ProductStatus(Clean(Field("status")));
            var remove = body.Text("removeImage").Equals("true",StringComparison.OrdinalIgnoreCase);
            var url = remove ? "" : body.TryGetProperty("image",out _) ? ImageStore.Url(body.Text("image",1001),true) : original;
            var changed = image is not null || url != original ||
                new[] {"code","name","description","price","categoryId","brandId"}.Any(k => body.TryGetProperty(k,out _) && Clean(Field(k)) != Clean(p[k]));
            var name = Clean(Field("name"),250);
            if (changed) {
                var code = Clean(Field("code"),50);
                if (code == "" || name == "") throw new ApiException("Vui lòng nhập mã sản phẩm và tên.");
                await db.Execute(Q.UpdateProduct,code,name,Clean(Field("description"),8000),Price(Field("price")!),
                    Id(Field("categoryId")),Id(Field("brandId")),id);
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
            await db.Execute(Q.SetProductStatus,status,id);
            await db.Audit(actor,"SUA_SAN_PHAM","SanPham",id,name);
            return Ok;
        },token),token));
        images.RemoveProduct(replaced);
        return result;
    }

    private static async Task<object> ProductWrite(Func<Task<object>> write)
    {
        try { return await write(); }
        catch (MySqlException e) when (e.Number == 1062 && e.Message.Contains("MaSanPham")) {
            throw new ApiException("Mã sản phẩm đã tồn tại.",409);
        }
    }

    private static async Task<object> VariantWrite(Func<Task<object>> write)
    {
        try { return await write(); }
        catch (MySqlException e) when (e.Number == 1062) {
            throw new ApiException(e.Message.Contains("SKU") ? "SKU đã tồn tại." : "Sản phẩm đã có biến thể với size và màu này.",409);
        }
    }

    private static decimal? VariantPrice(JsonElement body) => body.Text("price") is "" or "0" ? null : Price(body.Field("price"));

    public Task<object> CreateVariant(long actor,JsonElement body,CancellationToken token) => VariantWrite(() =>
        sql.Transaction<object>(async db => {
            var sku = body.Required("sku","SKU",80);
            var id = await db.Insert(Q.InsertVariant,Id(body.Field("productId")),sku,Id(body.Field("sizeId")),Id(body.Field("colorId")),VariantPrice(body));
            await db.Execute(Q.CreateStock,id);
            await db.Audit(actor,"TAO_BIEN_THE","BienTheSanPham",id,sku);
            return new { id };
        },token));

    public Task<object> UpdateVariant(long id,long actor,JsonElement body,CancellationToken token) => VariantWrite(() =>
        sql.Transaction(async db => {
            var old = await db.One(Q.LockVariant,id) ?? throw new ApiException("Không tìm thấy biến thể.",404);
            var active = body.Field("active").ValueKind switch {
                JsonValueKind.True => 1, JsonValueKind.False => 0, _ => Integer(body.Field("active"),0,1,"Trạng thái không hợp lệ.")
            };
            if (Convert.ToBoolean(old.DangKinhDoanh) && new[] {"sizeId","colorId","price","sku"}.Any(k=>body.TryGetProperty(k,out _)))
                throw new ApiException("Hãy ẩn biến thể trước khi sửa.",409);
            if (body.Text("sizeId").Length > 0 && body.Text("colorId").Length > 0) {
                var size = Id(body.Field("sizeId"));
                var color = Id(body.Field("colorId"));
                if (await db.Value<long>(Q.CountVariantOrders,id) > 0 && (size != (int)old.KichCoID || color != (int)old.MauSacID))
                    throw new ApiException("Biến thể đã giao dịch không được đổi size/màu. Hãy tạo biến thể mới.");
                await db.Execute(Q.UpdateVariant,size,color,VariantPrice(body),id);
            }
            await db.Execute(Q.SetVariantStatus,active,id);
            await db.Audit(actor,"AN_HIEN_BIEN_THE","BienTheSanPham",id,active == 0 ? "false" : "true");
            return Ok;
        },token));

    public Task<object> DeleteVariant(long id,long actor,CancellationToken token) =>
        sql.Transaction(async db => {
            var variant = await db.One(Q.LockVariantIdentity,id) ?? throw new ApiException("Không tìm thấy biến thể.",404);
            if (Convert.ToBoolean(variant.DangKinhDoanh)) throw new ApiException("Hãy ẩn biến thể trước khi xóa.",409);
            var stock = await db.One(Q.LockStock,id);
            if (stock is not null && ((int)stock.SoLuongTon != 0 || (int)stock.SoLuongDaPhanBo != 0))
                throw new ApiException("Biến thể còn tồn kho hoặc đang giữ hàng.",409);
            await db.Execute(Q.RemoveStock,id);
            await db.Execute(Q.RemoveVariant,id);
            await db.Audit(actor,"XOA_BIEN_THE","BienTheSanPham",id,(string)variant.SKU);
            return Ok;
        },token);

    private static (string Table,string Column,string Name) Taxonomy(string key) => key switch {
        "categories" => ("DanhMuc","DanhMucID","TenDanhMuc"), "brands" => ("ThuongHieu","ThuongHieuID","TenThuongHieu"),
        _ => throw new ApiException("Không tìm thấy mục.",404)
    };

    public Task<object> SaveTaxonomy(string key,long? id,long actor,JsonElement body,CancellationToken token)
    {
        var (table,column,nameColumn) = Taxonomy(key);
        var name = body.Required("name","tên");
        var status = body.Text("status").NullIfEmpty() ?? (body.IsSet("active")
            ? body.Text("active") is "true" or "True" or "1" ? "HOAT_DONG" : "AN"
            : id is null ? "HOAT_DONG" : "AN");
        if (status is not ("NHAP" or "HOAT_DONG" or "AN")) throw new ApiException("Trạng thái không hợp lệ.");
        var active = status == "HOAT_DONG";
        return sql.Transaction<object>(async db => {
            if (id is null) {
                var created = await db.Insert($"INSERT INTO {table}({nameColumn},MoTa,DangHoatDong,LaBanNhap) VALUES(?,?,?,?)",
                    name,body.Text("description"),active,!active);
                await db.Audit(actor,"TAO_DANH_MUC",table,created,name);
                return new { id = created };
            }
            await db.Execute($"UPDATE {table} SET {nameColumn}=?,DangHoatDong=?,LaBanNhap=?,NgayCapNhat=UTC_TIMESTAMP() WHERE {column}=?",
                name,active,!active,id);
            await db.Audit(actor,"SUA_DANH_MUC",table,id.Value,name);
            return Ok;
        },token);
    }

    public async Task<object> DeleteItem(string key,long id,long actor,CancellationToken token)
    {
        var product = key == "products";
        var (table,column,_) = product ? ("SanPham","SanPhamID","TenSanPham") : Taxonomy(key);
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
