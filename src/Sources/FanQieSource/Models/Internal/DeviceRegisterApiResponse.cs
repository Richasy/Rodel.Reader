// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.FanQie.Models.Internal;

/// <summary>
/// 设备注册 API 响应.
/// </summary>
internal sealed class DeviceRegisterApiResponse
{
    /// <summary>
    /// 响应码.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 响应消息.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 响应数据.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public DeviceRegisterData? Data { get; set; }
}

/// <summary>
/// 设备注册数据.
/// </summary>
internal sealed class DeviceRegisterData
{
    /// <summary>
    /// 设备 ID.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("device_id")]
    public string? DeviceId { get; set; }

    /// <summary>
    /// 解密密钥（十六进制字符串）.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("secret_key")]
    public string? SecretKey { get; set; }

    /// <summary>
    /// 安装 ID.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("install_id")]
    public string? InstallId { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string? Status { get; set; }
}
