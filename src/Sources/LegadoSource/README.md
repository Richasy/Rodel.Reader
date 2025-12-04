# Legado Source

用于访问 [Legado (开源阅读)](https://github.com/gedoor/legado) 和 [hectorqin/reader](https://github.com/hectorqin/reader) 服务器 API 的客户端库。

## 功能特性

- ✅ 书架管理（获取书架、添加/删除书籍）
- ✅ 章节操作（获取章节列表、获取章节内容）
- ✅ 阅读进度同步
- ✅ 书源管理（增删改查）
- ✅ 封面获取
- ✅ 支持 Legado 原版和 hectorqin/reader 服务器
- ✅ 完整的日志记录
- ✅ AOT 友好（使用 JsonSerializerContext）

## 快速开始

### 基本用法

```csharp
using Richasy.RodelReader.Sources.Legado;

// 创建客户端配置
var options = new LegadoClientOptions
{
    BaseUrl = "http://192.168.1.100:1234",
    ServerType = ServerType.Legado,  // 或 ServerType.HectorqinReader
};

// 创建客户端
using var client = new LegadoClient(options);

// 获取书架
var books = await client.GetBookshelfAsync();

// 获取章节列表
var chapters = await client.GetChapterListAsync(books[0].BookUrl);

// 获取章节内容
var content = await client.GetChapterContentAsync(books[0].BookUrl, 0);
```

### 使用 hectorqin/reader 服务器

```csharp
var options = new LegadoClientOptions
{
    BaseUrl = "http://your-server:8080",
    ServerType = ServerType.HectorqinReader,
    AccessToken = "your-access-token",  // 多用户模式需要
};

using var client = new LegadoClient(options);
```

### 带日志记录

```csharp
using Microsoft.Extensions.Logging;

// 使用你喜欢的日志框架
ILogger<LegadoClient> logger = ...;

var client = new LegadoClient(options, logger);
```

## API 参考

### ILegadoClient 接口

| 方法 | 说明 |
|------|------|
| `GetBookshelfAsync` | 获取书架上的所有书籍 |
| `SaveBookAsync` | 保存书籍到书架 |
| `DeleteBookAsync` | 从书架删除书籍 |
| `GetChapterListAsync` | 获取书籍的章节列表 |
| `GetChapterContentAsync` | 获取章节内容 |
| `SaveProgressAsync` | 保存阅读进度 |
| `GetBookSourcesAsync` | 获取所有书源 |
| `GetBookSourceAsync` | 获取单个书源 |
| `SaveBookSourceAsync` | 保存书源 |
| `SaveBookSourcesAsync` | 批量保存书源 |
| `DeleteBookSourcesAsync` | 批量删除书源 |
| `GetCoverAsync` | 获取封面图片流 |
| `GetCoverUrl` | 获取封面完整 URL |

## 配置选项

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `BaseUrl` | string | - | 服务器基础 URL（必需） |
| `ServerType` | ServerType | Legado | 服务器类型 |
| `AccessToken` | string? | null | 访问令牌（hectorqin/reader 多用户模式） |
| `Timeout` | TimeSpan | 30秒 | 请求超时时间 |
| `IgnoreSslErrors` | bool | true | 是否忽略 SSL 证书错误 |
| `UserAgent` | string | "RodelReader/1.0" | User-Agent 字符串 |

## 服务器类型差异

| 特性 | Legado | hectorqin/reader |
|------|--------|------------------|
| API 前缀 | `/` | `/reader3/` |
| 认证 | 无 | AccessToken |
| 多用户 | 否 | 是 |

## 参考资料

- [Legado API 文档](https://github.com/gedoor/legado/blob/master/api.md)
- [hectorqin/reader 文档](https://github.com/hectorqin/reader/blob/master/doc.md)
