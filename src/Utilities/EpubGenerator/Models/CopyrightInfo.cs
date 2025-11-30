// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// 版权信息.
/// </summary>
public sealed class CopyrightInfo
{
    /// <summary>
    /// 版权声明文本.
    /// </summary>
    public string? Copyright { get; init; }

    /// <summary>
    /// 权利说明.
    /// </summary>
    public string? Rights { get; init; }

    /// <summary>
    /// 版次.
    /// </summary>
    public string? Edition { get; init; }

    /// <summary>
    /// ISBN.
    /// </summary>
    public string? Isbn { get; init; }

    /// <summary>
    /// 出版日期.
    /// </summary>
    public DateTimeOffset? PublishDate { get; init; }
}
