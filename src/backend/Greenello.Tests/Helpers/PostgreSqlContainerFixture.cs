using Greenello.API.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Greenello.Tests.Helpers;

/// <summary>
/// xUnit クラスフィクスチャ。テストクラス単位で PostgreSQL コンテナを起動し、
/// EF Core マイグレーションを適用する。同一テストクラス内の全テストメソッドで共有される。
/// </summary>
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
#pragma warning disable CS0618 // PostgreSqlBuilder() の parameterless constructor は非推奨だが、4.11.0 時点では WithImage チェーンに必須
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("greenello_test")
        .WithUsername("postgres")
        .WithPassword("test")
        .Build();
#pragma warning restore CS0618

    /// <summary>テスト用 PostgreSQL コンテナへの接続文字列。</summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// テスト実行前にコンテナを起動し、マイグレーションを適用する。
    /// xUnit が IAsyncLifetime を検出して自動的に呼び出す。
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// テスト実行後にコンテナを破棄する。
    /// xUnit が IAsyncLifetime を検出して自動的に呼び出す。
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
