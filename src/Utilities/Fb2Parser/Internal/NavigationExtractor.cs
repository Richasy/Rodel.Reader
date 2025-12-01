// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Utilities.Fb2Parser;

/// <summary>
/// 从章节中提取导航结构。
/// </summary>
internal static class NavigationExtractor
{
    /// <summary>
    /// 从章节列表提取导航项。
    /// </summary>
    /// <param name="sections">章节列表。</param>
    /// <returns>导航项列表。</returns>
    public static List<Fb2NavItem> Extract(IReadOnlyList<Fb2Section> sections)
    {
        var navItems = new List<Fb2NavItem>();

        foreach (var section in sections)
        {
            var navItem = CreateNavItem(section);
            if (navItem != null)
            {
                navItems.Add(navItem);
            }
        }

        return navItems;
    }

    private static Fb2NavItem? CreateNavItem(Fb2Section section)
    {
        // 如果章节没有标题且没有子章节，跳过
        if (string.IsNullOrEmpty(section.Title) && !section.HasChildren)
        {
            return null;
        }

        var navItem = new Fb2NavItem
        {
            Title = section.Title ?? $"Section {section.Id}",
            SectionId = section.Id,
            Level = section.Level,
        };

        // 递归处理子章节
        foreach (var child in section.Children)
        {
            var childNav = CreateNavItem(child);
            if (childNav != null)
            {
                navItem.Children.Add(childNav);
            }
        }

        return navItem;
    }
}
