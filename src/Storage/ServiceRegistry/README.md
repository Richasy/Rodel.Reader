# ServiceRegistry

服务注册表存储模块，用于管理应用中的多个服务实例。

## 功能

- **多服务实例管理**：支持创建多个书籍库、RSS 订阅源、播客服务
- **服务切换**：记录当前活动服务，支持快速切换
- **自定义配置**：每个服务可配置名称、图标、主题色等
- **数据隔离**：每个服务实例使用独立的数据目录

## 服务类型

| 类型 | 说明 |
|------|------|
| `Book` | 书籍/漫画服务 |
| `Rss` | RSS 订阅服务 |
| `Podcast` | 播客服务 |

## 目录结构

```
{LibraryPath}/
├── registry.db                    # 服务注册表数据库
├── {ServiceId-1}/                 # 服务实例 1 的数据目录
│   └── ...
├── {ServiceId-2}/                 # 服务实例 2 的数据目录
│   └── ...
└── ...
```

## 使用示例

```csharp
// 初始化注册表
var options = new ServiceRegistryOptions { LibraryPath = "C:/MyLibrary" };
var registry = new ServiceRegistry(options);
await registry.InitializeAsync();

// 创建服务
var bookService = await registry.CreateServiceAsync("我的书库", ServiceType.Book);
var privateBooks = await registry.CreateServiceAsync("私密书库", ServiceType.Book, icon: "lock");

// 设置活动服务
await registry.SetActiveServiceAsync(bookService.Id);

// 获取服务数据路径
var dataPath = registry.GetServiceDataPath(bookService.Id);
// 结果: C:/MyLibrary/{bookService.Id}

// 配合 BookStorage 使用
var bookStorageOptions = new BookStorageOptions
{
    DatabasePath = Path.Combine(dataPath, "book.db")
};
var bookStorage = new BookStorage(bookStorageOptions);
await bookStorage.InitializeAsync();
```

## 数据模型

### ServiceInstance

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | string | 服务唯一标识符 (GUID) |
| `Name` | string | 用户自定义名称 |
| `Type` | ServiceType | 服务类型 |
| `Icon` | string? | 图标标识 |
| `Color` | string? | 主题色 (十六进制) |
| `Description` | string? | 服务描述 |
| `CreatedAt` | DateTimeOffset | 创建时间 |
| `LastAccessedAt` | DateTimeOffset | 最后访问时间 |
| `SortOrder` | int | 排序顺序 |
| `Settings` | string? | 扩展配置 (JSON) |
| `IsActive` | bool | 是否为当前活动服务 |
