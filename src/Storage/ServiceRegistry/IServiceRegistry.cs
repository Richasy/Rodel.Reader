// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Storage.ServiceRegistry;

/// <summary>
/// 服务注册表接口.
/// </summary>
public interface IServiceRegistry : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// 初始化注册表.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    #region 服务实例管理

    /// <summary>
    /// 获取所有服务实例.
    /// </summary>
    Task<IReadOnlyList<ServiceInstance>> GetAllServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据类型获取服务实例列表.
    /// </summary>
    Task<IReadOnlyList<ServiceInstance>> GetServicesByTypeAsync(ServiceType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 ID 获取服务实例.
    /// </summary>
    Task<ServiceInstance?> GetServiceAsync(string serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建新的服务实例.
    /// </summary>
    /// <param name="name">服务名称.</param>
    /// <param name="type">服务类型.</param>
    /// <param name="icon">图标标识（可选）.</param>
    /// <param name="color">主题色（可选）.</param>
    /// <param name="description">服务描述（可选）.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>创建的服务实例.</returns>
    Task<ServiceInstance> CreateServiceAsync(
        string name,
        ServiceType type,
        string? icon = null,
        string? color = null,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新服务实例.
    /// </summary>
    Task UpdateServiceAsync(ServiceInstance service, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除服务实例.
    /// </summary>
    /// <param name="serviceId">服务 ID.</param>
    /// <param name="deleteData">是否同时删除服务数据目录.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>是否删除成功.</returns>
    Task<bool> DeleteServiceAsync(string serviceId, bool deleteData = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查服务名称是否已存在.
    /// </summary>
    Task<bool> IsServiceNameExistsAsync(string name, string? excludeId = null, CancellationToken cancellationToken = default);

    #endregion

    #region 活动服务管理

    /// <summary>
    /// 获取当前活动服务.
    /// </summary>
    Task<ServiceInstance?> GetActiveServiceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置活动服务.
    /// </summary>
    Task SetActiveServiceAsync(string serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除活动服务状态.
    /// </summary>
    Task ClearActiveServiceAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 排序管理

    /// <summary>
    /// 更新服务排序顺序.
    /// </summary>
    /// <param name="orderedIds">按顺序排列的服务 ID 列表.</param>
    /// <param name="cancellationToken">取消令牌.</param>
    Task UpdateServiceOrderAsync(IEnumerable<string> orderedIds, CancellationToken cancellationToken = default);

    #endregion

    #region 数据路径

    /// <summary>
    /// 获取服务的数据存储路径.
    /// </summary>
    /// <param name="serviceId">服务 ID.</param>
    /// <returns>服务数据目录的完整路径.</returns>
    string GetServiceDataPath(string serviceId);

    /// <summary>
    /// 确保服务数据目录存在.
    /// </summary>
    /// <param name="serviceId">服务 ID.</param>
    /// <returns>服务数据目录的完整路径.</returns>
    string EnsureServiceDataPath(string serviceId);

    #endregion
}
