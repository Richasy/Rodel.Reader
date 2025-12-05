// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.Rss.Database;

/// <summary>
/// 分组数据仓库.
/// </summary>
internal sealed class GroupRepository
{
    private readonly RssDatabase _database;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupRepository"/> class.
    /// </summary>
    public GroupRepository(RssDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有分组.
    /// </summary>
    public async Task<IReadOnlyList<RssFeedGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name FROM Groups ORDER BY Name";

        await using var cmd = _database.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var groups = new List<RssFeedGroup>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            groups.Add(MapToGroup(reader));
        }

        _logger?.LogDebug("Retrieved {Count} groups.", groups.Count);
        return groups;
    }

    /// <summary>
    /// 根据 ID 获取分组.
    /// </summary>
    public async Task<RssFeedGroup?> GetByIdAsync(string groupId, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT Id, Name FROM Groups WHERE Id = @id";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", groupId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return MapToGroup(reader);
        }

        return null;
    }

    /// <summary>
    /// 添加或更新分组.
    /// </summary>
    public async Task UpsertAsync(RssFeedGroup group, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO Groups (Id, Name)
            VALUES (@id, @name)
            ON CONFLICT(Id) DO UPDATE SET Name = excluded.Name
            """;

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", group.Id);
        cmd.Parameters.AddWithValue("@name", group.Name);

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted group: {GroupId}", group.Id);
    }

    /// <summary>
    /// 批量添加或更新分组.
    /// </summary>
    public async Task UpsertManyAsync(IEnumerable<RssFeedGroup> groups, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            foreach (var group in groups)
            {
                const string sql = """
                    INSERT INTO Groups (Id, Name)
                    VALUES (@id, @name)
                    ON CONFLICT(Id) DO UPDATE SET Name = excluded.Name
                    """;

                await using var cmd = _database.CreateCommand(sql);
                cmd.Transaction = transaction;
                cmd.Parameters.AddWithValue("@id", group.Id);
                cmd.Parameters.AddWithValue("@name", group.Name);

                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Upserted multiple groups in batch.");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 删除分组.
    /// </summary>
    public async Task<bool> DeleteAsync(string groupId, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM Groups WHERE Id = @id";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@id", groupId);

        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Deleted group: {GroupId}, affected: {Affected}", groupId, affected);

        return affected > 0;
    }

    private static RssFeedGroup MapToGroup(SqliteDataReader reader)
    {
        return new RssFeedGroup
        {
            Id = reader.GetString(0),
            Name = reader.GetString(1),
        };
    }
}
