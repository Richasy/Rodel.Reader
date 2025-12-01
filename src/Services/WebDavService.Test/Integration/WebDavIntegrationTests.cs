// Copyright (c) Richasy. All rights reserved.

using System.Text;

namespace Richasy.RodelReader.Services.WebDav.Test.Integration;

/// <summary>
/// WebDAV 集成测试.
/// 使用内嵌的 SimpleWebDavServer 进行测试.
/// </summary>
[TestClass]
[TestCategory("Integration")]
public class WebDavIntegrationTests : IDisposable
{
    private const int ServerPort = 18088;

    private static SimpleWebDavServer? _server;
    private static WebDavClient? _client;
    private static string? _testDataPath;
    private bool _disposed;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        // 创建临时测试数据目录
        _testDataPath = Path.Combine(Path.GetTempPath(), $"WebDavTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDataPath);

        // 创建测试文件
        await File.WriteAllTextAsync(Path.Combine(_testDataPath, "test.txt"), "Hello, WebDAV!");

        // 启动内嵌 WebDAV 服务器
        _server = new SimpleWebDavServer(ServerPort, _testDataPath);
        _server.Start();

        // 等待服务器启动
        await Task.Delay(500);

        // 创建客户端
        var options = new WebDavClientOptions
        {
            BaseAddress = new Uri(_server.BaseUrl),
        };
        _client = new WebDavClient(options);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _client?.Dispose();

        if (_server != null)
        {
            await _server.StopAsync();
            _server.Dispose();
        }

        // 清理测试数据目录
        if (_testDataPath != null && Directory.Exists(_testDataPath))
        {
            try
            {
                Directory.Delete(_testDataPath, recursive: true);
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    #region PROPFIND Tests

    [TestMethod]
    public async Task Propfind_RootDirectory_ReturnsResources()
    {
        // Act
        var result = await _client!.Properties.PropfindAsync(
            new Uri("/", UriKind.Relative),
            new PropfindParameters
            {
                RequestType = PropfindRequestType.AllProperties,
                ApplyTo = ApplyTo.Propfind.ResourceAndChildren,
            });

        // Assert
        Assert.IsTrue(result.IsSuccessful, $"Status: {result.StatusCode}");
        Assert.IsTrue(result.Resources.Count > 0, "Should have at least root resource");
    }

    [TestMethod]
    public async Task Propfind_ExistingFile_ReturnsFileProperties()
    {
        // Act
        var result = await _client!.Properties.PropfindAsync(
            new Uri("/test.txt", UriKind.Relative),
            new PropfindParameters
            {
                RequestType = PropfindRequestType.AllProperties,
            });

        // Assert
        Assert.IsTrue(result.IsSuccessful);
        Assert.AreEqual(1, result.Resources.Count);

        var file = result.Resources.First();
        Assert.IsFalse(file.IsCollection);
    }

    #endregion

    #region MKCOL Tests

    [TestMethod]
    public async Task MkCol_CreateNewFolder_Succeeds()
    {
        var folderName = $"test-folder-{Guid.NewGuid():N}";

        try
        {
            // Act
            var result = await _client!.Resources.MkColAsync(new Uri($"/{folderName}", UriKind.Relative));

            // Assert
            Assert.IsTrue(result.IsSuccessful, $"Status: {result.StatusCode}");
        }
        finally
        {
            // Cleanup
            await _client!.Resources.DeleteAsync(new Uri($"/{folderName}", UriKind.Relative));
        }
    }

    #endregion

    #region PUT/GET Tests

    [TestMethod]
    public async Task PutAndGet_UploadAndDownloadFile_RoundTrips()
    {
        var fileName = $"upload-test-{Guid.NewGuid():N}.txt";
        var content = "Hello, WebDAV Integration Test!";

        try
        {
            // Upload
            using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var putResult = await _client!.Files.PutFileAsync(
                new Uri($"/{fileName}", UriKind.Relative),
                uploadStream);

            Assert.IsTrue(putResult.IsSuccessful, $"PUT failed: {putResult.StatusCode}");

            // Download
            using var getResult = await _client.Files.GetRawFileAsync(
                new Uri($"/{fileName}", UriKind.Relative));

            Assert.IsTrue(getResult.IsSuccessful, $"GET failed: {getResult.StatusCode}");

            using var reader = new StreamReader(getResult.Stream);
            var downloadedContent = await reader.ReadToEndAsync();

            Assert.AreEqual(content, downloadedContent);
        }
        finally
        {
            // Cleanup
            await _client!.Resources.DeleteAsync(new Uri($"/{fileName}", UriKind.Relative));
        }
    }

    #endregion

    #region DELETE Tests

    [TestMethod]
    public async Task Delete_ExistingFile_Succeeds()
    {
        var fileName = $"delete-test-{Guid.NewGuid():N}.txt";

        // Create file first
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Delete me"));
        await _client!.Files.PutFileAsync(new Uri($"/{fileName}", UriKind.Relative), stream);

        // Act
        var result = await _client.Resources.DeleteAsync(new Uri($"/{fileName}", UriKind.Relative));

        // Assert
        Assert.IsTrue(result.IsSuccessful, $"Status: {result.StatusCode}");

        // Verify deleted
        var propfindResult = await _client.Properties.PropfindAsync(
            new Uri($"/{fileName}", UriKind.Relative),
            new PropfindParameters());

        Assert.AreEqual(404, propfindResult.StatusCode);
    }

    #endregion

    #region COPY Tests

    [TestMethod]
    public async Task Copy_ExistingFile_CreatesNewFile()
    {
        var sourceFile = $"copy-source-{Guid.NewGuid():N}.txt";
        var destFile = $"copy-dest-{Guid.NewGuid():N}.txt";
        var content = "Copy test content";

        try
        {
            // Create source
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await _client!.Files.PutFileAsync(new Uri($"/{sourceFile}", UriKind.Relative), stream);

            // Act
            var result = await _client.Resources.CopyAsync(
                new Uri($"/{sourceFile}", UriKind.Relative),
                new Uri($"/{destFile}", UriKind.Relative));

            Assert.IsTrue(result.IsSuccessful, $"COPY failed: {result.StatusCode}");

            // Verify destination exists
            using var getResult = await _client.Files.GetRawFileAsync(
                new Uri($"/{destFile}", UriKind.Relative));

            Assert.IsTrue(getResult.IsSuccessful);
        }
        finally
        {
            await _client!.Resources.DeleteAsync(new Uri($"/{sourceFile}", UriKind.Relative));
            await _client.Resources.DeleteAsync(new Uri($"/{destFile}", UriKind.Relative));
        }
    }

    #endregion

    #region MOVE Tests

    [TestMethod]
    public async Task Move_ExistingFile_MovesToNewLocation()
    {
        var sourceFile = $"move-source-{Guid.NewGuid():N}.txt";
        var destFile = $"move-dest-{Guid.NewGuid():N}.txt";
        var content = "Move test content";

        try
        {
            // Create source
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await _client!.Files.PutFileAsync(new Uri($"/{sourceFile}", UriKind.Relative), stream);

            // Act
            var result = await _client.Resources.MoveAsync(
                new Uri($"/{sourceFile}", UriKind.Relative),
                new Uri($"/{destFile}", UriKind.Relative));

            Assert.IsTrue(result.IsSuccessful, $"MOVE failed: {result.StatusCode}");

            // Verify source doesn't exist
            var sourceResult = await _client.Properties.PropfindAsync(
                new Uri($"/{sourceFile}", UriKind.Relative),
                new PropfindParameters());
            Assert.AreEqual(404, sourceResult.StatusCode);

            // Verify destination exists
            using var getResult = await _client.Files.GetRawFileAsync(
                new Uri($"/{destFile}", UriKind.Relative));
            Assert.IsTrue(getResult.IsSuccessful);
        }
        finally
        {
            await _client!.Resources.DeleteAsync(new Uri($"/{destFile}", UriKind.Relative));
        }
    }

    #endregion

    #region LOCK/UNLOCK Tests

    [TestMethod]
    public async Task LockAndUnlock_File_Succeeds()
    {
        var fileName = $"lock-test-{Guid.NewGuid():N}.txt";

        try
        {
            // Create file
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Lock me"));
            await _client!.Files.PutFileAsync(new Uri($"/{fileName}", UriKind.Relative), stream);

            // Lock
            var lockResult = await _client.Locks.LockAsync(
                new Uri($"/{fileName}", UriKind.Relative),
                new LockParameters
                {
                    LockScope = LockScope.Exclusive,
                    Owner = new PrincipalLockOwner("testuser"),
                });

            // 某些服务器可能不支持锁定
            if (lockResult.StatusCode == 501 || lockResult.StatusCode == 405)
            {
                Assert.Inconclusive("Server does not support LOCK");
                return;
            }

            Assert.IsTrue(lockResult.IsSuccessful, $"LOCK failed: {lockResult.StatusCode}");
            Assert.IsNotNull(lockResult.LockToken);

            // Unlock
            var unlockResult = await _client.Locks.UnlockAsync(
                new Uri($"/{fileName}", UriKind.Relative),
                new UnlockParameters(lockResult.LockToken));

            Assert.IsTrue(unlockResult.IsSuccessful, $"UNLOCK failed: {unlockResult.StatusCode}");
        }
        finally
        {
            await _client!.Resources.DeleteAsync(new Uri($"/{fileName}", UriKind.Relative));
        }
    }

    #endregion

    #region PROPPATCH Tests

    [TestMethod]
    public async Task Proppatch_SetDisplayName_Succeeds()
    {
        var fileName = $"proppatch-test-{Guid.NewGuid():N}.txt";

        try
        {
            // Create file
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Proppatch test"));
            await _client!.Files.PutFileAsync(new Uri($"/{fileName}", UriKind.Relative), stream);

            // Set property
            var result = await _client.Properties.ProppatchAsync(
                new Uri($"/{fileName}", UriKind.Relative),
                new ProppatchParameters
                {
                    PropertiesToSet =
                    [
                        new WebDavProperty("displayname", WebDavConstants.DavNamespace, "Custom Name"),
                    ],
                });

            // 某些服务器可能不支持修改属性
            if (result.StatusCode == 403 || result.StatusCode == 501)
            {
                Assert.Inconclusive("Server does not support PROPPATCH");
                return;
            }

            Assert.IsTrue(result.IsSuccessful, $"PROPPATCH failed: {result.StatusCode}");
        }
        finally
        {
            await _client!.Resources.DeleteAsync(new Uri($"/{fileName}", UriKind.Relative));
        }
    }

    #endregion
}
