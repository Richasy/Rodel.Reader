// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser;

/// <summary>
/// Feed 人员（作者/贡献者）.
/// </summary>
/// <param name="Name">人员名称.</param>
/// <param name="PersonType">人员类型.</param>
/// <param name="Email">电子邮件地址（可选）.</param>
/// <param name="Uri">人员主页（可选）.</param>
public sealed record FeedPerson(
    string Name,
    FeedPersonType PersonType = FeedPersonType.Author,
    string? Email = null,
    string? Uri = null);
