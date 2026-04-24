using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Greenello.API.Data;

/// <summary>
/// <c>dotnet ef migrations</c> などの EF Core CLI ツール向けのデザインタイムファクトリ。
/// CLI 実行時は DI コンテナが利用できないため、このファクトリが DbContext を直接生成する。
/// EF Core CLI は AppDbContextFactory がない場合、Program.cs を実行してホストをビルドし、
/// DI コンテナから AppDbContextを取得する仕組みを持っているが、それだとミドルウェア登録などの不要な実行も行われるのでFactoryがあると便利になる。
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// EF Core CLI ツールが呼び出す DbContext 生成メソッド。
    /// 環境変数 <c>DATABASE_URL</c> が未設定の場合は例外をスローする。
    /// <c>dotnet ef migrations</c> を実行する前に <c>.env.example</c> に従って環境変数を設定すること。
    /// </summary>
    /// <param name="args">CLI から渡される引数（通常は使用しない）。</param>
    /// <returns>設定済みの <see cref="AppDbContext"/> インスタンス。</returns>
    /// <exception cref="InvalidOperationException"><c>DATABASE_URL</c> 環境変数が未設定の場合。</exception>
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? throw new InvalidOperationException(
                "DATABASE_URL environment variable is not set. Set it before running 'dotnet ef migrations'."
            );

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
