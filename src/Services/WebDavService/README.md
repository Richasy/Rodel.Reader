# WebDav 服务库

一个现代化的 WebDAV 客户端库，用于 .NET 9.0+，支持完整的 WebDAV 协议操作。

## 特性

- ✅ 完整的 WebDAV 协议支持（RFC 4918）
- ✅ 模块化设计，按功能划分接口
- ✅ 支持依赖注入
- ✅ 可注入的日志组件
- ✅ 异步 API
- ✅ 强类型的请求/响应模型

## 安装

将项目引用添加到你的解决方案中：

```xml
<ProjectReference Include="..\WebDavService\WebDavService.csproj" />
```

## 快速开始

### 基本使用

```csharp
using Richasy.RodelReader.Services.WebDav;

// 创建客户端
using var client = WebDavClient.Create(new Uri("https://example.com/webdav"));

// 列出目录内容
var response = await client.Properties.PropfindAsync(
    new PropfindParameters
    {
        RequestUri = "/documents",
        RequestType = PropfindRequestType.AllProperties,
        ApplyTo = ApplyTo.Propfind.ResourceAndAncestors,
    });

foreach (var resource in response.Resources)
{
    Console.WriteLine($"{resource.DisplayName} - {resource.ResourceType}");
}
```

### 带身份验证

```csharp
using var client = WebDavClient.Create(
    new Uri("https://example.com/webdav"),
    "username",
    "password");
```

### 完整配置

```csharp
var options = new WebDavClientOptions
{
    BaseAddress = new Uri("https://example.com/webdav"),
    Credentials = new NetworkCredential("user", "pass"),
    Timeout = TimeSpan.FromSeconds(30),
    PreAuthenticate = true,
};

using var client = new WebDavClient(options);
```

### 带日志记录

```csharp
using Microsoft.Extensions.Logging;

// 使用 ILoggerFactory 创建日志器
var logger = loggerFactory.CreateLogger<WebDavClient>();
using var client = new WebDavClient(options, logger);
```

## 模块化接口

客户端按功能划分为以下模块：

### Properties（属性操作）

```csharp
// 获取资源属性 (PROPFIND)
var response = await client.Properties.PropfindAsync(parameters);

// 获取特定属性
var response = await client.Properties.PropfindAsync(new PropfindParameters
{
    RequestUri = "/file.txt",
    RequestType = PropfindRequestType.NamedProperties,
    CustomProperties = new[]
    {
        new WebDavProperty("getlastmodified", WebDavConstants.Dav),
        new WebDavProperty("getcontentlength", WebDavConstants.Dav),
    },
});

// 修改属性 (PROPPATCH)
var response = await client.Properties.ProppatchAsync(new ProppatchParameters
{
    RequestUri = "/file.txt",
    PropertiesToSet = new[] { new WebDavProperty("customProp", "urn:custom", "value") },
    PropertiesToRemove = new[] { new WebDavProperty("oldProp", "urn:custom") },
});
```

### Resources（资源操作）

```csharp
// 创建目录 (MKCOL)
await client.Resources.MkColAsync(new MkColParameters { RequestUri = "/new-folder" });

// 删除资源 (DELETE)
await client.Resources.DeleteAsync(new DeleteParameters { RequestUri = "/old-file.txt" });

// 复制资源 (COPY)
await client.Resources.CopyAsync(new CopyParameters
{
    SourceUri = "/source.txt",
    DestinationUri = "/dest.txt",
    Overwrite = true,
});

// 移动/重命名资源 (MOVE)
await client.Resources.MoveAsync(new MoveParameters
{
    SourceUri = "/old-name.txt",
    DestinationUri = "/new-name.txt",
});
```

### Files（文件操作）

```csharp
// 下载文件获取原始流
using var response = await client.Files.GetRawFileAsync(
    new GetFileParameters { RequestUri = "/document.pdf" });

await using var fileStream = File.Create("local.pdf");
await response.Stream.CopyToAsync(fileStream);

// 下载文件并处理
await client.Files.GetProcessedFileAsync(
    new GetFileParameters { RequestUri = "/document.pdf" },
    async (stream, ct) =>
    {
        // 处理流...
    });

// 上传文件
await using var content = File.OpenRead("local.pdf");
await client.Files.PutFileAsync(new PutFileParameters
{
    RequestUri = "/remote.pdf",
    Content = content,
    ContentType = "application/pdf",
});
```

### Locks（锁操作）

```csharp
// 锁定资源 (LOCK)
var lockResponse = await client.Locks.LockAsync(new LockParameters
{
    RequestUri = "/important.docx",
    Scope = LockScope.Exclusive,
    Owner = new LockOwner { Value = "user@example.com" },
    Timeout = TimeSpan.FromMinutes(30),
});

var lockToken = lockResponse.LockToken;

// 使用锁令牌修改文件
await client.Files.PutFileAsync(new PutFileParameters
{
    RequestUri = "/important.docx",
    Content = content,
    LockToken = lockToken,
});

// 解锁 (UNLOCK)
await client.Locks.UnlockAsync(new UnlockParameters
{
    RequestUri = "/important.docx",
    LockToken = lockToken,
});
```

### Search（搜索操作）

```csharp
// 搜索资源 (SEARCH)
var results = await client.Search.SearchAsync(new SearchParameters
{
    RequestUri = "/",
    SearchQuery = "important",
    Scope = SearchScope.Infinity,
});
```

## 依赖注入

### 使用 Microsoft.Extensions.DependencyInjection

```csharp
services.AddSingleton(sp =>
{
    var options = new WebDavClientOptions
    {
        BaseAddress = new Uri("https://example.com/webdav"),
        Credentials = new NetworkCredential("user", "pass"),
    };
    var logger = sp.GetRequiredService<ILogger<WebDavClient>>();
    return new WebDavClient(options, logger);
});
```

### 完全自定义注入

```csharp
// 注册各模块实现
services.AddSingleton<IWebDavDispatcher>(sp =>
{
    var client = sp.GetRequiredService<HttpClient>();
    var logger = sp.GetRequiredService<ILogger<WebDavDispatcher>>();
    return new WebDavDispatcher(client, logger);
});

services.AddSingleton<IPropertyOperator, PropertyOperator>();
services.AddSingleton<IResourceOperator, ResourceOperator>();
services.AddSingleton<IFileOperator, FileOperator>();
services.AddSingleton<ILockOperator, LockOperator>();
services.AddSingleton<ISearchOperator, SearchOperator>();

// 使用完全 DI 构造函数
services.AddSingleton<IWebDavClient>(sp =>
    new WebDavClient(
        sp.GetRequiredService<IPropertyOperator>(),
        sp.GetRequiredService<IResourceOperator>(),
        sp.GetRequiredService<IFileOperator>(),
        sp.GetRequiredService<ILockOperator>(),
        sp.GetRequiredService<ISearchOperator>(),
        sp.GetRequiredService<IWebDavDispatcher>(),
        sp.GetRequiredService<ILogger<WebDavClient>>()));
```

## 错误处理

```csharp
try
{
    await client.Resources.DeleteAsync(new DeleteParameters { RequestUri = "/file.txt" });
}
catch (WebDavException ex)
{
    Console.WriteLine($"WebDAV 错误: {ex.Message}");
    Console.WriteLine($"状态码: {ex.StatusCode}");
    Console.WriteLine($"请求 URI: {ex.RequestUri}");
}
```

## 项目结构

```
WebDavService/
├── Abstractions/           # 公共接口定义
│   ├── IWebDavClient.cs
│   ├── IWebDavDispatcher.cs
│   ├── IPropertyOperator.cs
│   ├── IResourceOperator.cs
│   ├── IFileOperator.cs
│   ├── ILockOperator.cs
│   └── ISearchOperator.cs
├── Models/                 # 数据模型
│   ├── Enums/
│   └── Parameters/         # 请求参数
├── Exceptions/             # 异常类型
├── Helpers/                # 辅助工具类
├── Internal/               # 内部实现
│   ├── Builders/           # XML 请求构建器
│   ├── Parsers/            # XML 响应解析器
│   ├── Operators/          # 操作模块实现
│   └── WebDavDispatcher.cs
└── WebDavClient.cs         # 主入口
```

## 许可证

Copyright (c) Richasy. All rights reserved.
