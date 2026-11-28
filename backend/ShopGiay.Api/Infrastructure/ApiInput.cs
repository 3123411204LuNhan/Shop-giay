using System.Globalization;
using System.Text.Json;

namespace ShopGiay.Infrastructure;

public sealed class ApiException(string message,int status = 400,string? code = null) : Exception(message)
{
    public int Status { get; } = status;
    public string? Code { get; } = code;
}

public static class ApiInput
{
    public static readonly object Ok = new { ok = true };
    public static string Text(this JsonElement body,string key,int limit = 1000) =>
        Clean(body.Field(key),limit);
    public static string Clean(object? value,int limit = 1000)
    {
        var raw = value is JsonElement { ValueKind: JsonValueKind.Null or JsonValueKind.Undefined }
            ? "" : (value is IFormattable formatted
                ? formatted.ToString(null,CultureInfo.InvariantCulture)
                : value?.ToString())?.Trim() ?? "";
        return raw[..Math.Min(raw.Length,limit)];
    }
    public static string? NullIfEmpty(this string value) => value.Length == 0 ? null : value;
    public static string Required(this JsonElement body,string key,string label,int limit = 1000) =>
        body.Text(key,limit).NullIfEmpty() ?? throw new ApiException($"Vui lòng nhập {label}.");
    public static decimal Number(object? value) => decimal.TryParse(Clean(value),NumberStyles.Float,CultureInfo.InvariantCulture,out var n)
        ? n : throw new ApiException("Giá trị số không hợp lệ.");
    public static long Integer(object? value,long min,long max,string message = "Số lượng không hợp lệ.")
    {
        if (!decimal.TryParse(Clean(value),NumberStyles.Float,CultureInfo.InvariantCulture,out var n)
            || n < min || n > max || n != decimal.Truncate(n)) throw new ApiException(message);
        return (long)n;
    }
    public static long Id(object? value) => Integer(value,1,100000);
    public static JsonElement Field(this JsonElement body,string key) =>
        body.ValueKind == JsonValueKind.Object && body.TryGetProperty(key,out var v) ? v : default;
    public static string? String(this JsonElement body,string key) =>
        body.Field(key) is { ValueKind: JsonValueKind.String } v ? v.GetString() : null;
    public static JsonElement[] Items(this JsonElement body) => body.Field("items") is { ValueKind: JsonValueKind.Array } list
        ? list.EnumerateArray().ToArray() : [];
    public static bool IsSet(this JsonElement body,string key) => body.Field(key).ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined);
    public static string RequestKey(this JsonElement body) => Guid.TryParseExact(body.Text("requestId"),"D",out _)
        ? body.Text("requestId") : throw new ApiException("Mã chống lặp không hợp lệ.");
}
