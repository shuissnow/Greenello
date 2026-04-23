# タスクリスト

## 単体テスト

- [ ] Testcontainers を使用したテスト用 PostgreSQL コンテナ起動ヘルパー（`TestDbContextFactory.cs`）を作成する
- [ ] DB-001: マイグレーション適用後に4テーブルが存在することを確認するテストを実装する
- [ ] DB-002: users テーブルの email UNIQUE 制約を確認するテストを実装する
- [ ] DB-003: users テーブルの `failed_login_count` デフォルト値（0）を確認するテストを実装する
- [ ] DB-004: boards テーブルの存在しない owner_user_id に対する FK 制約を確認するテストを実装する
- [ ] DB-005: boards を持つ users を DELETE した際の RESTRICT 制約を確認するテストを実装する
- [ ] DB-006: tasks テーブルの status CHECK 制約を確認するテストを実装する
- [ ] DB-007: boards 削除時の tasks CASCADE 削除を確認するテストを実装する
- [ ] DB-008: 親タスク削除時のサブタスク CASCADE 削除を確認するテストを実装する
- [ ] DB-009: 担当者 users 削除時の assignee_user_id SET NULL を確認するテストを実装する
- [ ] DB-010: notification_logs テーブルの UNIQUE 制約（task_id, user_id, sent_at）を確認するテストを実装する
