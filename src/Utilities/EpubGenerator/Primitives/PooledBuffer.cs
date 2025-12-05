// Copyright (c) Reader Copilot. All rights reserved.

using System.Buffers;
using System.Runtime.CompilerServices;

namespace Richasy.RodelReader.Utilities.EpubGenerator;

/// <summary>
/// 基于 ArrayPool 的临时缓冲区，用于减少内存分配.
/// </summary>
/// <typeparam name="T">缓冲区元素类型.</typeparam>
internal ref struct PooledBuffer<T>
{
    private T[]? _array;
    private readonly int _length;

    /// <summary>
    /// 创建指定长度的缓冲区.
    /// </summary>
    public PooledBuffer(int length)
    {
        _array = ArrayPool<T>.Shared.Rent(length);
        _length = length;
    }

    /// <summary>
    /// 获取缓冲区的 Span.
    /// </summary>
    public readonly Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array.AsSpan(0, _length);
    }

    /// <summary>
    /// 获取缓冲区长度.
    /// </summary>
    public readonly int Length => _length;

    /// <summary>
    /// 获取底层数组的 Span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> GetArraySpan() => _array.AsSpan(0, _length);

    /// <summary>
    /// 释放缓冲区.
    /// </summary>
    public void Dispose()
    {
        if (_array is not null)
        {
            ArrayPool<T>.Shared.Return(_array);
            _array = null;
        }
    }
}

/// <summary>
/// 值类型列表，减少装箱开销.
/// </summary>
/// <typeparam name="T">元素类型.</typeparam>
internal ref struct ValueList<T>
{
    private T[] _items;
    private int _count;

    /// <summary>
    /// 创建指定初始容量的列表.
    /// </summary>
    public ValueList(int initialCapacity = 16)
    {
        _items = ArrayPool<T>.Shared.Rent(initialCapacity);
        _count = 0;
    }

    /// <summary>
    /// 获取当前元素数量.
    /// </summary>
    public readonly int Count => _count;

    /// <summary>
    /// 获取容量.
    /// </summary>
    public readonly int Capacity => _items.Length;

    /// <summary>
    /// 获取或设置指定索引的元素.
    /// </summary>
    public readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _items[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _items[index] = value;
    }

    /// <summary>
    /// 添加元素.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_count == _items.Length)
        {
            Grow();
        }

        _items[_count++] = item;
    }

    /// <summary>
    /// 获取元素的只读跨度.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<T> AsSpan() => _items.AsSpan(0, _count);

    /// <summary>
    /// 转换为数组.
    /// </summary>
    public readonly T[] ToArray()
    {
        if (_count == 0)
        {
            return System.Array.Empty<T>();
        }

        var result = new T[_count];
        System.Array.Copy(_items, result, _count);
        return result;
    }

    /// <summary>
    /// 转换为 List.
    /// </summary>
    public readonly List<T> ToList()
    {
        var list = new List<T>(_count);
        for (var i = 0; i < _count; i++)
        {
            list.Add(_items[i]);
        }

        return list;
    }

    /// <summary>
    /// 释放内部数组.
    /// </summary>
    public void Dispose()
    {
        if (_items is not null)
        {
            ArrayPool<T>.Shared.Return(_items);
            _items = null!;
        }
    }

    private void Grow()
    {
        var newCapacity = _items.Length * 2;
        var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
        System.Array.Copy(_items, newArray, _count);
        ArrayPool<T>.Shared.Return(_items);
        _items = newArray;
    }
}
