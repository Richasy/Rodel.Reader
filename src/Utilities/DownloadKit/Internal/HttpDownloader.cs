// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.DownloadKit.Internal;

/// <summary>
/// HTTP 下载器，负责实际的下载操作.
/// </summary>
internal sealed class HttpDownloader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化 <see cref="HttpDownloader"/> 类的新实例.
    /// </summary>
    /// <param name="httpClient">HTTP 客户端.</param>
    /// <param name="logger">日志器.</param>
    public HttpDownloader(HttpClient httpClient, ILogger logger)
    {
        _httpClient = Guard.NotNull(httpClient);
        _logger = Guard.NotNull(logger);
    }

    /// <summary>
    /// 执行下载操作.
    /// </summary>
    /// <param name="uri">下载 URI.</param>
    /// <param name="destinationPath">目标文件路径.</param>
    /// <param name="options">下载选项.</param>
    /// <param name="progress">进度报告器.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>下载结果.</returns>
    public async Task<DownloadResult> DownloadAsync(
        Uri uri,
        string destinationPath,
        DownloadOptions options,
        IProgress<DownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        var tracker = new ProgressTracker(options.ProgressThrottleMs);

        try
        {
            _logger.LogInformation("开始下载: {Uri} -> {Path}", uri, destinationPath);

            // 验证并准备目标路径
            await PrepareDestinationAsync(destinationPath, options, cancellationToken).ConfigureAwait(false);

            // 创建请求
            using var request = CreateRequest(uri, options);

            // 发送请求获取响应
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);

            // 检查响应状态
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("HTTP 请求失败: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new DownloadException(
                    $"HTTP 请求失败: {response.StatusCode} - {response.ReasonPhrase}",
                    response.StatusCode,
                    uri);
            }

            // 获取内容长度
            var contentLength = response.Content.Headers.ContentLength;
            tracker.SetTotalBytes(contentLength);

            _logger.LogDebug("响应成功，内容长度: {ContentLength}", contentLength?.ToString() ?? "未知");

            // 开始下载
            tracker.Start();
            ReportProgress(progress, tracker.GetProgress());

            // 执行流式下载
            await DownloadStreamAsync(
                response,
                destinationPath,
                options,
                tracker,
                progress,
                cancellationToken).ConfigureAwait(false);

            // 完成
            tracker.Stop(DownloadState.Completed);
            var finalProgress = tracker.GetFinalProgress();
            ReportProgress(progress, finalProgress);

            _logger.LogInformation(
                "下载完成: {Path}, 大小: {Size} 字节, 耗时: {Elapsed}",
                destinationPath,
                tracker.BytesReceived,
                tracker.ElapsedTime);

            return DownloadResult.Success(destinationPath, tracker.BytesReceived, tracker.ElapsedTime);
        }
        catch (OperationCanceledException)
        {
            tracker.Stop(DownloadState.Canceled);
            ReportProgress(progress, tracker.GetFinalProgress());

            _logger.LogWarning("下载已取消: {Uri}, 已下载: {Bytes} 字节", uri, tracker.BytesReceived);

            // 清理部分下载的文件
            await CleanupPartialFileAsync(destinationPath).ConfigureAwait(false);

            return DownloadResult.Canceled(destinationPath, tracker.ElapsedTime, tracker.BytesReceived);
        }
        catch (DownloadIOException ex)
        {
            tracker.Stop(DownloadState.Failed);
            ReportProgress(progress, tracker.GetFinalProgress());

            _logger.LogError(ex, "IO 错误: {Uri}", uri);

            return DownloadResult.Failure(destinationPath, ex, tracker.ElapsedTime, tracker.BytesReceived);
        }
        catch (DownloadException)
        {
            tracker.Stop(DownloadState.Failed);
            ReportProgress(progress, tracker.GetFinalProgress());

            // 清理部分下载的文件
            await CleanupPartialFileAsync(destinationPath).ConfigureAwait(false);

            throw;
        }
        catch (Exception ex)
        {
            tracker.Stop(DownloadState.Failed);
            ReportProgress(progress, tracker.GetFinalProgress());

            _logger.LogError(ex, "下载失败: {Uri}", uri);

            // 清理部分下载的文件
            await CleanupPartialFileAsync(destinationPath).ConfigureAwait(false);

            return DownloadResult.Failure(destinationPath, ex, tracker.ElapsedTime, tracker.BytesReceived);
        }
    }

    /// <summary>
    /// 获取远程文件信息.
    /// </summary>
    /// <param name="uri">文件 URI.</param>
    /// <param name="options">下载选项.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>文件信息.</returns>
    public async Task<RemoteFileInfo> GetFileInfoAsync(
        Uri uri,
        DownloadOptions options,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("获取远程文件信息: {Uri}", uri);

        using var request = new HttpRequestMessage(HttpMethod.Head, uri);
        ApplyHeaders(request, options);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new DownloadException(
                $"无法获取文件信息: {response.StatusCode}",
                response.StatusCode,
                uri);
        }

        var info = new RemoteFileInfo
        {
            ContentLength = response.Content.Headers.ContentLength,
            ContentType = response.Content.Headers.ContentType?.MediaType,
            LastModified = response.Content.Headers.LastModified,
            ETag = response.Headers.ETag?.Tag,
            AcceptRanges = response.Headers.AcceptRanges.Contains("bytes"),
        };

        // 尝试从 Content-Disposition 获取文件名
        var contentDisposition = response.Content.Headers.ContentDisposition;
        if (contentDisposition != null)
        {
            info.FileName = contentDisposition.FileNameStar ?? contentDisposition.FileName;
        }

        _logger.LogDebug("文件信息: 大小={Size}, 类型={Type}", info.ContentLength, info.ContentType);

        return info;
    }

    private static HttpRequestMessage CreateRequest(Uri uri, DownloadOptions options)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        ApplyHeaders(request, options);
        return request;
    }

    private static void ApplyHeaders(HttpRequestMessage request, DownloadOptions options)
    {
        foreach (var header in options.Headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (!string.IsNullOrEmpty(options.UserAgent) &&
            !request.Headers.Contains("User-Agent"))
        {
            request.Headers.TryAddWithoutValidation("User-Agent", options.UserAgent);
        }
    }

    private async Task PrepareDestinationAsync(
        string destinationPath,
        DownloadOptions options,
        CancellationToken cancellationToken)
    {
        // 检查文件是否存在
        if (File.Exists(destinationPath))
        {
            if (options.OverwriteExisting)
            {
                _logger.LogDebug("删除已存在的文件: {Path}", destinationPath);
                File.Delete(destinationPath);
            }
            else
            {
                throw new DownloadIOException($"文件已存在: {destinationPath}");
            }
        }

        // 确保目录存在
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            _logger.LogDebug("创建目录: {Directory}", directory);
            Directory.CreateDirectory(directory);
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    private static async Task DownloadStreamAsync(
        HttpResponseMessage response,
        string destinationPath,
        DownloadOptions options,
        ProgressTracker tracker,
        IProgress<DownloadProgress>? progress,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[options.BufferSize];

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var fileStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            options.BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        int bytesRead;
        while ((bytesRead = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);

            tracker.UpdateBytesReceived(bytesRead);

            // 节流的进度报告
            if (tracker.ShouldReportProgress())
            {
                ReportProgress(progress, tracker.GetProgress());
            }
        }

        // 确保数据写入磁盘
        await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void ReportProgress(IProgress<DownloadProgress>? progress, DownloadProgress downloadProgress)
    {
        progress?.Report(downloadProgress);
    }

    private async Task CleanupPartialFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                _logger.LogDebug("清理部分下载的文件: {Path}", filePath);
                await Task.Run(() => File.Delete(filePath)).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清理部分文件失败: {Path}", filePath);
        }
    }
}
