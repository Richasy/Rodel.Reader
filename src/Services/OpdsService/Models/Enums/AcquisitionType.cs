// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models.Enums;

/// <summary>
/// OPDS 获取类型.
/// </summary>
public enum AcquisitionType
{
    /// <summary>
    /// 通用获取（无特定类型）.
    /// </summary>
    Generic,

    /// <summary>
    /// 开放获取（免费下载）.
    /// </summary>
    OpenAccess,

    /// <summary>
    /// 借阅.
    /// </summary>
    Borrow,

    /// <summary>
    /// 购买.
    /// </summary>
    Buy,

    /// <summary>
    /// 样本/试读.
    /// </summary>
    Sample,

    /// <summary>
    /// 订阅.
    /// </summary>
    Subscribe,
}
