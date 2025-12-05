// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 书写方向.
/// </summary>
public enum WritingDirection
{
    /// <summary>
    /// 从左到右.
    /// </summary>
    Ltr,

    /// <summary>
    /// 从右到左（阿拉伯语、希伯来语）.
    /// </summary>
    Rtl,

    /// <summary>
    /// 从上到下（日语竖排）.
    /// </summary>
    Ttb,
}
