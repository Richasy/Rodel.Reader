// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.Legado.Internal;

/// <summary>
/// API 响应包装器.
/// </summary>
/// <typeparam name="T">数据类型.</typeparam>
internal sealed class ApiResponse<T>
{
    /// <summary>
    /// 是否成功.
    /// </summary>
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息.
    /// </summary>
    [JsonPropertyName("errorMsg")]
    public string? ErrorMsg { get; set; }

    /// <summary>
    /// 数据.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

/// <summary>
/// JSON 序列化上下文（支持 AOT）.
/// </summary>
[JsonSerializable(typeof(ApiResponse<List<Book>>))]
[JsonSerializable(typeof(ApiResponse<List<Chapter>>))]
[JsonSerializable(typeof(ApiResponse<string>))]
[JsonSerializable(typeof(ApiResponse<Book>))]
[JsonSerializable(typeof(ApiResponse<BookSource>))]
[JsonSerializable(typeof(ApiResponse<List<BookSource>>))]
[JsonSerializable(typeof(List<Book>))]
[JsonSerializable(typeof(List<Chapter>))]
[JsonSerializable(typeof(List<BookSource>))]
[JsonSerializable(typeof(Book))]
[JsonSerializable(typeof(Chapter))]
[JsonSerializable(typeof(BookSource))]
[JsonSerializable(typeof(BookProgress))]
[JsonSerializable(typeof(string))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class LegadoJsonContext : JsonSerializerContext
{
}
