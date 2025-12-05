// Copyright (c) Reader Copilot. All rights reserved.

namespace Richasy.RodelReader.Utilities.FeedParser.Internal;

/// <summary>
/// 跳过前导空白字符的流包装器.
/// </summary>
/// <remarks>
/// 某些 RSS/Atom 源可能在 XML 声明之前有空白字符，
/// 这会导致 XmlReader 抛出异常。此流包装器会跳过这些前导空白。
/// </remarks>
internal sealed class LeadingWhitespaceSkippingStream : Stream
{
    private readonly Stream _innerStream;
    private bool _skippedLeadingWhitespace;

    public LeadingWhitespaceSkippingStream(Stream innerStream)
    {
        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
    }

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanSeek => _innerStream.CanSeek;

    public override bool CanWrite => false;

    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set
        {
            _innerStream.Position = value;
            // 如果回到开头，需要重新跳过空白
            if (value == 0)
            {
                _skippedLeadingWhitespace = false;
            }
        }
    }

    public override void Flush() => _innerStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        SkipLeadingWhitespaceIfNeeded();
        return _innerStream.Read(buffer, offset, count);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await SkipLeadingWhitespaceIfNeededAsync(cancellationToken).ConfigureAwait(false);
        return await _innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await SkipLeadingWhitespaceIfNeededAsync(cancellationToken).ConfigureAwait(false);
        return await _innerStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var result = _innerStream.Seek(offset, origin);
        if (origin == SeekOrigin.Begin && offset == 0)
        {
            _skippedLeadingWhitespace = false;
        }

        return result;
    }

    public override void SetLength(long value) => _innerStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        // 不释放内部流，由调用者负责
        base.Dispose(disposing);
    }

    private void SkipLeadingWhitespaceIfNeeded()
    {
        if (_skippedLeadingWhitespace)
        {
            return;
        }

        _skippedLeadingWhitespace = true;

        if (!_innerStream.CanRead)
        {
            return;
        }

        // 逐字节读取直到找到非空白字符
        int b;
        while ((b = _innerStream.ReadByte()) >= 0)
        {
            // 检查是否是空白字符（空格、制表符、换行、回车）
            if (b != ' ' && b != '\t' && b != '\n' && b != '\r')
            {
                // 找到非空白字符，回退一个字节
                if (_innerStream.CanSeek)
                {
                    _innerStream.Seek(-1, SeekOrigin.Current);
                }

                break;
            }
        }
    }

    private async Task SkipLeadingWhitespaceIfNeededAsync(CancellationToken cancellationToken)
    {
        if (_skippedLeadingWhitespace)
        {
            return;
        }

        _skippedLeadingWhitespace = true;

        if (!_innerStream.CanRead)
        {
            return;
        }

        var buffer = new byte[1];

        // 逐字节读取直到找到非空白字符
        while (await _innerStream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false) > 0)
        {
            var b = buffer[0];

            // 检查是否是空白字符（空格、制表符、换行、回车）
            if (b != ' ' && b != '\t' && b != '\n' && b != '\r')
            {
                // 找到非空白字符，回退一个字节
                if (_innerStream.CanSeek)
                {
                    _innerStream.Seek(-1, SeekOrigin.Current);
                }

                break;
            }
        }
    }
}
