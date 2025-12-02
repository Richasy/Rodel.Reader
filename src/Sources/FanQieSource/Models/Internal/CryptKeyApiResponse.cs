// Copyright (c) Richasy. All rights reserved.

using System.Text.Json.Serialization;

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 加密密钥 API 响应.
/// 对应 Java 中的 FqRegisterKeyResponse.
/// </summary>
internal sealed class CryptKeyApiResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public CryptKeyData? Data { get; set; }
}

/// <summary>
/// 加密密钥数据.
/// 对应 Java 中的 FqRegisterKeyPayloadResponse.
/// </summary>
internal sealed class CryptKeyData
{
    /// <summary>
    /// 加密后的密钥（Base64 编码），需要使用 ContentDecryptor.DecryptRegisterKey 解密.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// 密钥版本.
    /// </summary>
    [JsonPropertyName("keyver")]
    public long Keyver { get; set; }
}

/// <summary>
/// 注册密钥请求.
/// 对应 Java 中的 FqRegisterKeyPayload.
/// </summary>
internal sealed class RegisterKeyRequest
{
    /// <summary>
    /// 加密后的内容（使用 ContentDecryptor.GenerateRegisterKeyContent 生成）.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 密钥版本（通常为 1）.
    /// </summary>
    [JsonPropertyName("keyver")]
    public long Keyver { get; set; } = 1;
}
