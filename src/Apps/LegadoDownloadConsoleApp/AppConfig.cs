// Copyright (c) Richasy. All rights reserved.

using System.Text.Json;
using Richasy.RodelReader.Sources.Legado.Models;
using Richasy.RodelReader.Sources.Legado.Models.Enums;

namespace LegadoDownloadConsoleApp;

/// <summary>
/// 应用配置.
/// </summary>
public sealed class AppConfig
{
    private const string ConfigFileName = "legado-config.json";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// 服务器地址.
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// 服务器类型.
    /// </summary>
    public ServerType ServerType { get; set; } = ServerType.Legado;

    /// <summary>
    /// 访问令牌（用于 hectorqin/reader 多用户模式）.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// 默认输出目录.
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// 最大并发下载数.
    /// </summary>
    public int MaxConcurrentDownloads { get; set; } = 3;

    /// <summary>
    /// 获取配置文件路径.
    /// </summary>
    public static string GetConfigPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var configDir = Path.Combine(appDataPath, "LegadoDownloader");
        Directory.CreateDirectory(configDir);
        return Path.Combine(configDir, ConfigFileName);
    }

    /// <summary>
    /// 加载配置.
    /// </summary>
    public static AppConfig? Load()
    {
        var configPath = GetConfigPath();
        if (!File.Exists(configPath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<AppConfig>(json, SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 保存配置.
    /// </summary>
    public void Save()
    {
        var configPath = GetConfigPath();
        var json = JsonSerializer.Serialize(this, SerializerOptions);
        File.WriteAllText(configPath, json);
    }

    /// <summary>
    /// 删除配置.
    /// </summary>
    public static void Delete()
    {
        var configPath = GetConfigPath();
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }
    }

    /// <summary>
    /// 配置是否有效.
    /// </summary>
    public bool IsValid()
        => !string.IsNullOrWhiteSpace(ServerUrl);

    /// <summary>
    /// 转换为客户端配置.
    /// </summary>
    public LegadoClientOptions ToClientOptions()
        => new()
        {
            BaseUrl = ServerUrl ?? throw new InvalidOperationException("服务器地址未配置"),
            ServerType = ServerType,
            AccessToken = AccessToken,
        };
}
