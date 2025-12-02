// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ApplePodcast.Models;

/// <summary>
/// 播客分类.
/// </summary>
/// <remarks>
/// <para>每个分类包含 ID 和默认英文名称。</para>
/// <para>使用 <see cref="GetAllCategories"/> 获取所有预定义分类。</para>
/// <para>可通过 <see cref="GetName"/> 方法配合自定义本地化函数获取本地化名称。</para>
/// </remarks>
public sealed class PodcastCategory : IEquatable<PodcastCategory>
{
    private static readonly Dictionary<string, PodcastCategory> _categoriesById = [];

    /// <summary>
    /// 分类 ID.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 默认名称（英文）.
    /// </summary>
    public string DefaultName { get; }

    private PodcastCategory(string id, string defaultName)
    {
        Id = id;
        DefaultName = defaultName;
        _categoriesById[id] = this;
    }

    /// <summary>
    /// 所有分类.
    /// </summary>
    public static PodcastCategory All { get; } = new("0", "All");

    /// <summary>
    /// 艺术 (Arts).
    /// </summary>
    public static PodcastCategory Arts { get; } = new("1301", "Arts");

    /// <summary>
    /// 喜剧 (Comedy).
    /// </summary>
    public static PodcastCategory Comedy { get; } = new("1303", "Comedy");

    /// <summary>
    /// 教育 (Education).
    /// </summary>
    public static PodcastCategory Education { get; } = new("1304", "Education");

    /// <summary>
    /// 儿童与家庭 (Kids &amp; Family).
    /// </summary>
    public static PodcastCategory KidsAndFamily { get; } = new("1305", "Kids & Family");

    /// <summary>
    /// 电视与电影 (TV &amp; Film).
    /// </summary>
    public static PodcastCategory TvAndFilm { get; } = new("1309", "TV & Film");

    /// <summary>
    /// 音乐 (Music).
    /// </summary>
    public static PodcastCategory Music { get; } = new("1310", "Music");

    /// <summary>
    /// 宗教与灵性 (Religion &amp; Spirituality).
    /// </summary>
    public static PodcastCategory ReligionAndSpirituality { get; } = new("1314", "Religion & Spirituality");

    /// <summary>
    /// 科技 (Technology).
    /// </summary>
    public static PodcastCategory Technology { get; } = new("1318", "Technology");

    /// <summary>
    /// 商业 (Business).
    /// </summary>
    public static PodcastCategory Business { get; } = new("1321", "Business");

    /// <summary>
    /// 社会与文化 (Society &amp; Culture).
    /// </summary>
    public static PodcastCategory SocietyAndCulture { get; } = new("1324", "Society & Culture");

    /// <summary>
    /// 历史 (History).
    /// </summary>
    public static PodcastCategory History { get; } = new("1487", "History");

    /// <summary>
    /// 真实犯罪 (True Crime).
    /// </summary>
    public static PodcastCategory TrueCrime { get; } = new("1488", "True Crime");

    /// <summary>
    /// 新闻 (News).
    /// </summary>
    public static PodcastCategory News { get; } = new("1489", "News");

    /// <summary>
    /// 休闲 (Leisure).
    /// </summary>
    public static PodcastCategory Leisure { get; } = new("1502", "Leisure");

    /// <summary>
    /// 健康与健身 (Health &amp; Fitness).
    /// </summary>
    public static PodcastCategory HealthAndFitness { get; } = new("1512", "Health & Fitness");

    /// <summary>
    /// 政府 (Government).
    /// </summary>
    public static PodcastCategory Government { get; } = new("1527", "Government");

    /// <summary>
    /// 科学 (Science).
    /// </summary>
    public static PodcastCategory Science { get; } = new("1533", "Science");

    /// <summary>
    /// 体育 (Sports).
    /// </summary>
    public static PodcastCategory Sports { get; } = new("1545", "Sports");

    /// <summary>
    /// 虚构 (Fiction).
    /// </summary>
    public static PodcastCategory Fiction { get; } = new("1483", "Fiction");

    /// <summary>
    /// 获取所有预定义分类.
    /// </summary>
    /// <returns>分类列表（按 ID 排序）.</returns>
    public static IReadOnlyList<PodcastCategory> GetAllCategories()
    {
        return
        [
            All,
            Arts,
            Comedy,
            Education,
            KidsAndFamily,
            TvAndFilm,
            Music,
            ReligionAndSpirituality,
            Technology,
            Business,
            SocietyAndCulture,
            Fiction,
            History,
            TrueCrime,
            News,
            Leisure,
            HealthAndFitness,
            Government,
            Science,
            Sports,
        ];
    }

    /// <summary>
    /// 根据 ID 获取预定义分类.
    /// </summary>
    /// <param name="id">分类 ID.</param>
    /// <returns>如果找到则返回对应分类，否则返回 null.</returns>
    public static PodcastCategory? GetById(string id)
        => _categoriesById.TryGetValue(id, out var category) ? category : null;

    /// <summary>
    /// 根据 ID 获取或创建分类.
    /// </summary>
    /// <param name="id">分类 ID.</param>
    /// <param name="defaultName">如果是新分类，使用的默认名称.</param>
    /// <returns>分类实例.</returns>
    public static PodcastCategory GetOrCreate(string id, string? defaultName = null)
        => GetById(id) ?? new PodcastCategory(id, defaultName ?? $"Category {id}");

    /// <summary>
    /// 获取分类名称.
    /// </summary>
    /// <param name="localizer">可选的本地化函数，接收分类 ID，返回本地化名称.</param>
    /// <returns>本地化名称（如果提供了 localizer 且返回非空值），否则返回默认英文名称.</returns>
    public string GetName(Func<string, string?>? localizer = null)
    {
        if (localizer is not null)
        {
            var localized = localizer(Id);
            if (!string.IsNullOrEmpty(localized))
            {
                return localized;
            }
        }

        return DefaultName;
    }

    /// <inheritdoc/>
    public bool Equals(PodcastCategory? other)
        => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is PodcastCategory other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => Id.GetHashCode(StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string ToString()
        => $"{DefaultName} ({Id})";

    /// <summary>
    /// 相等运算符.
    /// </summary>
    public static bool operator ==(PodcastCategory? left, PodcastCategory? right)
        => left is null ? right is null : left.Equals(right);

    /// <summary>
    /// 不等运算符.
    /// </summary>
    public static bool operator !=(PodcastCategory? left, PodcastCategory? right)
        => !(left == right);
}
