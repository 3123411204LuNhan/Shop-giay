using Org.BouncyCastle.Crypto.Generators;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ShopGiay.Infrastructure;

public static class Password
{
    public static string Token() => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
    public static bool ValidToken(string? value) => value is not null && Regex.IsMatch(value,@"\A[a-f0-9]{64}\z");
    public static byte[] TokenHash(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
    public static bool ValidEmail(string value) => value.Length <= 254 && Regex.IsMatch(value,@"\A[^\s@]+@[^\s@]+\.[^\s@]+\z");
    private static byte[] Derive(string value,string salt) =>
        SCrypt.Generate(Encoding.UTF8.GetBytes(value),Encoding.UTF8.GetBytes(salt),16384,8,1,64);

    public static string Hash(string value)
    {
        var salt = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
        return salt + ":" + Convert.ToHexStringLower(Derive(value,salt));
    }

    public static bool Verify(string value,string hash)
    {
        var parts = hash.Split(':');
        return value.Length <= 128 && parts.Length == 2 &&
            Regex.IsMatch(parts[0],@"\A[a-f0-9]{32}\z") && Regex.IsMatch(parts[1],@"\A[a-f0-9]{128}\z") &&
            CryptographicOperations.FixedTimeEquals(Derive(value,parts[0]),Convert.FromHexString(parts[1]));
    }

    public static string Validate(string? value) => value is { Length: >= 8 and <= 128 } ? value
        : throw new ApiException("Mật khẩu cần từ 8 đến 128 ký tự.");
}
