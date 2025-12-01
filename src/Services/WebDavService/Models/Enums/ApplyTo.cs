// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// 指定 WebDAV 操作应用的范围（用于 PROPFIND）.
/// </summary>
public static class ApplyTo
{
    /// <summary>
    /// PROPFIND 操作的应用范围.
    /// </summary>
    public static class Propfind
    {
        /// <summary>
        /// 仅应用于资源本身.
        /// </summary>
        public static readonly PropfindApplyTo ResourceOnly = new(0);

        /// <summary>
        /// 应用于资源及其直接子资源.
        /// </summary>
        public static readonly PropfindApplyTo ResourceAndChildren = new(1);

        /// <summary>
        /// 应用于资源及其所有后代资源.
        /// </summary>
        public static readonly PropfindApplyTo ResourceAndAllDescendants = new(int.MaxValue);
    }

    /// <summary>
    /// COPY 操作的应用范围.
    /// </summary>
    public static class Copy
    {
        /// <summary>
        /// 仅复制资源本身.
        /// </summary>
        public static readonly CopyApplyTo ResourceOnly = new(0);

        /// <summary>
        /// 复制资源及其所有后代.
        /// </summary>
        public static readonly CopyApplyTo ResourceAndAncestors = new(int.MaxValue);
    }

    /// <summary>
    /// LOCK 操作的应用范围.
    /// </summary>
    public static class Lock
    {
        /// <summary>
        /// 仅锁定资源本身.
        /// </summary>
        public static readonly LockApplyTo ResourceOnly = new(0);

        /// <summary>
        /// 锁定资源及其所有后代.
        /// </summary>
        public static readonly LockApplyTo ResourceAndAncestors = new(int.MaxValue);
    }
}

/// <summary>
/// PROPFIND 应用范围值.
/// </summary>
/// <param name="Depth">深度值.</param>
public readonly record struct PropfindApplyTo(int Depth);

/// <summary>
/// COPY 应用范围值.
/// </summary>
/// <param name="Depth">深度值.</param>
public readonly record struct CopyApplyTo(int Depth);

/// <summary>
/// LOCK 应用范围值.
/// </summary>
/// <param name="Depth">深度值.</param>
public readonly record struct LockApplyTo(int Depth);
