// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.ServiceRegistry.Database;

/// <summary>
/// 服务数据仓库.
/// </summary>
internal sealed class ServiceRepository
{
    private readonly RegistryDatabase _database;
    private readonly ILogger? _logger;
    private readonly ServiceEntityRepository<RegistryDatabase> _repository = new();

    public ServiceRepository(RegistryDatabase database, ILogger? logger = null)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有服务.
    /// </summary>
    public async Task<IReadOnlyList<ServiceInstance>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Services ORDER BY SortOrder ASC, CreatedAt ASC";
        await using var cmd = _database.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ServiceInstance>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ServiceEntityRepository<RegistryDatabase>.MapToEntity(reader).ToModel());
        }

        _logger?.LogDebug("Retrieved {Count} services.", results.Count);
        return results;
    }

    /// <summary>
    /// 根据类型获取服务.
    /// </summary>
    public async Task<IReadOnlyList<ServiceInstance>> GetByTypeAsync(ServiceType type, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Services WHERE Type = @type ORDER BY SortOrder ASC, CreatedAt ASC";
        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@type", (int)type);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<ServiceInstance>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(ServiceEntityRepository<RegistryDatabase>.MapToEntity(reader).ToModel());
        }

        return results;
    }

    /// <summary>
    /// 根据 ID 获取服务.
    /// </summary>
    public async Task<ServiceInstance?> GetByIdAsync(string serviceId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(_database, serviceId, cancellationToken).ConfigureAwait(false);
        return entity?.ToModel();
    }

    /// <summary>
    /// 获取活动服务.
    /// </summary>
    public async Task<ServiceInstance?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT * FROM Services WHERE IsActive = 1 LIMIT 1";
        await using var cmd = _database.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return ServiceEntityRepository<RegistryDatabase>.MapToEntity(reader).ToModel();
        }

        return null;
    }

    /// <summary>
    /// 检查名称是否存在.
    /// </summary>
    public async Task<bool> IsNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        var sql = excludeId is null
            ? "SELECT COUNT(1) FROM Services WHERE Name = @name"
            : "SELECT COUNT(1) FROM Services WHERE Name = @name AND Id != @excludeId";

        await using var cmd = _database.CreateCommand(sql);
        cmd.Parameters.AddWithValue("@name", name);
        if (excludeId is not null)
        {
            cmd.Parameters.AddWithValue("@excludeId", excludeId);
        }

        var count = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt64(count) > 0;
    }

    /// <summary>
    /// 获取下一个排序顺序.
    /// </summary>
    public async Task<int> GetNextSortOrderAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COALESCE(MAX(SortOrder), -1) + 1 FROM Services";
        await using var cmd = _database.CreateCommand(sql);
        var result = await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    /// <summary>
    /// 添加或更新服务.
    /// </summary>
    public async Task UpsertAsync(ServiceInstance service, CancellationToken cancellationToken = default)
    {
        var entity = ServiceEntity.FromModel(service);
        await _repository.UpsertAsync(_database, entity, cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Upserted service: {ServiceId} ({ServiceName})", service.Id, service.Name);
    }

    /// <summary>
    /// 设置活动服务（先清除所有活动状态，再设置指定服务为活动）.
    /// </summary>
    public async Task SetActiveAsync(string serviceId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // 清除所有活动状态
            await using (var clearCmd = _database.CreateCommand("UPDATE Services SET IsActive = 0"))
            {
                clearCmd.Transaction = transaction;
                await clearCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            // 设置指定服务为活动
            await using (var setCmd = _database.CreateCommand("UPDATE Services SET IsActive = 1, LastAccessedAt = @time WHERE Id = @id"))
            {
                setCmd.Transaction = transaction;
                setCmd.Parameters.AddWithValue("@id", serviceId);
                setCmd.Parameters.AddWithValue("@time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                await setCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Set active service: {ServiceId}", serviceId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 清除活动服务状态.
    /// </summary>
    public async Task ClearActiveAsync(CancellationToken cancellationToken = default)
    {
        await using var cmd = _database.CreateCommand("UPDATE Services SET IsActive = 0");
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger?.LogDebug("Cleared active service status.");
    }

    /// <summary>
    /// 更新排序顺序.
    /// </summary>
    public async Task UpdateOrderAsync(IEnumerable<string> orderedIds, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var order = 0;
            foreach (var id in orderedIds)
            {
                await using var cmd = _database.CreateCommand("UPDATE Services SET SortOrder = @order WHERE Id = @id");
                cmd.Transaction = transaction;
                cmd.Parameters.AddWithValue("@order", order);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                order++;
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            _logger?.LogDebug("Updated service order for {Count} services.", order);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// 删除服务.
    /// </summary>
    public async Task<bool> DeleteAsync(string serviceId, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(_database, serviceId, cancellationToken).ConfigureAwait(false);
    }
}
