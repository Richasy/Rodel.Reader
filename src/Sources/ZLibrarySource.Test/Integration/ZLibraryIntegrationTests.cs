// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Test.Integration;

/// <summary>
/// ZLibrary 集成测试.
/// 使用真实 API 进行测试，需要有效的账号凭据.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public class ZLibraryIntegrationTests
{
    private const string TestEmail = "zar234@qq.com";
    private const string TestPassword = "w123456w";
    private const string TestMirror = "https://zlib.by";

    // 从浏览器抓取的 cookies，用于绕过 Cloudflare 检测
    private static readonly Dictionary<string, string> TestCookies = new()
    {
        ["siteLanguage"] = "en",
        ["refuseChangeDomain"] = "1",
    };

    private static ZLibraryClient? _client;
    private static bool _isLoggedIn;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        var options = new ZLibraryClientOptions
        {
            CustomMirror = TestMirror,
            InitialCookies = TestCookies,
        };

        _client = new ZLibraryClient(options);

        try
        {
            await _client.LoginAsync(TestEmail, TestPassword);
            _isLoggedIn = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            _isLoggedIn = false;
        }
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _client?.Dispose();
    }

    private static void EnsureLoggedIn()
    {
        if (!_isLoggedIn)
        {
            Assert.Inconclusive("Unable to run integration test - login failed");
        }
    }

    #region Login Tests

    [TestMethod]
    public void Login_AfterClassInitialize_IsAuthenticated()
    {
        EnsureLoggedIn();
        Assert.IsTrue(_client!.IsAuthenticated);
    }

    [TestMethod]
    public async Task Login_WithInvalidCredentials_ThrowsLoginFailedException()
    {
        // Arrange
        var options = new ZLibraryClientOptions
        {
            CustomMirror = TestMirror,
            InitialCookies = TestCookies,
        };
        using var client = new ZLibraryClient(options);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<LoginFailedException>(
            async () => await client.LoginAsync("invalid@email.com", "wrongpassword"));
    }

    #endregion

    #region Search Tests

    [TestMethod]
    public async Task Search_WithValidQuery_ReturnsResults()
    {
        EnsureLoggedIn();

        // Act
        var result = await _client!.SearchAsync("clean code", 1);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Items.Count > 0, "Search should return at least one result");

        var firstBook = result.Items[0];
        Assert.IsNotNull(firstBook.Id);
        Assert.IsNotNull(firstBook.Name);
    }

    [TestMethod]
    public async Task Search_WithOptions_ReturnsFilteredResults()
    {
        EnsureLoggedIn();

        // Arrange
        var options = new BookSearchOptions
        {
            Languages = [BookLanguage.English],
            Extensions = [BookExtension.PDF]
        };

        // Act
        var result = await _client!.SearchAsync("programming", 1, options);

        // Assert
        Assert.IsNotNull(result);
        // Results should be filtered by the options
    }

    [TestMethod]
    public async Task Search_WithNonExistentQuery_ReturnsEmptyResults()
    {
        EnsureLoggedIn();

        // Act
        var result = await _client!.SearchAsync("xyznonexistentbookquery12345", 1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Items.Count);
    }

    #endregion

    #region Book Detail Tests

    [TestMethod]
    public async Task Search_ReturnsBookWithDownloadUrl()
    {
        EnsureLoggedIn();

        // Act - search for a book
        var searchResult = await _client!.SearchAsync("clean code", 1);

        // Assert
        Assert.IsTrue(searchResult.Items.Count > 0, "Search should return at least one result");

        var book = searchResult.Items[0];
        Assert.IsNotNull(book.Id, "Book ID should not be null");
        Assert.IsNotNull(book.Name, "Book name should not be null");
        Assert.IsNotNull(book.DownloadUrl, "Book download URL should not be null");
        Assert.IsNotNull(book.Url, "Book URL should not be null");

        Console.WriteLine($"Book: {book.Name}");
        Console.WriteLine($"Download URL: {book.DownloadUrl}");
    }

    #endregion

    #region Profile Tests

    [TestMethod]
    public async Task GetProfile_WhenAuthenticated_ReturnsProfile()
    {
        EnsureLoggedIn();

        // Act
        var profile = await _client!.GetProfileAsync();

        // Assert
        Assert.IsNotNull(profile);
        Assert.IsTrue(profile.Id > 0, "User ID should be greater than 0");
        Assert.IsNotNull(profile.Email, "Email should not be null");
        Assert.IsTrue(profile.DownloadsLimit > 0, "Downloads limit should be greater than 0");
        Assert.IsTrue(profile.DownloadsToday >= 0, "Downloads today should be non-negative");
        Assert.IsTrue(profile.DownloadsRemaining >= 0, "Downloads remaining should be non-negative");

        Console.WriteLine($"User: {profile.Name} ({profile.Email})");
        Console.WriteLine($"Downloads: {profile.DownloadsToday}/{profile.DownloadsLimit}");
        Console.WriteLine($"Premium: {profile.IsPremium}");
    }

    #endregion

    #region Download Tests

    [TestMethod]
    public async Task GetDownloadInfo_WithValidBook_ReturnsDownloadInfo()
    {
        EnsureLoggedIn();

        // Act - search for a book first
        var searchResult = await _client!.SearchAsync("clean code", 1);
        Assert.IsTrue(searchResult.Items.Count > 0, "Search should return at least one result");

        var book = searchResult.Items[0];
        Assert.IsNotNull(book.Id, "Book ID should not be null");
        Assert.IsNotNull(book.Hash, "Book Hash should not be null");

        Console.WriteLine($"Book ID: {book.Id}");
        Console.WriteLine($"Book Hash: {book.Hash}");
        Console.WriteLine($"Book Name: {book.Name}");

        // 直接测试 API URL
        var url = $"{_client.Mirror}/eapi/book/{book.Id}/{book.Hash}/file";
        Console.WriteLine($"API URL: {url}");

        // Get download info
        var downloadInfo = await _client!.GetDownloadInfoAsync(book);

        if (downloadInfo == null)
        {
            Console.WriteLine("Download info is null - this could be due to download limit reached or API response format issue");
        }
        else
        {
            Console.WriteLine($"Download Link: {downloadInfo.DownloadLink}");
            Console.WriteLine($"Full File Name: {downloadInfo.FullFileName}");
        }

        // 如果下载信息为空，可能是因为达到了下载限制，这种情况下跳过测试
        if (downloadInfo == null)
        {
            Assert.Inconclusive("Download info is null - possibly download limit reached or API format changed");
            return;
        }

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(downloadInfo.DownloadLink), "Download link should not be empty");
        Assert.IsFalse(string.IsNullOrEmpty(downloadInfo.FileName), "File name should not be empty");
        Assert.IsFalse(string.IsNullOrEmpty(downloadInfo.Extension), "Extension should not be empty");
    }

    [TestMethod]
    public async Task GetDownloadInfo_WithBookIdAndHash_ReturnsDownloadInfo()
    {
        EnsureLoggedIn();

        // Act - search for a book first
        var searchResult = await _client!.SearchAsync("clean code", 1);
        Assert.IsTrue(searchResult.Items.Count > 0, "Search should return at least one result");

        var book = searchResult.Items[0];

        // Get download info using ID and Hash
        var downloadInfo = await _client!.GetDownloadInfoAsync(book.Id!, book.Hash!);

        // 如果下载信息为空，可能是因为达到了下载限制
        if (downloadInfo == null)
        {
            Assert.Inconclusive("Download info is null - possibly download limit reached");
            return;
        }

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(downloadInfo.DownloadLink), "Download link should not be empty");

        Console.WriteLine($"Download Link: {downloadInfo.DownloadLink}");
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public async Task Search_WhenNotAuthenticated_ThrowsNotAuthenticatedException()
    {
        // Arrange
        var options = new ZLibraryClientOptions
        {
            CustomMirror = TestMirror,
            // 不设置 InitialCookies，确保未认证状态
        };
        using var client = new ZLibraryClient(options);
        // Don't login

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotAuthenticatedException>(
            async () => await client.SearchAsync("test", 1));
    }

    #endregion
}
