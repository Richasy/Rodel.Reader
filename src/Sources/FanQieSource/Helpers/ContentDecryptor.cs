// Copyright (c) Richasy. All rights reserved.

#pragma warning disable CA5390 // 此处使用的是已知的固定注册密钥

namespace Richasy.RodelReader.Sources.FanQie.Helpers;

/// <summary>
/// AES 内容解密器.
/// 基于 fqnovel-unidbg 项目的 FqCrypto.java 实现.
/// </summary>
internal static class ContentDecryptor
{
    /// <summary>
    /// 固定的注册密钥（用于加密 payload 和解密 registerkey 响应）.
    /// REG_KEY 是 32 位的十六进制字符串，表示 16 字节的密钥.
    /// </summary>
    private const string RegKeyHex = "ac25c67ddd8f38c1b37a2348828e222e";

    /// <summary>
    /// 解密章节内容.
    /// </summary>
    /// <param name="encryptedContent">Base64 编码的加密内容.</param>
    /// <param name="cryptKeyHex">从 registerkey API 获取并解密的密钥（32 位十六进制字符串），如果为空则使用默认的 REG_KEY.</param>
    /// <returns>解密后的 HTML 内容.</returns>
    /// <exception cref="Exceptions.FanQieDecryptException">解密失败时抛出.</exception>
    public static string DecryptContent(string encryptedContent, string? cryptKeyHex = null)
    {
        if (string.IsNullOrWhiteSpace(encryptedContent))
        {
            return string.Empty;
        }

        // 如果没有提供 cryptKeyHex，使用默认的 REG_KEY
        var keyToUse = string.IsNullOrWhiteSpace(cryptKeyHex) ? RegKeyHex : cryptKeyHex;

        try
        {
            // 1. Base64 解码
            var encryptedBytes = Convert.FromBase64String(encryptedContent);

            // 2. 提取 IV（前16字节）和密文
            if (encryptedBytes.Length <= 16)
            {
                throw new Exceptions.FanQieDecryptException("Encrypted content is too short.");
            }

            var iv = encryptedBytes.AsSpan(0, 16).ToArray();
            var ciphertext = encryptedBytes.AsSpan(16).ToArray();

            // 3. 准备密钥（从十六进制字符串解码为字节数组）
            var keyBytes = HexStringToByteArray(keyToUse);

            // 4. AES-128-CBC 解密
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

            // 5. 检查是否是 GZIP 压缩数据并解压
            if (decryptedBytes.Length >= 2 &&
                decryptedBytes[0] == 0x1f &&
                decryptedBytes[1] == 0x8b)
            {
                using var compressedStream = new MemoryStream(decryptedBytes);
                using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
                using var resultStream = new MemoryStream();
                gzipStream.CopyTo(resultStream);
                return Encoding.UTF8.GetString(resultStream.ToArray());
            }
            else
            {
                // 如果不是压缩数据，直接返回 UTF-8 字符串
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        catch (Exceptions.FanQieDecryptException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exceptions.FanQieDecryptException($"Failed to decrypt content: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 生成 RegisterKey 请求的 content 字段.
    /// 对应 Java 中的 newRegisterKeyContent 方法.
    /// </summary>
    /// <param name="serverDeviceId">服务器设备 ID（数字字符串）.</param>
    /// <returns>Base64 编码的加密 content.</returns>
    public static string GenerateRegisterKeyContent(string serverDeviceId)
    {
        if (string.IsNullOrWhiteSpace(serverDeviceId))
        {
            throw new ArgumentException("Server device ID cannot be empty.", nameof(serverDeviceId));
        }

        try
        {
            // 解析设备 ID
            var deviceId = long.Parse(serverDeviceId, System.Globalization.CultureInfo.InvariantCulture);

            // 将两个 long 值按小端序转换为字节数组并连接 (deviceId + 0)
            var buffer = new byte[16];
            BitConverter.TryWriteBytes(buffer.AsSpan(0, 8), deviceId);
            BitConverter.TryWriteBytes(buffer.AsSpan(8, 8), 0L);

            // 如果是大端序系统，需要反转字节
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer, 0, 8);
                Array.Reverse(buffer, 8, 8);
            }

            // 准备密钥（从十六进制字符串解码）
            var keyBytes = HexStringToByteArray(RegKeyHex);

            // 生成随机 IV
            var iv = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            // AES-128-CBC 加密
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var encryptedData = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);

            // 组合 IV + 密文
            var result = new byte[iv.Length + encryptedData.Length];
            iv.CopyTo(result, 0);
            encryptedData.CopyTo(result, iv.Length);

            // Base64 编码
            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            throw new Exceptions.FanQieDecryptException($"Failed to generate register key content: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从 registerkey API 响应中解密获取真实的解密密钥.
    /// 对应 Java 中的 getRealKey 方法.
    /// </summary>
    /// <param name="registerkeyResponseKey">registerkey API 响应中的 key 字段（Base64 编码）.</param>
    /// <returns>前 16 字节的十六进制字符串（32 个字符）.</returns>
    public static string DecryptRegisterKey(string registerkeyResponseKey)
    {
        if (string.IsNullOrWhiteSpace(registerkeyResponseKey))
        {
            throw new ArgumentException("Register key response is empty.", nameof(registerkeyResponseKey));
        }

        try
        {
            // 1. Base64 解码
            var raw = Convert.FromBase64String(registerkeyResponseKey);

            if (raw.Length < 16)
            {
                throw new Exceptions.FanQieDecryptException("Encrypted register key is too short.");
            }

            // 2. 提取 IV（前 16 字节）和密文
            var iv = raw.AsSpan(0, 16).ToArray();
            var cipherText = raw.AsSpan(16).ToArray();

            // 3. 准备密钥（从 REG_KEY 十六进制字符串解码）
            var keyBytes = HexStringToByteArray(RegKeyHex);

            // 4. AES-128-CBC 解密
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

            // 5. 转换为十六进制字符串，取前 16 字节（32 个十六进制字符）
            var fullKeyHex = ByteArrayToHexString(decrypted);

            if (fullKeyHex.Length >= 32)
            {
                return fullKeyHex[..32].ToUpperInvariant();
            }
            else
            {
                throw new Exceptions.FanQieDecryptException("Decrypted key is too short.");
            }
        }
        catch (Exceptions.FanQieDecryptException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exceptions.FanQieDecryptException($"Failed to decrypt register key: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 十六进制字符串转字节数组.
    /// </summary>
    private static byte[] HexStringToByteArray(string hex)
    {
        var len = hex.Length;
        var data = new byte[len / 2];
        for (var i = 0; i < len; i += 2)
        {
            data[i / 2] = (byte)((GetHexValue(hex[i]) << 4) + GetHexValue(hex[i + 1]));
        }

        return data;
    }

    /// <summary>
    /// 字节数组转十六进制字符串.
    /// </summary>
    private static string ByteArrayToHexString(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// 获取十六进制字符的数值.
    /// </summary>
    private static int GetHexValue(char hex)
    {
        return hex switch
        {
            >= '0' and <= '9' => hex - '0',
            >= 'A' and <= 'F' => hex - 'A' + 10,
            >= 'a' and <= 'f' => hex - 'a' + 10,
            _ => throw new ArgumentException($"Invalid hex character: {hex}"),
        };
    }
}
