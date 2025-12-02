// Copyright (c) Richasy. All rights reserved.

using System.Diagnostics;

namespace Richasy.RodelReader.Utilities.DownloadKit.Internal;

/// <summary>
/// 进度追踪器，用于计算下载速度并节流进度报告.
/// </summary>
internal sealed class ProgressTracker
{
    private readonly Stopwatch _stopwatch;
    private readonly int _throttleMs;
    private readonly object _lock = new();

    private long _totalBytes;
    private long _bytesReceived;
    private long _lastReportedBytes;
    private long _lastSpeedBytes;
    private double _lastSpeedTime;
    private double _currentSpeed;
    private DateTime _lastReportTime;
    private DownloadState _state;

    /// <summary>
    /// 初始化 <see cref="ProgressTracker"/> 类的新实例.
    /// </summary>
    /// <param name="throttleMs">进度报告节流时间（毫秒）.</param>
    public ProgressTracker(int throttleMs = DownloadOptions.DefaultProgressThrottleMs)
    {
        _throttleMs = throttleMs;
        _stopwatch = new Stopwatch();
        _lastReportTime = DateTime.MinValue;
        _state = DownloadState.Pending;
    }

    /// <summary>
    /// 获取已接收的字节数.
    /// </summary>
    public long BytesReceived
    {
        get
        {
            lock (_lock)
            {
                return _bytesReceived;
            }
        }
    }

    /// <summary>
    /// 获取总字节数.
    /// </summary>
    public long? TotalBytes
    {
        get
        {
            lock (_lock)
            {
                return _totalBytes > 0 ? _totalBytes : null;
            }
        }
    }

    /// <summary>
    /// 获取当前下载速度（字节/秒）.
    /// </summary>
    public double CurrentSpeed
    {
        get
        {
            lock (_lock)
            {
                return _currentSpeed;
            }
        }
    }

    /// <summary>
    /// 获取已用时间.
    /// </summary>
    public TimeSpan ElapsedTime => _stopwatch.Elapsed;

    /// <summary>
    /// 获取当前状态.
    /// </summary>
    public DownloadState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// 设置总字节数.
    /// </summary>
    /// <param name="totalBytes">总字节数.</param>
    public void SetTotalBytes(long? totalBytes)
    {
        lock (_lock)
        {
            _totalBytes = totalBytes ?? 0;
        }
    }

    /// <summary>
    /// 开始追踪.
    /// </summary>
    public void Start()
    {
        lock (_lock)
        {
            _state = DownloadState.Downloading;
            _stopwatch.Start();
            _lastSpeedTime = 0;
            _lastSpeedBytes = 0;
        }
    }

    /// <summary>
    /// 停止追踪.
    /// </summary>
    /// <param name="state">最终状态.</param>
    public void Stop(DownloadState state)
    {
        lock (_lock)
        {
            _state = state;
            _stopwatch.Stop();
        }
    }

    /// <summary>
    /// 更新已接收字节数.
    /// </summary>
    /// <param name="bytesReceived">新接收的字节数.</param>
    public void UpdateBytesReceived(long bytesReceived)
    {
        lock (_lock)
        {
            _bytesReceived += bytesReceived;
            UpdateSpeed();
        }
    }

    /// <summary>
    /// 检查是否应该报告进度.
    /// </summary>
    /// <returns>如果应该报告进度，则为 true.</returns>
    public bool ShouldReportProgress()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var timeSinceLastReport = (now - _lastReportTime).TotalMilliseconds;

            // 时间节流
            if (timeSinceLastReport < _throttleMs)
            {
                return false;
            }

            // 检查是否有显著变化（至少 0.5% 或 100KB）
            var bytesDiff = _bytesReceived - _lastReportedBytes;
            var percentDiff = _totalBytes > 0 ? (double)bytesDiff / _totalBytes * 100 : 0;

            if (bytesDiff < 102400 && percentDiff < 0.5)
            {
                return false;
            }

            _lastReportTime = now;
            _lastReportedBytes = _bytesReceived;
            return true;
        }
    }

    /// <summary>
    /// 获取当前进度信息.
    /// </summary>
    /// <returns>进度信息.</returns>
    public DownloadProgress GetProgress()
    {
        lock (_lock)
        {
            return new DownloadProgress(
                _bytesReceived,
                _totalBytes > 0 ? _totalBytes : null,
                _currentSpeed,
                _state);
        }
    }

    /// <summary>
    /// 强制获取最终进度（忽略节流）.
    /// </summary>
    /// <returns>最终进度信息.</returns>
    public DownloadProgress GetFinalProgress()
    {
        lock (_lock)
        {
            return new DownloadProgress(
                _bytesReceived,
                _totalBytes > 0 ? _totalBytes : null,
                _currentSpeed,
                _state);
        }
    }

    private void UpdateSpeed()
    {
        var currentTime = _stopwatch.Elapsed.TotalSeconds;
        var timeDiff = currentTime - _lastSpeedTime;

        // 每 0.5 秒更新一次速度
        if (timeDiff >= 0.5)
        {
            var bytesDiff = _bytesReceived - _lastSpeedBytes;
            _currentSpeed = bytesDiff / timeDiff;
            _lastSpeedTime = currentTime;
            _lastSpeedBytes = _bytesReceived;
        }
    }
}
