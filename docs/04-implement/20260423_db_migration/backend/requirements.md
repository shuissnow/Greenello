# 機能要件

## 機能概要

EF Core マイグレーションを使用して、PostgreSQL 16 データベースに以下のテーブルを作成します。

- `users`
- `boards`
- `tasks`
- `notification_logs`

## ゴール

EF Core の Code First アプローチで全テーブルを定義し、マイグレーションファイルを生成・適用できる状態にします。

## 受け入れ条件

- [ ] `users`、`boards`、`tasks`、`notification_logs` の4テーブルが PostgreSQL に作成される
- [ ] 各テーブルのカラム型・制約・インデックス・外部キーがテーブル定義書と一致する
- [ ] `dotnet ef migrations add` でマイグレーションファイルが生成できる
- [ ] `dotnet ef database update` でマイグレーションが正常に適用される
- [ ] `AppDbContext` が DI コンテナに登録され、PostgreSQL への接続が環境変数 `DATABASE_URL` から読み込まれる

## スコープ

### 含むもの

- EF Core エンティティクラスの作成（`User.cs`、`Board.cs`、`Task.cs`、`NotificationLog.cs`）
- `AppDbContext` の作成と Fluent API による制約・インデックス・外部キー設定
- EF Core マイグレーションファイルの生成
- `Program.cs` への AppDbContext DI 登録

### 含まないもの

- リポジトリクラスの実装（Phase 2 以降で実装）
- シードデータの投入
- アプリケーションロジック全般

## 参照ドキュメント

- `docs/03-detailed-design/テーブル定義書.md`
- `docs/03-detailed-design/バックエンド・クラス・モジュール設計書.md`
- `docs/03-detailed-design/環境変数定義書.md`
