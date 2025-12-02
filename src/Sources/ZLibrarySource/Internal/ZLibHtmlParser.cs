// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Internal;

/// <summary>
/// HTML 解析器实现.
/// </summary>
internal sealed class ZLibHtmlParser : IHtmlParser
{
    private readonly ILogger _logger;
    private readonly IBrowsingContext _browsingContext;

    /// <summary>
    /// 初始化 <see cref="ZLibHtmlParser"/> 类的新实例.
    /// </summary>
    /// <param name="logger">日志器.</param>
    public ZLibHtmlParser(ILogger logger)
    {
        _logger = logger;
        var config = Configuration.Default;
        _browsingContext = BrowsingContext.New(config);
    }

    /// <inheritdoc/>
    public (List<BookItem> Books, int TotalPages) ParseSearchResults(string html, string mirror)
    {
        var document = _browsingContext.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();
        var books = new List<BookItem>();

        var searchBox = document.QuerySelector("#searchResultBox");
        if (searchBox == null)
        {
            _logger.LogWarning("Could not find search result box");
            return (books, 0);
        }

        // 检查是否没有结果
        var notFound = document.QuerySelector(".notFound");
        if (notFound != null)
        {
            _logger.LogDebug("No results found");
            return (books, 0);
        }

        // 解析书籍列表
        var bookItems = searchBox.QuerySelectorAll(".book-item");
        foreach (var item in bookItems)
        {
            try
            {
                var book = ParseBookItem(item, mirror);
                if (book != null)
                {
                    books.Add(book);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse book item");
            }
        }

        // 解析总页数
        var totalPages = ParseTotalPages(document);

        return (books, totalPages);
    }

    /// <inheritdoc/>
    public BookDetail ParseBookDetail(string html, string url, string mirror)
    {
        var document = _browsingContext.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();

        var wrap = document.QuerySelector(".row.cardBooks");
        if (wrap == null)
        {
            throw new ParseException($"Failed to parse book detail page: {url}");
        }

        var zcover = document.QuerySelector("z-cover");
        if (zcover == null)
        {
            throw new ParseException($"Failed to find z-cover in: {url}");
        }

        // 获取书籍 ID（从 URL 提取）
        var bookId = ExtractBookIdFromUrl(url);

        // 获取标题
        var title = zcover.GetAttribute("title")?.Trim() ?? "Unknown";

        // 获取封面
        var coverImg = zcover.QuerySelector("img.image");
        var coverUrl = coverImg?.GetAttribute("src");

        // 获取作者
        var authors = new List<BookAuthor>();
        var col = wrap.QuerySelector(".col-sm-9");
        if (col != null)
        {
            var anchors = col.QuerySelectorAll("a");
            foreach (var anchor in anchors)
            {
                var href = anchor.GetAttribute("href");
                if (!string.IsNullOrEmpty(href) && href.Contains("/g/", StringComparison.Ordinal))
                {
                    authors.Add(new BookAuthor
                    {
                        Name = anchor.TextContent.Trim(),
                        Url = $"{mirror}{href}",
                    });
                }
            }
        }

        // 获取描述
        var descBox = wrap.QuerySelector("#bookDescriptionBox");
        var description = descBox?.TextContent.Trim();

        // 获取详细属性
        var detailsBox = wrap.QuerySelector(".bookDetailsBox");
        string? year = null, edition = null, publisher = null, language = null;
        string? isbn10 = null, isbn13 = null;
        string? categoryName = null, categoryUrl = null;
        string? extension = null, fileSize = null;

        if (detailsBox != null)
        {
            year = GetPropertyValue(detailsBox, "year");
            edition = GetPropertyValue(detailsBox, "edition");
            publisher = GetPropertyValue(detailsBox, "publisher");
            language = GetPropertyValue(detailsBox, "language");

            // 解析 ISBN
            var isbns = detailsBox.QuerySelectorAll(".property_isbn");
            foreach (var isbn in isbns)
            {
                var label = isbn.QuerySelector(".property_label")?.TextContent.Trim().TrimEnd(':');
                var value = isbn.QuerySelector(".property_value")?.TextContent.Trim();
                if (label?.Contains("10", StringComparison.Ordinal) == true)
                {
                    isbn10 = value;
                }
                else if (label?.Contains("13", StringComparison.Ordinal) == true)
                {
                    isbn13 = value;
                }
            }

            // 解析分类
            var catProp = detailsBox.QuerySelector(".property_categories");
            if (catProp != null)
            {
                var catValue = catProp.QuerySelector(".property_value");
                if (catValue != null)
                {
                    categoryName = catValue.TextContent.Trim();
                    var catLink = catValue.QuerySelector("a");
                    if (catLink != null)
                    {
                        categoryUrl = $"{mirror}{catLink.GetAttribute("href")}";
                    }
                }
            }

            // 解析文件信息
            var fileProp = detailsBox.QuerySelector(".property__file");
            if (fileProp != null)
            {
                var fileText = fileProp.TextContent.Trim();
                var parts = fileText.Split(',');
                if (parts.Length >= 1)
                {
                    extension = parts[0].Split('\n').LastOrDefault()?.Trim();
                }

                if (parts.Length >= 2)
                {
                    fileSize = parts[1].Trim();
                }
            }
        }

        // 获取评分
        var ratingElement = wrap.QuerySelector(".book-rating");
        var rating = ratingElement?.TextContent.Replace("\n", string.Empty, StringComparison.Ordinal).Replace(" ", string.Empty, StringComparison.Ordinal).Trim();

        // 获取下载链接
        var dlBtn = document.QuerySelector("a.btn.btn-default.addDownloadedBook");
        string? downloadUrl = null;
        var isDownloadAvailable = false;
        if (dlBtn != null)
        {
            var btnText = dlBtn.TextContent;
            if (btnText.Contains("unavailable", StringComparison.OrdinalIgnoreCase))
            {
                downloadUrl = null;
                isDownloadAvailable = false;
            }
            else
            {
                downloadUrl = $"{mirror}{dlBtn.GetAttribute("href")}";
                isDownloadAvailable = true;
            }
        }

        return new BookDetail
        {
            Id = bookId,
            Name = title,
            Url = url,
            CoverUrl = coverUrl,
            Description = description,
            Authors = authors.Count > 0 ? authors : null,
            Publisher = publisher,
            Year = year,
            Edition = edition,
            Language = language,
            Isbn10 = isbn10,
            Isbn13 = isbn13,
            Category = categoryName != null ? new BookCategory { Name = categoryName, Url = categoryUrl } : null,
            Extension = extension,
            FileSize = fileSize,
            Rating = rating,
            DownloadUrl = downloadUrl,
            IsDownloadAvailable = isDownloadAvailable,
        };
    }

    /// <inheritdoc/>
    public DownloadLimits ParseDownloadLimits(string html)
    {
        var document = _browsingContext.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();

        var dstats = document.QuerySelector(".dstats-info");
        if (dstats == null)
        {
            throw new ParseException("Could not parse download limit page");
        }

        var dCount = dstats.QuerySelector(".d-count");
        if (dCount == null)
        {
            throw new ParseException("Could not parse download count");
        }

        var countText = dCount.TextContent.Trim().Split('/');
        var dailyUsed = int.Parse(countText[0].Trim());
        var dailyAllowed = int.Parse(countText[1].Trim());

        var dReset = dstats.QuerySelector(".d-reset");
        var resetTime = dReset?.TextContent.Trim();

        return new DownloadLimits
        {
            DailyUsed = dailyUsed,
            DailyAllowed = dailyAllowed,
            ResetTime = resetTime,
        };
    }

    /// <inheritdoc/>
    public List<DownloadHistoryItem> ParseDownloadHistory(string html, string mirror)
    {
        var document = _browsingContext.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();
        var items = new List<DownloadHistoryItem>();

        var box = document.QuerySelector(".dstats-content");
        if (box == null)
        {
            throw new ParseException("Could not parse download history page");
        }

        // 检查是否为空
        var notFound = box.QuerySelector("p");
        if (notFound != null && notFound.TextContent.Contains("Downloads not found", StringComparison.Ordinal))
        {
            return items;
        }

        var rows = box.QuerySelectorAll("tr.dstats-row");
        foreach (var row in rows)
        {
            var titleDiv = row.QuerySelector(".book-title");
            var dateCell = row.QuerySelector("td.lg-w-120");

            if (titleDiv == null)
            {
                continue;
            }

            var name = titleDiv.TextContent.Trim();
            var date = dateCell?.TextContent.Trim();

            var anchor = row.QuerySelector("a");
            var url = anchor != null ? $"{mirror}{anchor.GetAttribute("href")}" : null;

            items.Add(new DownloadHistoryItem
            {
                Name = name,
                Url = url,
                Date = date,
            });
        }

        return items;
    }

    /// <inheritdoc/>
    public (List<Booklist> Booklists, int TotalPages) ParseBooklistResults(string html, string mirror)
    {
        var document = _browsingContext.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();
        var booklists = new List<Booklist>();

        // 检查是否没有结果
        var notFound = document.QuerySelector(".cBox1");
        if (notFound != null && notFound.TextContent.Contains("On your request nothing has been found", StringComparison.Ordinal))
        {
            return (booklists, 0);
        }

        var booklistElements = document.QuerySelectorAll("z-booklist");
        foreach (var element in booklistElements)
        {
            try
            {
                var name = element.GetAttribute("topic")?.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var url = element.GetAttribute("href");
                var description = element.GetAttribute("description")?.Trim();
                var author = element.GetAttribute("authorprofile")?.Trim();
                var count = element.GetAttribute("quantity")?.Trim();
                var views = element.GetAttribute("views")?.Trim();

                // 解析预览书籍
                var previewBooks = new List<BookItem>();
                var carousel = element.QuerySelectorAll("a");
                foreach (var bookAnchor in carousel)
                {
                    var zcover = bookAnchor.QuerySelector("z-cover");
                    if (zcover == null)
                    {
                        continue;
                    }

                    var bookId = zcover.GetAttribute("id")?.Trim();
                    var bookTitle = zcover.GetAttribute("title")?.Trim();

                    if (!string.IsNullOrEmpty(bookId) && !string.IsNullOrEmpty(bookTitle))
                    {
                        previewBooks.Add(new BookItem
                        {
                            Id = bookId,
                            Name = bookTitle,
                            Url = $"{mirror}{bookAnchor.GetAttribute("href")}",
                        });
                    }
                }

                booklists.Add(new Booklist
                {
                    Name = name,
                    Url = url != null ? $"{mirror}{url}" : null,
                    Description = description,
                    Author = author,
                    BookCount = count,
                    Views = views,
                    PreviewBooks = previewBooks.Count > 0 ? previewBooks : null,
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse booklist item");
            }
        }

        var totalPages = ParseTotalPages(document);
        return (booklists, totalPages);
    }

    private static BookItem? ParseBookItem(IElement item, string mirror)
    {
        var bookcard = item.QuerySelector("z-bookcard");
        if (bookcard == null)
        {
            return null;
        }

        var id = bookcard.GetAttribute("id");
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var isbn = bookcard.GetAttribute("isbn");
        var href = bookcard.GetAttribute("href");
        var url = href != null ? $"{mirror}{href}" : null;

        // 封面
        var coverElement = bookcard.QuerySelector("img");
        var coverUrl = coverElement?.GetAttribute("data-src") ?? coverElement?.GetAttribute("src");

        // 作者
        var authorSlot = bookcard.QuerySelector("[slot='author']");
        var authors = authorSlot?.TextContent
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrEmpty(a))
            .ToList();

        // 标题
        var titleSlot = bookcard.QuerySelector("[slot='title']");
        var name = titleSlot?.TextContent.Trim() ?? "Unknown";

        // 其他属性
        var publisher = bookcard.GetAttribute("publisher")?.Trim();
        var year = bookcard.GetAttribute("year")?.Trim();
        var language = bookcard.GetAttribute("language")?.Trim();
        var extension = bookcard.GetAttribute("extension")?.Trim();
        var fileSize = bookcard.GetAttribute("filesize")?.Trim();
        var rating = bookcard.GetAttribute("rating")?.Trim();
        var quality = bookcard.GetAttribute("quality")?.Trim();

        return new BookItem
        {
            Id = id,
            Name = name,
            Isbn = isbn,
            Url = url,
            CoverUrl = coverUrl,
            Authors = authors,
            Publisher = publisher,
            Year = year,
            Language = language,
            Extension = extension,
            FileSize = fileSize,
            Rating = rating,
            Quality = quality,
        };
    }

    private static int ParseTotalPages(IDocument document)
    {
        var scripts = document.QuerySelectorAll("script");
        foreach (var script in scripts)
        {
            var text = script.TextContent;
            if (text.Contains("var pagerOptions", StringComparison.Ordinal))
            {
                var pos = text.IndexOf("pagesTotal:", StringComparison.Ordinal);
                if (pos >= 0)
                {
                    var start = pos + "pagesTotal:".Length;
                    var end = text.IndexOf(',', start);
                    if (end > start)
                    {
                        var countStr = text[start..end].Trim();
                        if (int.TryParse(countStr, out var count))
                        {
                            return count;
                        }
                    }
                }
            }
        }

        return 1;
    }

    private static string? GetPropertyValue(IElement detailsBox, string propertyName)
    {
        var prop = detailsBox.QuerySelector($".property_{propertyName}");
        return prop?.QuerySelector(".property_value")?.TextContent.Trim();
    }

    private static string ExtractBookIdFromUrl(string url)
    {
        // URL 格式: https://z-library.sk/book/123456/...
        var segments = url.Split('/');
        for (var i = 0; i < segments.Length; i++)
        {
            if (segments[i] == "book" && i + 1 < segments.Length)
            {
                return segments[i + 1];
            }
        }

        return "unknown";
    }
}
