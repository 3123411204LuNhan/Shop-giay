using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace ShopGiay.Web.Infrastructure;
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string stored);
}

// Mật khẩu mới dùng PBKDF2 của .NET; tài khoản Node.js cũ vẫn xác minh được scrypt N=16384,r=8,p=1.
public sealed class PasswordHasher : IPasswordHasher
{
    // AuthService/UserService → Hash; salt riêng cho mỗi mật khẩu.
    public string Hash(string password)
    {
        // Tạo biến salt để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var salt = RandomNumberGenerator.GetBytes(16);
        // Tạo biến key để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210000, HashAlgorithmName.SHA512, 64);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return $"pbkdf2-sha512:210000:{Convert.ToHexString(salt)}:{Convert.ToHexString(key)}";
    }

    // Không đổi dữ liệu tài khoản cũ; so sánh khóa theo thời gian cố định.
    public bool Verify(string password, string stored)
    {
        // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
        try
        {
            // Tạo biến parts để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var parts = stored.Split(':');
            // Tạo biến actual, expected để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            byte[] actual, expected;
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (parts.Length == 4 && parts[0] == "pbkdf2-sha512")
            {
                // Tạo biến rounds để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var rounds = int.Parse(parts[1]);
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (rounds is < 100000 or > 1000000)
                    // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                    return false;
                // Gán kết quả vào expected để bước xử lý tiếp theo dùng dữ liệu mới.
                expected = Convert.FromHexString(parts[3]);
                // Gán kết quả vào actual để bước xử lý tiếp theo dùng dữ liệu mới.
                actual = Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromHexString(parts[2]), rounds, HashAlgorithmName.SHA512, 64);
            }
            else // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
 // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (parts.Length == 2 && parts[0].Length == 32 && parts[1].Length == 128)
            {
                // Gán kết quả vào expected để bước xử lý tiếp theo dùng dữ liệu mới.
                expected = Convert.FromHexString(parts[1]);
                // Gán kết quả vào actual để bước xử lý tiếp theo dùng dữ liệu mới.
                actual = LegacyScrypt(password, parts[0]);
            }
            else
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return false;
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return false;
        }
    }

    // Tương thích node:crypto.scryptSync(password, saltHexString, 64), theo RFC 7914.
    private static byte[] LegacyScrypt(string password, string salt)
    {
        // Tạo biến input để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var input = Encoding.UTF8.GetBytes(password);
        // Tạo biến b để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var b = Rfc2898DeriveBytes.Pbkdf2(input, Encoding.UTF8.GetBytes(salt), 1, HashAlgorithmName.SHA256, 1024);
        // Tạo biến x để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var x = new uint[256];
        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < x.Length; i++)
            // Gán kết quả vào x[i] để bước xử lý tiếp theo dùng dữ liệu mới.
            x[i] = BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(i * 4));
        // Tạo biến v để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var v = new uint[16384 * 256];
        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < 16384; i++)
        {
            // Thực hiện bước x.CopyTo trong PasswordHasher; tiếp tục theo luồng của hàm.
            x.CopyTo(v, i * 256);
            // Thực hiện bước BlockMix trong PasswordHasher; tiếp tục theo luồng của hàm.
            BlockMix(x);
        }

        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < 16384; i++)
        {
            // Tạo biến index để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var index = (int)(x[240] & 16383) * 256;
            // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
            for (var k = 0; k < 256; k++)
                // Gán kết quả vào x[k] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[k] ^= v[index + k];
            // Thực hiện bước BlockMix trong PasswordHasher; tiếp tục theo luồng của hàm.
            BlockMix(x);
        }

        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < x.Length; i++)
            // Thực hiện bước BinaryPrimitives.WriteUInt32LittleEndian trong PasswordHasher; tiếp tục theo luồng của hàm.
            BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(i * 4), x[i]);
        // Tạo biến result để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var result = Rfc2898DeriveBytes.Pbkdf2(input, b, 1, HashAlgorithmName.SHA256, 64);
        // Thực hiện bước CryptographicOperations.ZeroMemory trong PasswordHasher; tiếp tục theo luồng của hàm.
        CryptographicOperations.ZeroMemory(b);
        // Thực hiện bước Array.Clear trong PasswordHasher; tiếp tục theo luồng của hàm.
        Array.Clear(v);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return result;
    }

    // BlockMix r=8 sắp xếp các block chẵn rồi lẻ theo chuẩn scrypt.
    private static void BlockMix(uint[] b)
    {
        // Tạo biến x để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        Span<uint> x = stackalloc uint[16];
        // Tạo biến y để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        Span<uint> y = stackalloc uint[256];
        // Thực hiện bước b.AsSpan(240, 16).CopyTo trong PasswordHasher; tiếp tục theo luồng của hàm.
        b.AsSpan(240, 16).CopyTo(x);
        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < 16; i++)
        {
            // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
            for (var k = 0; k < 16; k++)
                // Gán kết quả vào x[k] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[k] ^= b[i * 16 + k];
            // Thực hiện bước Salsa trong PasswordHasher; tiếp tục theo luồng của hàm.
            Salsa(x);
            // Thực hiện bước x.CopyTo trong PasswordHasher; tiếp tục theo luồng của hàm.
            x.CopyTo(y.Slice(i * 16, 16));
        }

        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < 8; i++)
        {
            // Thực hiện bước y.Slice(i * 32, 16).CopyTo trong PasswordHasher; tiếp tục theo luồng của hàm.
            y.Slice(i * 32, 16).CopyTo(b.AsSpan(i * 16));
            // Thực hiện bước y.Slice(i * 32 + 16, 16).CopyTo trong PasswordHasher; tiếp tục theo luồng của hàm.
            y.Slice(i * 32 + 16, 16).CopyTo(b.AsSpan((i + 8) * 16));
        }
    }

    // Hàm vòng Salsa20/8 của RFC 7914; unchecked giữ phép cộng modulo 2^32.
    private static void Salsa(Span<uint> b)
    {
        // Tạo biến x để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        Span<uint> x = stackalloc uint[16];
        // Thực hiện bước b.CopyTo trong PasswordHasher; tiếp tục theo luồng của hàm.
        b.CopyTo(x);
        // Thực hiện bước R trong PasswordHasher; tiếp tục theo luồng của hàm.
        unchecked
        {
            // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
            for (var i = 0; i < 8; i += 2)
            {
                // Gán kết quả vào x[4] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[4] ^= R(x[0] + x[12], 7);
                // Gán kết quả vào x[8] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[8] ^= R(x[4] + x[0], 9);
                // Gán kết quả vào x[12] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[12] ^= R(x[8] + x[4], 13);
                // Gán kết quả vào x[0] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[0] ^= R(x[12] + x[8], 18);
                // Gán kết quả vào x[9] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[9] ^= R(x[5] + x[1], 7);
                // Gán kết quả vào x[13] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[13] ^= R(x[9] + x[5], 9);
                // Gán kết quả vào x[1] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[1] ^= R(x[13] + x[9], 13);
                // Gán kết quả vào x[5] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[5] ^= R(x[1] + x[13], 18);
                // Gán kết quả vào x[14] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[14] ^= R(x[10] + x[6], 7);
                // Gán kết quả vào x[2] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[2] ^= R(x[14] + x[10], 9);
                // Gán kết quả vào x[6] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[6] ^= R(x[2] + x[14], 13);
                // Gán kết quả vào x[10] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[10] ^= R(x[6] + x[2], 18);
                // Gán kết quả vào x[3] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[3] ^= R(x[15] + x[11], 7);
                // Gán kết quả vào x[7] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[7] ^= R(x[3] + x[15], 9);
                // Gán kết quả vào x[11] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[11] ^= R(x[7] + x[3], 13);
                // Gán kết quả vào x[15] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[15] ^= R(x[11] + x[7], 18);
                // Gán kết quả vào x[1] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[1] ^= R(x[0] + x[3], 7);
                // Gán kết quả vào x[2] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[2] ^= R(x[1] + x[0], 9);
                // Gán kết quả vào x[3] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[3] ^= R(x[2] + x[1], 13);
                // Gán kết quả vào x[0] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[0] ^= R(x[3] + x[2], 18);
                // Gán kết quả vào x[6] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[6] ^= R(x[5] + x[4], 7);
                // Gán kết quả vào x[7] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[7] ^= R(x[6] + x[5], 9);
                // Gán kết quả vào x[4] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[4] ^= R(x[7] + x[6], 13);
                // Gán kết quả vào x[5] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[5] ^= R(x[4] + x[7], 18);
                // Gán kết quả vào x[11] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[11] ^= R(x[10] + x[9], 7);
                // Gán kết quả vào x[8] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[8] ^= R(x[11] + x[10], 9);
                // Gán kết quả vào x[9] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[9] ^= R(x[8] + x[11], 13);
                // Gán kết quả vào x[10] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[10] ^= R(x[9] + x[8], 18);
                // Gán kết quả vào x[12] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[12] ^= R(x[15] + x[14], 7);
                // Gán kết quả vào x[13] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[13] ^= R(x[12] + x[15], 9);
                // Gán kết quả vào x[14] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[14] ^= R(x[13] + x[12], 13);
                // Gán kết quả vào x[15] để bước xử lý tiếp theo dùng dữ liệu mới.
                x[15] ^= R(x[14] + x[13], 18);
            }

            // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
            for (var i = 0; i < 16; i++)
                // Gán kết quả vào b[i] để bước xử lý tiếp theo dùng dữ liệu mới.
                b[i] += x[i];
        }
    }

    // Phép xoay trái 32 bit dùng trong Salsa20/8.
    private static uint R(uint x, int n) => (x << n) | (x >> (32 - n));
}
