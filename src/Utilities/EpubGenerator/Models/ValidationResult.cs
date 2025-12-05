// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 验证错误信息.
/// </summary>
public sealed class ValidationError
{
    /// <summary>
    /// 错误代码.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 错误消息.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 相关文件路径（可选）.
    /// </summary>
    public string? FilePath { get; init; }
}

/// <summary>
/// 验证警告信息.
/// </summary>
public sealed class ValidationWarning
{
    /// <summary>
    /// 警告代码.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// 警告消息.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// 相关文件路径（可选）.
    /// </summary>
    public string? FilePath { get; init; }
}

/// <summary>
/// EPUB 验证结果.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// 是否验证通过.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// 错误列表（可选）.
    /// </summary>
    public IReadOnlyList<ValidationError>? Errors { get; init; }

    /// <summary>
    /// 警告列表（可选）.
    /// </summary>
    public IReadOnlyList<ValidationWarning>? Warnings { get; init; }

    /// <summary>
    /// 创建一个成功的验证结果.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// 创建一个失败的验证结果.
    /// </summary>
    public static ValidationResult Failure(IReadOnlyList<ValidationError> errors, IReadOnlyList<ValidationWarning>? warnings = null)
        => new() { IsValid = false, Errors = errors, Warnings = warnings };
}
