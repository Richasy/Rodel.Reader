// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models;

/// <summary>
/// 用户资料信息.
/// </summary>
public sealed record UserProfile
{
    /// <summary>
    /// 获取用户 ID.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// 获取用户邮箱.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// 获取用户名.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// 获取 Kindle 邮箱.
    /// </summary>
    public string? KindleEmail { get; init; }

    /// <summary>
    /// 获取今日已下载次数.
    /// </summary>
    public int DownloadsToday { get; init; }

    /// <summary>
    /// 获取每日下载限制.
    /// </summary>
    public int DownloadsLimit { get; init; }

    /// <summary>
    /// 获取今日剩余下载次数.
    /// </summary>
    public int DownloadsRemaining => DownloadsLimit - DownloadsToday;

    /// <summary>
    /// 获取是否已确认账户.
    /// </summary>
    public bool IsConfirmed { get; init; }

    /// <summary>
    /// 获取是否为高级用户.
    /// </summary>
    public bool IsPremium { get; init; }
}
