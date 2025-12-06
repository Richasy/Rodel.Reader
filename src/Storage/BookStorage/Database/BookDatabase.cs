// Copyright (c) Richasy. All rights reserved.

using Richasy.SqliteGenerator;

namespace Richasy.RodelReader.Storage.Book.Database;

/// <summary>
/// 数据库连接管理器.
/// </summary>
internal sealed class BookDatabase : ISqliteDatabase, IAsyncDisposable, IDisposable
{
    private readonly string _connectionString;
    private readonly ILogger<BookDatabase>? _logger;
    private SqliteConnection? _connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BookDatabase"/> class.
    /// </summary>
    /// <param name="databasePath">数据库文件路径.</param>
    /// <param name="logger">日志记录器.</param>
    public BookDatabase(string databasePath, ILogger<BookDatabase>? logger = null)
    {
        // 禁用连接池以确保关闭连接后可以删除文件
        _connectionString = $"Data Source={databasePath};Pooling=False";
        _logger = logger;
    }

    /// <summary>
    /// 获取数据库连接.
    /// </summary>
    /// <returns>SQLite 连接.</returns>
    public SqliteConnection GetConnection()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_connection is null)
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
            _logger?.LogDebug("Database connection opened: {ConnectionString}", _connectionString);

            // 启用外键约束
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }

        return _connection;
    }

    /// <summary>
    /// 初始化数据库（创建表）.
    /// </summary>
    /// <param name="cancellationToken">取消令牌.</param>
    /// <returns>初始化任务.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Initializing Book database...");

        var connection = GetConnection();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = Schema.CreateTablesSql;

        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger?.LogInformation("Book database initialized successfully.");
    }

    /// <summary>
    /// 创建命令.
    /// </summary>
    /// <param name="sql">SQL 语句.</param>
    /// <returns>SQLite 命令.</returns>
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
    public SqliteCommand CreateCommand(string sql)
    {
        var connection = GetConnection();
        var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return cmd;
    }
#pragma warning restore CA2100

    /// <summary>
    /// 开始事务.
    /// </summary>
    /// <returns>事务对象.</returns>
    public async Task<SqliteTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var connection = GetConnection();
        return (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        _disposed = true;
        _logger?.LogDebug("Database connection disposed.");
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync().ConfigureAwait(false);
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }

        _disposed = true;
        _logger?.LogDebug("Database connection disposed asynchronously.");
    }
}
