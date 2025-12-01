# FeedParser 测试数据

此目录用于存放 Atom 测试文件。

## 文件格式

请将 Atom 1.0 格式的 XML 文件放在此目录中，文件扩展名为 `.xml`。

## 示例

将真实的 Atom Feed 保存为 XML 文件，例如：

- `github-releases.xml` - GitHub 发布 Atom
- `blog-feed.xml` - 博客 Atom Feed

## 测试

本地集成测试会自动遍历此目录中的所有 `.xml` 文件进行解析测试。
