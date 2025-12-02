// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models.Internal;

/// <summary>
/// 用户资料 API 响应.
/// </summary>
internal sealed class ProfileApiResponse
{
    /// <summary>
    /// 获取或设置成功标志.
    /// </summary>
    [JsonPropertyName("success")]
    public int Success { get; set; }

    /// <summary>
    /// 获取或设置用户信息.
    /// </summary>
    [JsonPropertyName("user")]
    public ProfileApiUser? User { get; set; }
}

/// <summary>
/// 用户 API 数据.
/// </summary>
internal sealed class ProfileApiUser
{
    /// <summary>
    /// 获取或设置用户 ID.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// 获取或设置邮箱.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// 获取或设置用户名.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// 获取或设置 Kindle 邮箱.
    /// </summary>
    [JsonPropertyName("kindle_email")]
    public string? KindleEmail { get; set; }

    /// <summary>
    /// 获取或设置 Remix 用户密钥.
    /// </summary>
    [JsonPropertyName("remix_userkey")]
    public string? RemixUserkey { get; set; }

    /// <summary>
    /// 获取或设置今日下载次数.
    /// </summary>
    [JsonPropertyName("downloads_today")]
    public int DownloadsToday { get; set; }

    /// <summary>
    /// 获取或设置下载限制.
    /// </summary>
    [JsonPropertyName("downloads_limit")]
    public int DownloadsLimit { get; set; }

    /// <summary>
    /// 获取或设置是否已确认.
    /// </summary>
    [JsonPropertyName("confirmed")]
    public int Confirmed { get; set; }

    /// <summary>
    /// 获取或设置是否为高级用户.
    /// </summary>
    [JsonPropertyName("isPremium")]
    public int IsPremium { get; set; }
}
