# Greenello Backend

C# .NET 8 Web API + Entity Framework Core + PostgreSQL

---

## プロジェクトセットアップ

```bash
# ソリューション作成
dotnet new sln -n Greenello

# API プロジェクト作成
dotnet new webapi -n Greenello.API --framework net8.0

# テストプロジェクト作成
dotnet new xunit -n Greenello.Tests --framework net8.0

# ソリューションにプロジェクトを追加
dotnet sln add Greenello.API/Greenello.API.csproj
dotnet sln add Greenello.Tests/Greenello.Tests.csproj

# テストプロジェクトから API プロジェクトへの参照追加
dotnet add Greenello.Tests/Greenello.Tests.csproj reference Greenello.API/Greenello.API.csproj
```

---

## ビルド・実行

```bash
# ビルド（Roslyn アナライザーも同時実行）
dotnet build

# API 起動
dotnet run --project Greenello.API

# テスト実行
dotnet test

# 依存パッケージの復元
dotnet restore
```

---

## パッケージ管理

```bash
# NuGet パッケージ追加
dotnet add Greenello.API/Greenello.API.csproj package <PackageName>

# NuGet パッケージ削除
dotnet remove Greenello.API/Greenello.API.csproj package <PackageName>

# インストール済みパッケージ一覧
dotnet list Greenello.API/Greenello.API.csproj package
```

---

## Formatter（CSharpier）

```bash
# フォーマット実行（ファイルを書き換える）
dotnet csharpier .

# フォーマットチェックのみ（CI 用・ファイルは変更しない）
dotnet csharpier --check .
```

> **ローカルツールの初回セットアップ（新規クローン時）**
> ```bash
> dotnet tool restore
> ```

---

## Linter（Roslyn アナライザー）

`dotnet build` 実行時に自動実行される。以下の違反はビルドエラーになる。

### nullable 違反

`<WarningsAsErrors>nullable</WarningsAsErrors>` により、nullable 警告はすべてエラー扱い。

### 命名規則（`.editorconfig`）

| 対象 | ルール | 例 |
|------|-------|----|
| private フィールド | `_camelCase` | `_userRepository` |
| interface | `IPascalCase` | `IUserRepository` |
| 型・非 private メンバー | `PascalCase` | `class UserService`, `GetUser()` |

---

## ローカルツール管理

```bash
# dotnet-tools.json に記載のツールを一括インストール（新規クローン後に実行）
dotnet tool restore

# ツールの追加
dotnet tool install <ToolName>

# インストール済みツール一覧
dotnet tool list
```
