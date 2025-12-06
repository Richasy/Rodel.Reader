// Copyright (c) Richasy. All rights reserved.

using Richasy.RodelReader.Storage.ServiceRegistry.Database;

namespace Richasy.RodelReader.Storage.ServiceRegistry;

/// <summary>
/// 服务注册表实现.
/// </summary>
public sealed class ServiceRegistry : IServiceRegistry
{
    private readonly ServiceRegistryOptions _options;
    private readonly ILogger<ServiceRegistry>? _logger;

    private RegistryDatabase? _database;
    private ServiceRepository? _serviceRepository;
    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRegistry"/> class.
    /// </summary>
    public ServiceRegistry(ServiceRegistryOptions options, ILogger<ServiceRegistry>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_initialized)
        {
            _logger?.LogDebug("ServiceRegistry already initialized.");
            return;
        }

        _logger?.LogInformation("Initializing ServiceRegistry at {LibraryPath}...", _options.LibraryPath);

        // 确保库目录存在
        if (!Directory.Exists(_options.LibraryPath))
        {
            Directory.CreateDirectory(_options.LibraryPath);
        }

        var databasePath = _options.GetDatabasePath();
        _database = new RegistryDatabase(databasePath, _logger as ILogger<RegistryDatabase>);

        if (_options.CreateTablesOnInit)
        {
            await _database.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        _serviceRepository = new ServiceRepository(_database, _logger);

        _initialized = true;
        _logger?.LogInformation("ServiceRegistry initialized successfully.");
    }

    #region 服务实例管理

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ServiceInstance>> GetAllServicesAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _serviceRepository!.GetAllAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ServiceInstance>> GetServicesByTypeAsync(ServiceType type, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _serviceRepository!.GetByTypeAsync(type, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ServiceInstance?> GetServiceAsync(string serviceId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _serviceRepository!.GetByIdAsync(serviceId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ServiceInstance> CreateServiceAsync(
        string name,
        ServiceType type,
        string? icon = null,
        string? color = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 检查名称是否已存在
        if (await _serviceRepository!.IsNameExistsAsync(name, null, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"Service with name '{name}' already exists.");
        }

        var now = DateTimeOffset.UtcNow;
        var nextOrder = await _serviceRepository.GetNextSortOrderAsync(cancellationToken).ConfigureAwait(false);

        var service = new ServiceInstance
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Type = type,
            Icon = icon,
            Color = color,
            Description = description,
            CreatedAt = now,
            LastAccessedAt = now,
            SortOrder = nextOrder,
            IsActive = false,
        };

        await _serviceRepository.UpsertAsync(service, cancellationToken).ConfigureAwait(false);

        // 创建服务数据目录
        EnsureServiceDataPath(service.Id);

        _logger?.LogInformation("Created service: {ServiceId} ({ServiceName}, {ServiceType})", service.Id, service.Name, service.Type);

        return service;
    }

    /// <inheritdoc/>
    public async Task UpdateServiceAsync(ServiceInstance service, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(service.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(service.Name);

        // 检查服务是否存在
        var existing = await _serviceRepository!.GetByIdAsync(service.Id, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            throw new InvalidOperationException($"Service with ID '{service.Id}' not found.");
        }

        // 检查名称是否与其他服务冲突
        if (await _serviceRepository.IsNameExistsAsync(service.Name, service.Id, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"Service with name '{service.Name}' already exists.");
        }

        await _serviceRepository.UpsertAsync(service, cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("Updated service: {ServiceId} ({ServiceName})", service.Id, service.Name);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteServiceAsync(string serviceId, bool deleteData = false, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        ArgumentException.ThrowIfNullOrWhiteSpace(serviceId);

        var service = await _serviceRepository!.GetByIdAsync(serviceId, cancellationToken).ConfigureAwait(false);
        if (service is null)
        {
            _logger?.LogWarning("Attempted to delete non-existent service: {ServiceId}", serviceId);
            return false;
        }

        var deleted = await _serviceRepository.DeleteAsync(serviceId, cancellationToken).ConfigureAwait(false);

        if (deleted && deleteData)
        {
            var dataPath = GetServiceDataPath(serviceId);
            if (Directory.Exists(dataPath))
            {
                try
                {
                    Directory.Delete(dataPath, recursive: true);
                    _logger?.LogInformation("Deleted service data directory: {DataPath}", dataPath);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to delete service data directory: {DataPath}", dataPath);
                }
            }
        }

        if (deleted)
        {
            _logger?.LogInformation("Deleted service: {ServiceId} ({ServiceName})", serviceId, service.Name);
        }

        return deleted;
    }

    /// <inheritdoc/>
    public async Task<bool> IsServiceNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _serviceRepository!.IsNameExistsAsync(name, excludeId, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region 活动服务管理

    /// <inheritdoc/>
    public async Task<ServiceInstance?> GetActiveServiceAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        return await _serviceRepository!.GetActiveAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task SetActiveServiceAsync(string serviceId, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        ArgumentException.ThrowIfNullOrWhiteSpace(serviceId);

        // 检查服务是否存在
        var service = await _serviceRepository!.GetByIdAsync(serviceId, cancellationToken).ConfigureAwait(false);
        if (service is null)
        {
            throw new InvalidOperationException($"Service with ID '{serviceId}' not found.");
        }

        await _serviceRepository.SetActiveAsync(serviceId, cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("Set active service: {ServiceId} ({ServiceName})", serviceId, service.Name);
    }

    /// <inheritdoc/>
    public async Task ClearActiveServiceAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        await _serviceRepository!.ClearActiveAsync(cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region 排序管理

    /// <inheritdoc/>
    public async Task UpdateServiceOrderAsync(IEnumerable<string> orderedIds, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        ArgumentNullException.ThrowIfNull(orderedIds);

        await _serviceRepository!.UpdateOrderAsync(orderedIds, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region 数据路径

    /// <inheritdoc/>
    public string GetServiceDataPath(string serviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceId);
        return Path.Combine(_options.LibraryPath, serviceId);
    }

    /// <inheritdoc/>
    public string EnsureServiceDataPath(string serviceId)
    {
        var path = GetServiceDataPath(serviceId);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger?.LogDebug("Created service data directory: {DataPath}", path);
        }

        return path;
    }

    #endregion

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _database?.Dispose();
        _database = null;
        _serviceRepository = null;
        _initialized = false;
        _disposed = true;

        _logger?.LogDebug("ServiceRegistry disposed.");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_database is not null)
        {
            await _database.DisposeAsync().ConfigureAwait(false);
            _database = null;
        }

        _serviceRepository = null;
        _initialized = false;
        _disposed = true;

        _logger?.LogDebug("ServiceRegistry disposed asynchronously.");
    }

    #endregion

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_initialized)
        {
            throw new InvalidOperationException("ServiceRegistry is not initialized. Call InitializeAsync first.");
        }
    }
}
