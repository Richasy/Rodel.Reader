// Copyright (c) Richasy. All rights reserved.

using System.Xml.Linq;

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// FB2 description 节点解析器。
/// </summary>
internal static class DescriptionParser
{
    private static readonly XNamespace FbNs = "http://www.gribuser.ru/xml/fictionbook/2.0";
    private static readonly XNamespace XlinkNs = "http://www.w3.org/1999/xlink";

    /// <summary>
    /// 解析 description 节点，提取元数据。
    /// </summary>
    /// <param name="descriptionElement">description 元素。</param>
    /// <returns>解析后的元数据。</returns>
    public static Fb2Metadata Parse(XElement? descriptionElement)
    {
        var metadata = new Fb2Metadata();

        if (descriptionElement == null)
        {
            return metadata;
        }

        // 解析 title-info
        var titleInfo = descriptionElement.Element(FbNs + "title-info")
            ?? descriptionElement.Elements().FirstOrDefault(e => e.Name.LocalName == "title-info");

        if (titleInfo != null)
        {
            ParseTitleInfo(titleInfo, metadata);
        }

        // 解析 document-info
        var documentInfo = descriptionElement.Element(FbNs + "document-info")
            ?? descriptionElement.Elements().FirstOrDefault(e => e.Name.LocalName == "document-info");

        if (documentInfo != null)
        {
            metadata.DocumentInfo = ParseDocumentInfo(documentInfo);
        }

        // 解析 publish-info
        var publishInfo = descriptionElement.Element(FbNs + "publish-info")
            ?? descriptionElement.Elements().FirstOrDefault(e => e.Name.LocalName == "publish-info");

        if (publishInfo != null)
        {
            metadata.PublishInfo = ParsePublishInfo(publishInfo);

            // 如果 title-info 中没有出版商，从 publish-info 中获取
            if (string.IsNullOrEmpty(metadata.Publisher))
            {
                metadata.Publisher = metadata.PublishInfo.Publisher;
            }

            if (string.IsNullOrEmpty(metadata.PublishDate))
            {
                metadata.PublishDate = metadata.PublishInfo.Year;
            }

            if (string.IsNullOrEmpty(metadata.Identifier))
            {
                metadata.Identifier = metadata.PublishInfo.Isbn;
            }
        }

        return metadata;
    }

    private static void ParseTitleInfo(XElement titleInfo, Fb2Metadata metadata)
    {
        // 解析类型/分类
        foreach (var genre in GetElements(titleInfo, "genre"))
        {
            var genreText = genre.Value.Trim();
            if (!string.IsNullOrEmpty(genreText))
            {
                metadata.Genres.Add(genreText);
            }
        }

        // 解析作者
        foreach (var author in GetElements(titleInfo, "author"))
        {
            var parsedAuthor = ParseAuthor(author);
            if (!string.IsNullOrEmpty(parsedAuthor.GetDisplayName()))
            {
                metadata.Authors.Add(parsedAuthor);
            }
        }

        // 解析标题
        var bookTitle = GetElement(titleInfo, "book-title");
        metadata.Title = bookTitle?.Value.Trim();

        // 解析注释/描述
        var annotation = GetElement(titleInfo, "annotation");
        if (annotation != null)
        {
            metadata.Description = ExtractTextContent(annotation);
        }

        // 解析关键词
        var keywords = GetElement(titleInfo, "keywords");
        if (keywords != null)
        {
            var keywordText = keywords.Value.Trim();
            if (!string.IsNullOrEmpty(keywordText))
            {
                // 关键词通常以逗号分隔
                foreach (var kw in keywordText.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = kw.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        metadata.Keywords.Add(trimmed);
                    }
                }
            }
        }

        // 解析日期
        var date = GetElement(titleInfo, "date");
        if (date != null)
        {
            metadata.PublishDate = date.Attribute("value")?.Value ?? date.Value.Trim();
        }

        // 解析封面
        var coverpage = GetElement(titleInfo, "coverpage");
        if (coverpage != null)
        {
            var image = GetElement(coverpage, "image");
            if (image != null)
            {
                // 获取图片引用（可能是 l:href 或 xlink:href）
                var href = image.Attribute(XlinkNs + "href")?.Value
                    ?? image.Attribute("href")?.Value
                    ?? image.Attributes().FirstOrDefault(a => a.Name.LocalName == "href")?.Value;

                if (!string.IsNullOrEmpty(href))
                {
                    metadata.CoverpageImageId = href.TrimStart('#');
                }
            }
        }

        // 解析语言
        var lang = GetElement(titleInfo, "lang");
        metadata.Language = lang?.Value.Trim();

        // 解析源语言
        var srcLang = GetElement(titleInfo, "src-lang");
        if (srcLang != null && string.IsNullOrEmpty(metadata.Language))
        {
            metadata.Language = srcLang.Value.Trim();
        }

        // 解析翻译者
        foreach (var translator in GetElements(titleInfo, "translator"))
        {
            var parsedTranslator = ParseAuthor(translator);
            if (!string.IsNullOrEmpty(parsedTranslator.GetDisplayName()))
            {
                metadata.Translators.Add(parsedTranslator);
            }
        }

        // 解析系列
        var sequence = GetElement(titleInfo, "sequence");
        if (sequence != null)
        {
            metadata.Sequence = ParseSequence(sequence);
        }
    }

    private static Fb2DocumentInfo ParseDocumentInfo(XElement documentInfo)
    {
        var info = new Fb2DocumentInfo();

        // 解析文档作者
        foreach (var author in GetElements(documentInfo, "author"))
        {
            var parsedAuthor = ParseAuthor(author);
            if (!string.IsNullOrEmpty(parsedAuthor.GetDisplayName()))
            {
                info.Authors.Add(parsedAuthor);
            }
        }

        // 解析创建程序
        var programUsed = GetElement(documentInfo, "program-used");
        info.ProgramUsed = programUsed?.Value.Trim();

        // 解析日期
        var date = GetElement(documentInfo, "date");
        if (date != null)
        {
            info.Date = date.Attribute("value")?.Value ?? date.Value.Trim();
        }

        // 解析源 URL
        foreach (var srcUrl in GetElements(documentInfo, "src-url"))
        {
            var url = srcUrl.Value.Trim();
            if (!string.IsNullOrEmpty(url))
            {
                info.SourceUrls.Add(url);
            }
        }

        // 解析 ID
        var id = GetElement(documentInfo, "id");
        info.Id = id?.Value.Trim();

        // 解析版本
        var version = GetElement(documentInfo, "version");
        info.Version = version?.Value.Trim();

        // 解析历史
        var history = GetElement(documentInfo, "history");
        if (history != null)
        {
            info.History = ExtractTextContent(history);
        }

        return info;
    }

    private static Fb2PublishInfo ParsePublishInfo(XElement publishInfo)
    {
        var info = new Fb2PublishInfo();

        var bookName = GetElement(publishInfo, "book-name");
        info.BookName = bookName?.Value.Trim();

        var publisher = GetElement(publishInfo, "publisher");
        info.Publisher = publisher?.Value.Trim();

        var city = GetElement(publishInfo, "city");
        info.City = city?.Value.Trim();

        var year = GetElement(publishInfo, "year");
        info.Year = year?.Value.Trim();

        var isbn = GetElement(publishInfo, "isbn");
        info.Isbn = isbn?.Value.Trim();

        var sequence = GetElement(publishInfo, "sequence");
        if (sequence != null)
        {
            info.Sequence = ParseSequence(sequence);
        }

        return info;
    }

    private static Fb2Author ParseAuthor(XElement authorElement)
    {
        var author = new Fb2Author();

        var firstName = GetElement(authorElement, "first-name");
        author.FirstName = firstName?.Value.Trim();

        var middleName = GetElement(authorElement, "middle-name");
        author.MiddleName = middleName?.Value.Trim();

        var lastName = GetElement(authorElement, "last-name");
        author.LastName = lastName?.Value.Trim();

        var nickname = GetElement(authorElement, "nickname");
        author.Nickname = nickname?.Value.Trim();

        var homePage = GetElement(authorElement, "home-page");
        author.HomePage = homePage?.Value.Trim();

        var email = GetElement(authorElement, "email");
        author.Email = email?.Value.Trim();

        var id = GetElement(authorElement, "id");
        author.Id = id?.Value.Trim();

        return author;
    }

    private static Fb2Sequence ParseSequence(XElement sequenceElement)
    {
        var sequence = new Fb2Sequence
        {
            Name = sequenceElement.Attribute("name")?.Value,
        };

        var numberStr = sequenceElement.Attribute("number")?.Value;
        if (int.TryParse(numberStr, out var number))
        {
            sequence.Number = number;
        }

        return sequence;
    }

    private static XElement? GetElement(XElement parent, string localName)
    {
        return parent.Element(FbNs + localName)
            ?? parent.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
    }

    private static List<XElement> GetElements(XElement parent, string localName)
    {
        var elements = parent.Elements(FbNs + localName).ToList();
        if (elements.Count == 0)
        {
            elements = parent.Elements().Where(e => e.Name.LocalName == localName).ToList();
        }

        return elements;
    }

    private static string ExtractTextContent(XElement element)
    {
        // 提取元素中的所有文本内容，处理段落
        var parts = new List<string>();

        foreach (var node in element.Nodes())
        {
            if (node is XText text)
            {
                var trimmed = text.Value.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    parts.Add(trimmed);
                }
            }
            else if (node is XElement child)
            {
                if (child.Name.LocalName == "p")
                {
                    var pText = child.Value.Trim();
                    if (!string.IsNullOrEmpty(pText))
                    {
                        parts.Add(pText);
                    }
                }
                else
                {
                    var childText = ExtractTextContent(child);
                    if (!string.IsNullOrEmpty(childText))
                    {
                        parts.Add(childText);
                    }
                }
            }
        }

        return string.Join("\n", parts);
    }
}
