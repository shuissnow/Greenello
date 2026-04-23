# テスト設計（バックエンド）

## テスト方針

- テストは Red → Green → Refactor の順で実装する
- テストケースは Given / When / Then 形式で記述する
- マイグレーションの検証は Testcontainers for .NET（PostgreSQL）を使用した統合テストで行う
- モックは使用せず、実際の PostgreSQL コンテナに対してマイグレーションを適用して検証する

## テスト対象

| 対象（クラス/関数） | ファイルパス |
|------------------|------------|
| `AppDbContext`（Fluent API 設定・マイグレーション） | `Greenello.API/Data/AppDbContext.cs` |

## テストケース一覧

| ケースID | テスト対象 | Given（前提条件） | When（操作） | Then（期待結果） | 正常/異常 |
|---------|----------|----------------|------------|----------------|---------|
| DB-001 | マイグレーション適用 | PostgreSQL コンテナが起動している | `database.MigrateAsync()` を実行する | 4テーブル（users / boards / tasks / notification_logs）が作成される | 正常 |
| DB-002 | users テーブル | マイグレーション適用済み | email が重複する users を2件 INSERT する | UNIQUE 制約違反例外がスローされる | 異常 |
| DB-003 | users テーブル | マイグレーション適用済み | `failed_login_count` を指定せず users を INSERT する | `failed_login_count` が `0` になっている | 正常 |
| DB-004 | boards テーブル | マイグレーション適用済み | 存在しない `owner_user_id` を持つ boards を INSERT する | FK 制約違反例外がスローされる | 異常 |
| DB-005 | boards テーブル | boards を持つ users が存在する | users を DELETE する | RESTRICT 制約により例外がスローされる | 異常 |
| DB-006 | tasks テーブル | マイグレーション適用済み | `status` に `'invalid'` を指定して tasks を INSERT する | CHECK 制約違反例外がスローされる | 異常 |
| DB-007 | tasks テーブル | boards を持つ tasks が存在する | 親 boards を DELETE する | tasks が CASCADE 削除される | 正常 |
| DB-008 | tasks テーブル | 親タスクにサブタスクが存在する | 親タスクを DELETE する | サブタスクが CASCADE 削除される | 正常 |
| DB-009 | tasks テーブル | assignee_user_id が設定された tasks が存在する | 担当者 users を DELETE する | `assignee_user_id` が NULL に更新される | 正常 |
| DB-010 | notification_logs テーブル | マイグレーション適用済み | 同一（task_id, user_id, sent_at）の notification_logs を2件 INSERT する | UNIQUE 制約違反例外がスローされる | 異常 |

## 使用ツール

- xUnit（テストフレームワーク）
- Testcontainers for .NET（PostgreSQL コンテナ起動）
- `Greenello.Tests/Helpers/TestDbContextFactory.cs`（テスト用 DbContext 生成ヘルパー）
