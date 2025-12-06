// Copyright (c) Richasy. All rights reserved.

using System.Text.Json;
using Richasy.RodelReader.Sources.Rss.Inoreader.Internal;

namespace Richasy.RodelReader.Sources.Rss.Inoreader;

/// <summary>
/// Inoreader OAuth 认证辅助类.
/// </summary>
public static class InoreaderAuthHelper
{
    /// <summary>
    /// 获取 OAuth 授权 URL.
    /// </summary>
    /// <param name="options">客户端配置.</param>
    /// <param name="state">状态参数（用于防止 CSRF 攻击）.</param>
    /// <returns>授权 URL.</returns>
    public static Uri GetAuthorizationUrl(InoreaderClientOptions options, string? state = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        state ??= Guid.NewGuid().ToString("N");
        var baseUrl = options.GetBaseUrl();
        var clientId = Uri.EscapeDataString(options.ClientId);
        var redirectUri = Uri.EscapeDataString(options.RedirectUri);

        return new Uri(baseUrl, $"/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=read%20write&state={state}");
    }

    /// <summary>
    /// 使用授权码交换访问令牌.
    /// </summary>
    /// <param name="code">授权码.</param>
    /// <param name="options">客户端配置.</param>
    /// <param name="httpClient">HTTP 客户端（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>Token 信息.</returns>
    public static async Task<TokenUpdateEventArgs> ExchangeCodeForTokenAsync(
        string code,
        InoreaderClientOptions options,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(options);

        var shouldDisposeClient = httpClient == null;
        httpClient ??= HttpClientHelper.CreateHttpClient(options.Timeout);

        try
        {
            var url = new Uri(options.GetBaseUrl(), "/oauth2/token");
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = options.ClientId,
                ["client_secret"] = options.ClientSecret,
                ["redirect_uri"] = options.RedirectUri,
                ["scope"] = "read write",
                ["grant_type"] = "authorization_code",
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content,
            };

            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var authResult = JsonSerializer.Deserialize(json, InoreaderJsonContext.Default.InoreaderAuthResponse)
                ?? throw new InvalidOperationException("Failed to parse auth response.");

            var expireTime = DateTimeOffset.Now.AddSeconds(authResult.ExpiresIn);

            return new TokenUpdateEventArgs
            {
                AccessToken = authResult.AccessToken,
                RefreshToken = authResult.RefreshToken,
                ExpireTime = expireTime,
            };
        }
        finally
        {
            if (shouldDisposeClient)
            {
                httpClient.Dispose();
            }
        }
    }

    /// <summary>
    /// 刷新访问令牌.
    /// </summary>
    /// <param name="refreshToken">刷新令牌.</param>
    /// <param name="options">客户端配置.</param>
    /// <param name="httpClient">HTTP 客户端（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>新的 Token 信息.</returns>
    public static async Task<TokenUpdateEventArgs> RefreshTokenAsync(
        string refreshToken,
        InoreaderClientOptions options,
        HttpClient? httpClient = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        ArgumentNullException.ThrowIfNull(options);

        var shouldDisposeClient = httpClient == null;
        httpClient ??= HttpClientHelper.CreateHttpClient(options.Timeout);

        try
        {
            var url = new Uri(options.GetBaseUrl(), "/oauth2/token");
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["refresh_token"] = refreshToken,
                ["client_id"] = options.ClientId,
                ["client_secret"] = options.ClientSecret,
                ["grant_type"] = "refresh_token",
            });

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content,
            };

            var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var authResult = JsonSerializer.Deserialize(json, InoreaderJsonContext.Default.InoreaderAuthResponse)
                ?? throw new InvalidOperationException("Failed to parse auth response.");

            var expireTime = DateTimeOffset.Now.AddSeconds(authResult.ExpiresIn);

            return new TokenUpdateEventArgs
            {
                AccessToken = authResult.AccessToken,
                RefreshToken = authResult.RefreshToken,
                ExpireTime = expireTime,
            };
        }
        finally
        {
            if (shouldDisposeClient)
            {
                httpClient.Dispose();
            }
        }
    }
}
