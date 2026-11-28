using System.Buffers.Binary;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ShopGiay.Infrastructure;

public sealed record Upload(byte[] Bytes,string Extension);

public sealed class ImageStore(IConfiguration config,ILogger<ImageStore> log)
{
    private const int Limit = 5 * 1024 * 1024;
    private static readonly Regex Filename = new(@"\A[0-9a-f-]{36}\.(png|jpg|webp)\z");
    private static readonly byte[] Png = Convert.FromHexString("89504e470d0a1a0a");

    private string Root {
        get {
            var root = Path.GetFullPath(config["IMAGE_UPLOAD_DIR"] ?? ".local/uploads");
            var relative = Path.GetRelativePath(Directory.GetCurrentDirectory(),root);
            if (!Path.IsPathRooted(relative) && !relative.StartsWith(".." + Path.DirectorySeparatorChar) &&
                !relative.StartsWith(".local" + Path.DirectorySeparatorChar))
                throw new InvalidOperationException("IMAGE_UPLOAD_DIR phải nằm trong .local hoặc ngoài thư mục dự án.");
            return root;
        }
    }

    public static async Task<(JsonElement Body,Upload? Image)> Read(HttpRequest request,CancellationToken token)
    {
        if (!request.HasFormContentType)
            return (await request.ReadFromJsonAsync<JsonElement>(token),null);
        if (!request.ContentType!.StartsWith("multipart/form-data",StringComparison.OrdinalIgnoreCase))
            throw new ApiException("Dữ liệu tải ảnh không hợp lệ.");
        if (request.ContentLength > Limit + 64 * 1024) throw new ApiException("Ảnh vượt quá 5 MB.",413);
        IFormCollection form;
        try { form = await request.ReadFormAsync(token); }
        catch (InvalidDataException) { throw new ApiException("Dữ liệu tải ảnh không hợp lệ."); }
        if (form.Files.Count > 1 || form.Any(field => field.Value.Count != 1 || field.Key == "imageFile"))
            throw new ApiException("Mỗi yêu cầu chỉ được có một ảnh và một giá trị cho mỗi trường.");
        var fields = JsonSerializer.SerializeToElement(form.ToDictionary(x=>x.Key,x=>x.Value.ToString()));
        var file = form.Files.FirstOrDefault();
        if (file is null) return (fields,null);
        if (file.Name != "imageFile") throw new ApiException("Dữ liệu tải ảnh không hợp lệ.");
        if (file.Length > Limit) throw new ApiException("Ảnh vượt quá 5 MB.",413);
        if (fields.Text("image") != "" || fields.Text("removeImage").Equals("true",StringComparison.OrdinalIgnoreCase))
            throw new ApiException("Hãy chọn một tệp ảnh hoặc một URL.");
        using var buffer = new MemoryStream((int)file.Length);
        await file.CopyToAsync(buffer,token);
        var bytes = buffer.ToArray();
        return (fields,new Upload(bytes,Extension(bytes)));
    }

    private static string Extension(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 33 && bytes.StartsWith(Png) && BinaryPrimitives.ReadUInt32BigEndian(bytes[8..]) == 13 &&
            bytes[12..16].SequenceEqual("IHDR"u8) && bytes[^8..^4].SequenceEqual("IEND"u8)) return "png";
        if (bytes.Length >= 12 && bytes.StartsWith(new byte[] {255,216,255}) && bytes[^2] == 255 && bytes[^1] == 217) return "jpg";
        if (bytes.Length >= 20 && bytes.StartsWith("RIFF"u8) && BinaryPrimitives.ReadUInt32LittleEndian(bytes[4..]) == bytes.Length - 8 &&
            bytes[8..12].SequenceEqual("WEBP"u8) &&
            (bytes[12..16].SequenceEqual("VP8 "u8) || bytes[12..16].SequenceEqual("VP8L"u8) || bytes[12..16].SequenceEqual("VP8X"u8))) return "webp";
        throw new ApiException("Tệp phải là ảnh JPEG, PNG hoặc WebP.",415);
    }

    public static string Url(string raw,bool product)
    {
        if (raw.Length == 0) return "";
        if (raw.Length <= 1000 && !raw.Any(char.IsWhiteSpace)) {
            if (product && raw.StartsWith("/images/") && !raw.Any(c => c is '?' or '#' or '\\') && !raw.Split('/').Contains(".."))
                return raw;
            if (Uri.TryCreate(raw,UriKind.Absolute,out var uri) && uri.Scheme is "http" or "https" && uri.UserInfo == "") return raw;
        }
        throw new ApiException("Nhập URL ảnh HTTP(S) hợp lệ.");
    }

    public async Task<T> WithUpload<T>(string scope,Upload? upload,Func<Func<Task<string>>,Task<T>> work,CancellationToken token)
    {
        string? name = null;
        try {
            return await work(async () => {
                if (upload is null) throw new InvalidOperationException("Thiếu tệp ảnh.");
                var directory = Path.Combine(Root,scope);
                Directory.CreateDirectory(directory);
                var candidate = Guid.NewGuid() + "." + upload.Extension;
                await using var stream = new FileStream(Path.Combine(directory,candidate),FileMode.CreateNew,FileAccess.Write);
                name = candidate;
                await stream.WriteAsync(upload.Bytes,token);
                return candidate;
            });
        } catch {
            if (name is not null) Remove(scope,name);
            throw;
        }
    }

    public void RemoveProduct(string? url)
    {
        const string prefix = "/api/product-images/";
        if (url?.StartsWith(prefix) == true) Remove("products",url[prefix.Length..]);
    }

    private void Remove(string scope,string name)
    {
        if (!Filename.IsMatch(name)) return;
        try { File.Delete(Path.Combine(Root,scope,name)); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) {
            log.LogWarning("Không dọn được ảnh {FileName}.",name);
        }
    }

    public (string Path,string ContentType) Locate(string scope,string name)
    {
        if (!Filename.IsMatch(name)) throw new ApiException("Không tìm thấy ảnh.",404);
        var path = Path.Combine(Root,scope,name);
        if (!File.Exists(path)) throw new ApiException("Không tìm thấy ảnh.",404);
        return (path,Path.GetExtension(name) switch { ".png" => "image/png", ".jpg" => "image/jpeg", _ => "image/webp" });
    }
}
