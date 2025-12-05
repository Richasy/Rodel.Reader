// Copyright (c) Reader Copilot. All rights reserved.

using System.Runtime.CompilerServices;
using System.Text;

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 高性能字符串构建辅助类.
/// </summary>
internal static class StringBuilderPool
{
    private static readonly DefaultObjectPool<StringBuilder> Pool = new(
        new StringBuilderPooledObjectPolicy { InitialCapacity = 1024, MaximumRetainedCapacity = 64 * 1024 });

    /// <summary>
    /// 从池中获取 StringBuilder.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder Rent() => Pool.Rent();

    /// <summary>
    /// 将 StringBuilder 归还到池中.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(StringBuilder builder)
    {
        builder.Clear();
        Pool.Return(builder);
    }

    /// <summary>
    /// 获取字符串并归还 StringBuilder.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToStringAndReturn(StringBuilder builder)
    {
        var result = builder.ToString();
        Return(builder);
        return result;
    }
}

/// <summary>
/// StringBuilder 池化策略.
/// </summary>
internal sealed class StringBuilderPooledObjectPolicy : IPooledObjectPolicy<StringBuilder>
{
    public int InitialCapacity { get; set; } = 256;
    public int MaximumRetainedCapacity { get; set; } = 32 * 1024;

    public StringBuilder Create() => new(InitialCapacity);

    public bool TryReturn(StringBuilder obj)
    {
        if (obj.Capacity > MaximumRetainedCapacity)
        {
            return false;
        }

        obj.Clear();
        return true;
    }
}

/// <summary>
/// 池化对象策略接口.
/// </summary>
/// <typeparam name="T">对象类型.</typeparam>
internal interface IPooledObjectPolicy<T> where T : class
{
    /// <summary>
    /// 创建新对象.
    /// </summary>
    T Create();

    /// <summary>
    /// 尝试归还对象到池中.
    /// </summary>
    bool TryReturn(T obj);
}

/// <summary>
/// 默认对象池实现.
/// </summary>
internal sealed class DefaultObjectPool<T> where T : class
{
    private readonly IPooledObjectPolicy<T> _policy;
    private readonly T?[] _items;
    private T? _firstItem;

    public DefaultObjectPool(IPooledObjectPolicy<T> policy, int maximumRetained = 16)
    {
        _policy = policy;
        _items = new T[maximumRetained - 1];
    }

    public T Rent()
    {
        var item = _firstItem;
        if (item == null || Interlocked.CompareExchange(ref _firstItem, null, item) != item)
        {
            item = RentViaScan();
        }

        return item;
    }

    public void Return(T obj)
    {
        if (!_policy.TryReturn(obj))
        {
            return;
        }

        if (_firstItem == null && Interlocked.CompareExchange(ref _firstItem, obj, null) == null)
        {
            return;
        }

        ReturnViaScan(obj);
    }

    private T RentViaScan()
    {
        var items = _items;
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            if (item != null && Interlocked.CompareExchange(ref items[i], null, item) == item)
            {
                return item;
            }
        }

        return _policy.Create();
    }

    private void ReturnViaScan(T obj)
    {
        var items = _items;
        for (var i = 0; i < items.Length; i++)
        {
            if (items[i] == null && Interlocked.CompareExchange(ref items[i], obj, null) == null)
            {
                return;
            }
        }
    }
}
