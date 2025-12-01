// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 常量定义.
/// </summary>
internal static class WebDavConstants
{
    /// <summary>
    /// DAV 命名空间.
    /// </summary>
    public const string DavNamespace = "DAV:";

    /// <summary>
    /// WebDAV 请求头.
    /// </summary>
    public static class Headers
    {
        /// <summary>
        /// Depth 请求头.
        /// </summary>
        public const string Depth = "Depth";

        /// <summary>
        /// Destination 请求头.
        /// </summary>
        public const string Destination = "Destination";

        /// <summary>
        /// Overwrite 请求头.
        /// </summary>
        public const string Overwrite = "Overwrite";

        /// <summary>
        /// If 请求头.
        /// </summary>
        public const string If = "If";

        /// <summary>
        /// Lock-Token 请求头.
        /// </summary>
        public const string LockToken = "Lock-Token";

        /// <summary>
        /// Timeout 请求头.
        /// </summary>
        public const string Timeout = "Timeout";

        /// <summary>
        /// Translate 请求头.
        /// </summary>
        public const string Translate = "Translate";
    }

    /// <summary>
    /// 媒体类型.
    /// </summary>
    public static class MediaTypes
    {
        /// <summary>
        /// XML 媒体类型.
        /// </summary>
        public const string Xml = "text/xml";

        /// <summary>
        /// 应用 XML 媒体类型.
        /// </summary>
        public const string ApplicationXml = "application/xml";
    }
}
