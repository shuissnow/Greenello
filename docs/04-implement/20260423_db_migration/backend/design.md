# 実装設計

## 実装方針

- EF Core の Code First アプローチでエンティティを定義し、Fluent API で制約・インデックス・外部キーを設定します
- PostgreSQL の UUID 主キーには `gen_random_uuid()` をデフォルト値として設定します
- 接続文字列は環境変数 `DATABASE_URL` から読み込みます（Npgsql 接続文字列形式）
- `AppDbContext` は `Data/AppDbContext.cs` に配置します
- エンティティクラスは `Models/` フォルダに配置します

## 作成・変更ファイル一覧

| ファイルパス | 作成/変更 | 概要 |
|------------|---------|------|
| `Greenello.API/Models/User.cs` | 作成 | users テーブルのエンティティ |
| `Greenello.API/Models/Board.cs` | 作成 | boards テーブルのエンティティ |
| `Greenello.API/Models/Task.cs` | 作成 | tasks テーブルのエンティティ |
| `Greenello.API/Models/NotificationLog.cs` | 作成 | notification_logs テーブルのエンティティ |
| `Greenello.API/Data/AppDbContext.cs` | 作成 | EF Core DbContext・Fluent API 設定 |
| `Greenello.API/Program.cs` | 変更 | AppDbContext を DI 登録・DATABASE_URL 読み込み |
| `Greenello.API/Migrations/` | 作成 | EF Core 自動生成マイグレーションファイル |

## データフロー

```
環境変数 DATABASE_URL
  → Program.cs で Npgsql 接続文字列として AppDbContext に登録
  → dotnet ef database update で PostgreSQL にテーブル作成
```

## 主要ロジック

### AppDbContext の Fluent API 設定

**users テーブル**

| 設定項目 | 内容 |
|---------|------|
| `id` | UUID PK、`HasDefaultValueSql("gen_random_uuid()")` |
| `email` | UNIQUE 制約（`HasIndex(u => u.Email).IsUnique()`） |
| `password_hash` | NOT NULL |
| `failed_login_count` | NOT NULL、`HasDefaultValue(0)` |
| `locked_until` | NULL 許容 |
| `created_at` / `updated_at` | NOT NULL、`HasDefaultValueSql("NOW()")` |

**boards テーブル**

| 設定項目 | 内容 |
|---------|------|
| `owner_user_id` | FK → users.id、`OnDelete(DeleteBehavior.Restrict)` |
| インデックス | `idx_boards_owner_user_id` |

**tasks テーブル**

| 設定項目 | 内容 |
|---------|------|
| `board_id` | FK → boards.id、`OnDelete(DeleteBehavior.Cascade)` |
| `parent_task_id` | FK → tasks.id（自己参照）、`OnDelete(DeleteBehavior.Cascade)`、NULL 許容 |
| `assignee_user_id` | FK → users.id、`OnDelete(DeleteBehavior.SetNull)`、NULL 許容 |
| `created_by_user_id` | FK → users.id、`OnDelete(DeleteBehavior.Restrict)` |
| `status` | CHECK 制約: `inbox` / `in_progress` / `done` |
| `priority` | CHECK 制約: `high` / `medium` / `low` / NULL |
| インデックス | `idx_tasks_board_id`、`idx_tasks_parent_task_id`、`idx_tasks_assignee_user_id`、`idx_tasks_status_due_date`（複合） |

**notification_logs テーブル**

| 設定項目 | 内容 |
|---------|------|
| `task_id` | FK → tasks.id、`OnDelete(DeleteBehavior.Cascade)` |
| `user_id` | FK → users.id、`OnDelete(DeleteBehavior.Restrict)` |
| UNIQUE インデックス | `idx_notification_logs_task_user_date`（task_id, user_id, sent_at） |
