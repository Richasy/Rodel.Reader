// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// EPUB 验证器.
/// </summary>
public interface IEpubValidator
{
    /// <summary>
    /// 验证 EPUB 内容是否符合规范.
    /// </summary>
    /// <param name="content">EPUB 内容集合.</param>
    /// <returns>验证结果.</returns>
    ValidationResult Validate(EpubContent content);

    /// <summary>
    /// 验证 EPUB 文件是否符合规范.
    /// </summary>
    /// <param name="filePath">EPUB 文件路径.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>验证结果.</returns>
    Task<ValidationResult> ValidateFileAsync(string filePath, CancellationToken cancellationToken = default);
}
