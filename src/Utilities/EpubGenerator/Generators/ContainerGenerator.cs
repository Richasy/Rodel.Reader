// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelPlayer.Utilities.EpubGenerator;

/// <summary>
/// Container.xml 生成器实现.
/// </summary>
internal sealed class ContainerGenerator : IContainerGenerator
{
    /// <inheritdoc/>
    public string Generate() => EpubTemplates.Container;
}
