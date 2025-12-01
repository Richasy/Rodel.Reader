// Copyright (c) Richasy. All rights reserved.

using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Richasy.RodelReader.Services.WebDav.Test.Integration;

/// <summary>
/// 简单的 WebDAV 服务器，用于集成测试.
/// 基于 HttpListener 实现基本的 WebDAV 功能.
/// </summary>
public sealed class SimpleWebDavServer : IDisposable
{
    private static readonly XNamespace DavNs = "DAV:";

    private readonly HttpListener _listener;
    private readonly string _rootPath;
    private readonly CancellationTokenSource _cts;
    private Task? _serverTask;
    private bool _disposed;

    public SimpleWebDavServer(int port, string rootPath)
    {
        Port = port;
        _rootPath = rootPath;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://localhost:{port}/");
        _cts = new CancellationTokenSource();

        Directory.CreateDirectory(_rootPath);
    }

    public int Port { get; }

    public string BaseUrl => $"http://localhost:{Port}";

    public void Start()
    {
        _listener.Start();
        _serverTask = Task.Run(async () => await ListenAsync(_cts.Token));
    }

    public async Task StopAsync()
    {
        await _cts.CancelAsync();
        _listener.Stop();

        if (_serverTask != null)
        {
            try
            {
                await _serverTask;
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cts.Cancel();
            _listener.Close();
            _cts.Dispose();
            _disposed = true;
        }
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => HandleRequestAsync(context), cancellationToken);
            }
            catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var localPath = GetLocalPath(request.Url!.AbsolutePath);

            switch (request.HttpMethod.ToUpperInvariant())
            {
                case "OPTIONS":
                    HandleOptions(response);
                    break;
                case "PROPFIND":
                    await HandlePropfindAsync(request, response, localPath);
                    break;
                case "PROPPATCH":
                    await HandleProppatchAsync(request, response, localPath);
                    break;
                case "MKCOL":
                    HandleMkcol(response, localPath);
                    break;
                case "GET":
                    await HandleGetAsync(response, localPath);
                    break;
                case "PUT":
                    await HandlePutAsync(request, response, localPath);
                    break;
                case "DELETE":
                    HandleDelete(response, localPath);
                    break;
                case "COPY":
                    HandleCopy(request, response, localPath);
                    break;
                case "MOVE":
                    HandleMove(request, response, localPath);
                    break;
                case "LOCK":
                    await HandleLockAsync(request, response, localPath);
                    break;
                case "UNLOCK":
                    HandleUnlock(response);
                    break;
                default:
                    response.StatusCode = 405;
                    break;
            }
        }
        catch (Exception)
        {
            response.StatusCode = 500;
        }
        finally
        {
            response.Close();
        }
    }

    private string GetLocalPath(string urlPath)
    {
        var decodedPath = Uri.UnescapeDataString(urlPath).TrimStart('/');
        return Path.Combine(_rootPath, decodedPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static void HandleOptions(HttpListenerResponse response)
    {
        response.Headers.Add("Allow", "OPTIONS, GET, PUT, DELETE, PROPFIND, PROPPATCH, MKCOL, COPY, MOVE, LOCK, UNLOCK");
        response.Headers.Add("DAV", "1, 2");
        response.StatusCode = 200;
    }

    private async Task HandlePropfindAsync(HttpListenerRequest request, HttpListenerResponse response, string localPath)
    {
        var exists = File.Exists(localPath) || Directory.Exists(localPath);
        if (!exists && localPath != _rootPath)
        {
            // 检查是否是根路径
            if (request.Url!.AbsolutePath == "/" || string.IsNullOrEmpty(request.Url.AbsolutePath))
            {
                localPath = _rootPath;
            }
            else
            {
                response.StatusCode = 404;
                return;
            }
        }

        var depth = request.Headers["Depth"] ?? "1";

        var multistatus = new XElement(DavNs + "multistatus");

        if (Directory.Exists(localPath) || localPath == _rootPath)
        {
            AddResourceResponse(multistatus, request.Url!, localPath, true);

            if (depth != "0")
            {
                foreach (var dir in Directory.GetDirectories(localPath))
                {
                    var name = Path.GetFileName(dir);
                    var childUrl = new Uri(request.Url!, name + "/");
                    AddResourceResponse(multistatus, childUrl, dir, true);
                }

                foreach (var file in Directory.GetFiles(localPath))
                {
                    var name = Path.GetFileName(file);
                    var childUrl = new Uri(request.Url!, name);
                    AddResourceResponse(multistatus, childUrl, file, false);
                }
            }
        }
        else
        {
            AddResourceResponse(multistatus, request.Url!, localPath, false);
        }

        var doc = new XDocument(multistatus);
        var xml = doc.ToString();
        var bytes = Encoding.UTF8.GetBytes(xml);

        response.StatusCode = 207;
        response.ContentType = "application/xml; charset=utf-8";
        await response.OutputStream.WriteAsync(bytes);
    }

    private static void AddResourceResponse(XElement multistatus, Uri url, string localPath, bool isCollection)
    {
        var propstat = new XElement(DavNs + "propstat");
        var prop = new XElement(DavNs + "prop");

        prop.Add(new XElement(DavNs + "displayname", Path.GetFileName(localPath)));

        if (isCollection)
        {
            prop.Add(new XElement(DavNs + "resourcetype", new XElement(DavNs + "collection")));
        }
        else
        {
            prop.Add(new XElement(DavNs + "resourcetype"));
            if (File.Exists(localPath))
            {
                var fileInfo = new FileInfo(localPath);
                prop.Add(new XElement(DavNs + "getcontentlength", fileInfo.Length));
                prop.Add(new XElement(DavNs + "getlastmodified", fileInfo.LastWriteTimeUtc.ToString("R")));
            }
        }

        propstat.Add(prop);
        propstat.Add(new XElement(DavNs + "status", "HTTP/1.1 200 OK"));

        var responseElement = new XElement(
            DavNs + "response",
            new XElement(DavNs + "href", url.AbsolutePath),
            propstat);

        multistatus.Add(responseElement);
    }

    private static async Task HandleProppatchAsync(HttpListenerRequest request, HttpListenerResponse response, string localPath)
    {
        if (!File.Exists(localPath) && !Directory.Exists(localPath))
        {
            response.StatusCode = 404;
            return;
        }

        // 简单实现：返回成功但不实际修改属性
        var multistatus = new XElement(
            DavNs + "multistatus",
            new XElement(
                DavNs + "response",
                new XElement(DavNs + "href", request.Url!.AbsolutePath),
                new XElement(
                    DavNs + "propstat",
                    new XElement(DavNs + "prop", new XElement(DavNs + "displayname")),
                    new XElement(DavNs + "status", "HTTP/1.1 200 OK"))));

        var doc = new XDocument(multistatus);
        var xml = doc.ToString();
        var bytes = Encoding.UTF8.GetBytes(xml);

        response.StatusCode = 207;
        response.ContentType = "application/xml; charset=utf-8";
        await response.OutputStream.WriteAsync(bytes);
    }

    private static void HandleMkcol(HttpListenerResponse response, string localPath)
    {
        if (Directory.Exists(localPath))
        {
            response.StatusCode = 405; // Method Not Allowed
            return;
        }

        try
        {
            Directory.CreateDirectory(localPath);
            response.StatusCode = 201;
        }
        catch
        {
            response.StatusCode = 500;
        }
    }

    private static async Task HandleGetAsync(HttpListenerResponse response, string localPath)
    {
        if (!File.Exists(localPath))
        {
            response.StatusCode = 404;
            return;
        }

        var bytes = await File.ReadAllBytesAsync(localPath);
        response.ContentType = "application/octet-stream";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
        response.StatusCode = 200;
    }

    private static async Task HandlePutAsync(HttpListenerRequest request, HttpListenerResponse response, string localPath)
    {
        try
        {
            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fs = File.Create(localPath);
            await request.InputStream.CopyToAsync(fs);

            response.StatusCode = 201;
        }
        catch
        {
            response.StatusCode = 500;
        }
    }

    private static void HandleDelete(HttpListenerResponse response, string localPath)
    {
        try
        {
            if (File.Exists(localPath))
            {
                File.Delete(localPath);
                response.StatusCode = 204;
            }
            else if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, recursive: true);
                response.StatusCode = 204;
            }
            else
            {
                response.StatusCode = 404;
            }
        }
        catch
        {
            response.StatusCode = 500;
        }
    }

    private void HandleCopy(HttpListenerRequest request, HttpListenerResponse response, string sourcePath)
    {
        var destination = request.Headers["Destination"];
        if (string.IsNullOrEmpty(destination))
        {
            response.StatusCode = 400;
            return;
        }

        try
        {
            var destUri = new Uri(destination);
            var destPath = GetLocalPath(destUri.AbsolutePath);

            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destPath, overwrite: true);
                response.StatusCode = 201;
            }
            else if (Directory.Exists(sourcePath))
            {
                CopyDirectory(sourcePath, destPath);
                response.StatusCode = 201;
            }
            else
            {
                response.StatusCode = 404;
            }
        }
        catch
        {
            response.StatusCode = 500;
        }
    }

    private void HandleMove(HttpListenerRequest request, HttpListenerResponse response, string sourcePath)
    {
        var destination = request.Headers["Destination"];
        if (string.IsNullOrEmpty(destination))
        {
            response.StatusCode = 400;
            return;
        }

        try
        {
            var destUri = new Uri(destination);
            var destPath = GetLocalPath(destUri.AbsolutePath);

            if (File.Exists(sourcePath))
            {
                File.Move(sourcePath, destPath, overwrite: true);
                response.StatusCode = 201;
            }
            else if (Directory.Exists(sourcePath))
            {
                Directory.Move(sourcePath, destPath);
                response.StatusCode = 201;
            }
            else
            {
                response.StatusCode = 404;
            }
        }
        catch
        {
            response.StatusCode = 500;
        }
    }

    private static async Task HandleLockAsync(HttpListenerRequest request, HttpListenerResponse response, string localPath)
    {
        if (!File.Exists(localPath) && !Directory.Exists(localPath))
        {
            response.StatusCode = 404;
            return;
        }

        // 生成锁定令牌
        var lockToken = $"opaquelocktoken:{Guid.NewGuid()}";

        var lockDiscovery = new XElement(
            DavNs + "prop",
            new XElement(
                DavNs + "lockdiscovery",
                new XElement(
                    DavNs + "activelock",
                    new XElement(DavNs + "locktype", new XElement(DavNs + "write")),
                    new XElement(DavNs + "lockscope", new XElement(DavNs + "exclusive")),
                    new XElement(DavNs + "depth", "0"),
                    new XElement(DavNs + "timeout", "Second-3600"),
                    new XElement(DavNs + "locktoken", new XElement(DavNs + "href", lockToken)))));

        var doc = new XDocument(lockDiscovery);
        var xml = doc.ToString();
        var bytes = Encoding.UTF8.GetBytes(xml);

        response.Headers.Add("Lock-Token", $"<{lockToken}>");
        response.StatusCode = 200;
        response.ContentType = "application/xml; charset=utf-8";
        await response.OutputStream.WriteAsync(bytes);
    }

    private static void HandleUnlock(HttpListenerResponse response)
    {
        response.StatusCode = 204;
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }
}
