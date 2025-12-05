// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// Feed 元素类型.
/// </summary>
public enum FeedElementType
{
    /// <summary>
    /// 无/未知元素.
    /// </summary>
    None = 0,

    /// <summary>
    /// 订阅项（文章/条目）.
    /// </summary>
    Item,

    /// <summary>
    /// 人员（作者/贡献者）.
    /// </summary>
    Person,

    /// <summary>
    /// 链接.
    /// </summary>
    Link,

    /// <summary>
    /// 内容.
    /// </summary>
    Content,

    /// <summary>
    /// 分类.
    /// </summary>
    Category,

    /// <summary>
    /// 图片.
    /// </summary>
    Image,
}
