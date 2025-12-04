// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Models.Enums;

/// <summary>
/// 服务器类型.
/// </summary>
public enum ServerType
{
    /// <summary>
    /// Legado 原版 (gedoor/legado).
    /// API 路径: /{endpoint}
    /// </summary>
    Legado,

    /// <summary>
    /// hectorqin/reader 阅读服务器.
    /// API 路径: /reader3/{endpoint}
    /// </summary>
    HectorqinReader,
}
