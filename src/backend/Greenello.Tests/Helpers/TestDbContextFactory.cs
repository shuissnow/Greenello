using Greenello.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Greenello.Tests.Helpers;

/// <summary>
/// テスト用 AppDbContext のファクトリ。
/// EF Core のキャッシュ・変更トラッカーを完全にリセットするため、
/// テストメソッドごとに CreateDbContext() で新しいインスタンスを作成して使用する。
/// </summary>
/// <remarks>
/// テスト用 AppDbContext ファクトリを初期化する。
/// </remarks>
/// <param name="connectionString">PostgreSQL コンテナの接続文字列。</param>
public class TestDbContextFactory(string connectionString)
{
    /// <summary>
    /// 新しい <see cref="AppDbContext"/> インスタンスを生成して返す。
    /// 各テストセクション（Given / When / Then）で独立したコンテキストを使い分け、
    /// EF Core のキャッシュによる誤検知を防ぐこと。
    /// </summary>
    /// <returns>新しい <see cref="AppDbContext"/> インスタンス。</returns>
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AppDbContext(options);
    }
}
