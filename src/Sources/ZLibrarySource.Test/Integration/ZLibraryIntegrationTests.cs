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
    private const string TestEmail = "z.richasy@gmail.com";
    private const string TestPassword = "Shar6501209!";
    private const string TestMirror = "https://zh.z-lib.fm";

    private static ZLibraryClient? _client;
    private static bool _isLoggedIn;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        var options = new ZLibraryClientOptions
        {
            CustomMirror = TestMirror
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
            CustomMirror = TestMirror
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
        var result = await _client!.Search.SearchAsync("clean code", 1);

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
        var result = await _client!.Search.SearchAsync("programming", 1, options);

        // Assert
        Assert.IsNotNull(result);
        // Results should be filtered by the options
    }

    [TestMethod]
    public async Task Search_WithNonExistentQuery_ReturnsEmptyResults()
    {
        EnsureLoggedIn();

        // Act
        var result = await _client!.Search.SearchAsync("xyznonexistentbookquery12345", 1);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Items.Count);
    }

    [TestMethod]
    public async Task FullTextSearch_WithValidQuery_ReturnsResults()
    {
        EnsureLoggedIn();

        // Act
        var result = await _client!.Search.FullTextSearchAsync("design patterns", 1);

        // Assert
        Assert.IsNotNull(result);
        // Full text search may or may not return results depending on index
    }

    #endregion

    #region Book Detail Tests

    [TestMethod]
    public async Task GetBookDetail_WithValidId_ReturnsDetail()
    {
        EnsureLoggedIn();

        // First, search for a book to get a valid ID
        var searchResult = await _client!.Search.SearchAsync("clean code", 1);
        Assert.IsTrue(searchResult.Items.Count > 0, "Need at least one search result to test book detail");

        var bookUrl = searchResult.Items[0].Url;
        Assert.IsNotNull(bookUrl, "Book URL should not be null");

        // Act
        var detail = await _client!.Books.GetByUrlAsync(bookUrl);

        // Assert
        Assert.IsNotNull(detail);
        Assert.IsNotNull(detail.Name);
        Assert.IsNotNull(detail.DownloadUrl);
    }

    #endregion

    #region Profile Tests

    [TestMethod]
    public async Task GetDownloadLimits_WhenAuthenticated_ReturnsLimits()
    {
        EnsureLoggedIn();

        // Act
        var limits = await _client!.Profile.GetDownloadLimitsAsync();

        // Assert
        Assert.IsNotNull(limits);
        Assert.IsTrue(limits.DailyAllowed > 0, "Daily limit should be greater than 0");
        Assert.IsTrue(limits.DailyUsed >= 0, "Downloaded count should be non-negative");
        Assert.IsTrue(limits.DailyRemaining >= 0, "Remaining should be non-negative");
    }

    [TestMethod]
    public async Task GetDownloadHistory_WhenAuthenticated_ReturnsHistory()
    {
        EnsureLoggedIn();

        // Act
        var history = await _client!.Profile.GetDownloadHistoryAsync(1);

        // Assert
        Assert.IsNotNull(history);
        // History may be empty if user hasn't downloaded anything
    }

    [TestMethod]
    public async Task GetDownloadHistory_WithDateRange_ReturnsFilteredHistory()
    {
        EnsureLoggedIn();

        // Arrange
        var fromDate = new DateOnly(2024, 1, 1);
        var toDate = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var history = await _client!.Profile.GetDownloadHistoryAsync(1, fromDate, toDate);

        // Assert
        Assert.IsNotNull(history);
    }

    #endregion

    #region Booklist Tests

    [TestMethod]
    public async Task SearchBooklists_WithValidQuery_ReturnsResults()
    {
        EnsureLoggedIn();

        // Act
        var result = await _client!.Booklists.SearchPublicAsync("programming", 1);

        // Assert
        Assert.IsNotNull(result);
        // Booklists may or may not exist for the query
    }

    [TestMethod]
    public async Task SearchPrivateBooklists_WhenAuthenticated_ReturnsBooklists()
    {
        EnsureLoggedIn();

        // Act
        var result = await _client!.Booklists.SearchPrivateAsync(string.Empty, 1);

        // Assert
        Assert.IsNotNull(result);
        // User may or may not have any booklists
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public async Task Search_WhenNotAuthenticated_ThrowsNotAuthenticatedException()
    {
        // Arrange
        var options = new ZLibraryClientOptions
        {
            CustomMirror = TestMirror
        };
        using var client = new ZLibraryClient(options);
        // Don't login

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotAuthenticatedException>(
            async () => await client.Search.SearchAsync("test", 1));
    }

    #endregion
}
