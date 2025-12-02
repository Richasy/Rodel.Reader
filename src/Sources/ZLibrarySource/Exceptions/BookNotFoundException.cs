// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Sources.ZLibrary.Exceptions;

/// <summary>
/// 书籍未找到异常.
/// </summary>
public sealed class BookNotFoundException : ZLibraryException
{
    /// <summary>
    /// 初始化 <see cref="BookNotFoundException"/> 类的新实例.
    /// </summary>
    public BookNotFoundException()
        : base("The specified book was not found.")
    {
    }

    /// <summary>
    /// 初始化 <see cref="BookNotFoundException"/> 类的新实例.
    /// </summary>
    /// <param name="bookId">书籍 ID.</param>
    public BookNotFoundException(string bookId)
        : base($"The book with ID '{bookId}' was not found.")
    {
        BookId = bookId;
    }

    /// <summary>
    /// 初始化 <see cref="BookNotFoundException"/> 类的新实例.
    /// </summary>
    /// <param name="message">异常消息.</param>
    /// <param name="innerException">内部异常.</param>
    public BookNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// 获取书籍 ID.
    /// </summary>
    public string? BookId { get; }
}
