# FeedParser 测试数据

此目录用于存放播客（Podcast）测试文件。

## 文件格式

请将包含 iTunes 播客扩展的 RSS XML 文件放在此目录中，文件扩展名为 `.xml`。

## 示例

将真实的播客 Feed 保存为 XML 文件，例如：

- `tech-podcast.xml` - 技术播客
- `news-podcast.xml` - 新闻播客

## 播客特有字段

播客 RSS 通常包含以下 iTunes 扩展字段：

- `itunes:author` - 作者
- `itunes:image` - 封面图片
- `itunes:category` - 分类
- `itunes:duration` - 时长
- `enclosure` - 音频文件

## 测试

本地集成测试会自动遍历此目录中的所有 `.xml` 文件进行解析测试，
并验证播客特有字段的解析。
