// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Models.Internal;

/// <summary>
/// 登录响应.
/// </summary>
internal sealed class LoginResponse
{
    /// <summary>
    /// 获取或设置响应数据.
    /// </summary>
    [JsonPropertyName("response")]
    public LoginResponseData? Response { get; set; }
}

/// <summary>
/// 登录响应数据.
/// </summary>
internal sealed class LoginResponseData
{
    /// <summary>
    /// 获取或设置验证错误信息.
    /// </summary>
    [JsonPropertyName("validationError")]
    public object? ValidationError { get; set; }

    /// <summary>
    /// 获取或设置用户 ID.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// 获取或设置用户密钥.
    /// </summary>
    [JsonPropertyName("userKey")]
    public string? UserKey { get; set; }
}
