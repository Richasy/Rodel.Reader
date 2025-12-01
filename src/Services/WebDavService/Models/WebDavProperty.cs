// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.WebDav;

/// <summary>
/// WebDAV 属性.
/// </summary>
public sealed class WebDavProperty
{
    /// <summary>
    /// 初始化 <see cref="WebDavProperty"/> 类的新实例.
    /// </summary>
    /// <param name="name">属性名称.</param>
    /// <param name="value">属性值.</param>
    public WebDavProperty(string name, string value)
        : this(name, null, value)
    {
    }

    /// <summary>
    /// 初始化 <see cref="WebDavProperty"/> 类的新实例.
    /// </summary>
    /// <param name="name">属性名称.</param>
    /// <param name="namespaceName">命名空间.</param>
    /// <param name="value">属性值.</param>
    public WebDavProperty(string name, string? namespaceName, string value)
    {
        Name = name;
        Namespace = namespaceName;
        Value = value;
    }

    /// <summary>
    /// 获取属性名称.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 获取命名空间.
    /// </summary>
    public string? Namespace { get; }

    /// <summary>
    /// 获取属性值.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{Namespace}:{Name} = {Value}";
}
