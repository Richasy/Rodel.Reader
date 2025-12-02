// Copyright (c) Richasy. All rights reserved.

#pragma warning disable CA5351 // 此处使用 MD5 仅用于生成设备标识符，不用于安全目的

namespace Richasy.RodelReader.Sources.FanQie.Helpers;

/// <summary>
/// 设备信息生成器.
/// 基于 fqnovel-unidbg 项目的 batch_device_register_xml.py 实现.
/// </summary>
internal static class DeviceGenerator
{
    private static readonly Random Random = new();

    private static readonly string[] DeviceBrands =
    [
        "Xiaomi", "HUAWEI", "OPPO", "vivo", "OnePlus", "Samsung"
    ];

    private static readonly Dictionary<string, string[]> DeviceModels = new()
    {
        ["Xiaomi"] = ["24031PN0DC", "2304FPN6DC", "23078RKD5C", "MI11", "MI12", "MI13", "RedmiK40", "RedmiK50"],
        ["HUAWEI"] = ["ELS-AN00", "TAS-AL00", "ANA-AN00", "P50", "P40", "Mate40", "Mate50"],
        ["OPPO"] = ["CPH2207", "CPH2211", "FindX5", "Reno8", "Reno9"],
        ["vivo"] = ["V2197A", "V2118A", "X80", "X90", "iQOO9"],
        ["OnePlus"] = ["LE2100", "LE2110", "OnePlus9", "OnePlus10", "OnePlus11"],
        ["Samsung"] = ["SM-G9980", "SM-G9910", "GalaxyS22", "GalaxyS23"],
    };

    private static readonly string[] AndroidVersions = ["10", "11", "12", "13", "14"];
    private static readonly int[] AndroidApis = [29, 30, 32, 33, 34];
    private static readonly string[] CpuAbis = ["arm64-v8a", "armeabi-v7a"];

    /// <summary>
    /// 生成随机的设备 ID（16 位数字字符串）.
    /// </summary>
    public static string GenerateDeviceId()
    {
        return Random.NextInt64(1000000000000000, 9999999999999999).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 生成随机的安装 ID（16 位数字字符串）.
    /// </summary>
    public static string GenerateInstallId()
    {
        return Random.NextInt64(1000000000000000, 9999999999999999).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 生成随机的 Android ID（16 位十六进制字符串）.
    /// </summary>
    public static string GenerateAndroidId()
    {
        const string chars = "0123456789abcdef";
        var result = new char[16];
        for (var i = 0; i < 16; i++)
        {
            result[i] = chars[Random.Next(chars.Length)];
        }

        return new string(result);
    }

    /// <summary>
    /// 基于真实算法生成 OpenUDID.
    /// 算法: char = md5(androidId); udid = char + md5(char).slice(0, 8)
    /// </summary>
    public static string GenerateOpenUdid(string? androidId = null)
    {
        androidId ??= GenerateAndroidId();

        // 第一步：对 android_id 进行 MD5
        var charHash = Md5Encode(androidId);

        // 第二步：对第一步结果再进行 MD5，取前8位
        var charMd5 = Md5Encode(charHash);

        // 第三步：拼接成40位的 openudid
        return (charHash + charMd5[..8]).ToLowerInvariant();
    }

    /// <summary>
    /// 生成随机 UUID.
    /// </summary>
    public static string GenerateUuid()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 生成随机签名哈希（32 位十六进制字符串）.
    /// </summary>
    public static string GenerateSigHash()
    {
        const string chars = "0123456789abcdef";
        var result = new char[32];
        for (var i = 0; i < 32; i++)
        {
            result[i] = chars[Random.Next(chars.Length)];
        }

        return new string(result);
    }

    /// <summary>
    /// 生成完整的设备信息.
    /// </summary>
    public static DeviceInfo GenerateDeviceInfo()
    {
        var brand = DeviceBrands[Random.Next(DeviceBrands.Length)];
        var models = DeviceModels[brand];
        var model = models[Random.Next(models.Length)];
        var versionIndex = Random.Next(AndroidVersions.Length);
        var androidId = GenerateAndroidId();

        return new DeviceInfo
        {
            DeviceId = GenerateDeviceId(),
            InstallId = GenerateInstallId(),
            AndroidId = androidId,
            OpenUdid = GenerateOpenUdid(androidId),
            DeviceBrand = brand,
            DeviceModel = model,
            OsVersion = AndroidVersions[versionIndex],
            OsApi = AndroidApis[versionIndex],
            CpuAbi = CpuAbis[Random.Next(CpuAbis.Length)],
            Cdid = GenerateUuid(),
            SigHash = GenerateSigHash(),
            ClientUdid = GenerateUuid(),
        };
    }

    /// <summary>
    /// MD5 编码.
    /// </summary>
    private static string Md5Encode(string text)
    {
        var inputBytes = Encoding.UTF8.GetBytes(text);
        var hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
        var sb = new StringBuilder();
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }
}

/// <summary>
/// 设备信息.
/// </summary>
internal sealed class DeviceInfo
{
    /// <summary>
    /// 设备 ID.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// 安装 ID.
    /// </summary>
    public required string InstallId { get; init; }

    /// <summary>
    /// Android ID.
    /// </summary>
    public required string AndroidId { get; init; }

    /// <summary>
    /// OpenUDID.
    /// </summary>
    public required string OpenUdid { get; init; }

    /// <summary>
    /// 设备品牌.
    /// </summary>
    public required string DeviceBrand { get; init; }

    /// <summary>
    /// 设备型号.
    /// </summary>
    public required string DeviceModel { get; init; }

    /// <summary>
    /// 系统版本.
    /// </summary>
    public required string OsVersion { get; init; }

    /// <summary>
    /// API 级别.
    /// </summary>
    public required int OsApi { get; init; }

    /// <summary>
    /// CPU ABI.
    /// </summary>
    public required string CpuAbi { get; init; }

    /// <summary>
    /// CDID.
    /// </summary>
    public required string Cdid { get; init; }

    /// <summary>
    /// 签名哈希.
    /// </summary>
    public required string SigHash { get; init; }

    /// <summary>
    /// Client UDID.
    /// </summary>
    public required string ClientUdid { get; init; }
}
