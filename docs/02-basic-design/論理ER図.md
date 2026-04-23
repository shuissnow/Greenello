# 論理ER図

## エンティティ一覧

| エンティティ名 | テーブル名 | 概要 |
|-------------|---------|------|
| ユーザー | users | 登録ユーザーの認証情報 |
| ボード | boards | タスク管理の単位。作成者のみがアクセスできる |
| タスク | tasks | ユーザーが管理する作業単位。サブタスクも同テーブル |
| 通知送信履歴 | notification_logs | 期限切れタスクへの通知送信記録（重複送信防止） |

## ER図

```mermaid
erDiagram
    users ||--o{ boards : "作成（owner）"
    boards ||--o{ tasks : "持つ"
    tasks ||--o{ tasks : "親子（parent_task_id）"
    users ||--o{ tasks : "担当（assignee_user_id）"
    tasks ||--o{ notification_logs : "持つ"
    users ||--o{ notification_logs : "受け取る"

    users {
        UUID id PK
        VARCHAR email
        VARCHAR password_hash
        INT failed_login_count
        TIMESTAMP locked_until
        TIMESTAMP created_at
        TIMESTAMP updated_at
    }

    boards {
        UUID id PK
        VARCHAR name
        UUID owner_user_id FK
        TIMESTAMP created_at
        TIMESTAMP updated_at
    }

    tasks {
        UUID id PK
        UUID board_id FK
        UUID parent_task_id FK
        VARCHAR title
        VARCHAR status
        VARCHAR priority
        VARCHAR category
        DATE due_date
        UUID assignee_user_id FK
        UUID created_by_user_id FK
        TIMESTAMP created_at
        TIMESTAMP updated_at
    }

    notification_logs {
        UUID id PK
        UUID task_id FK
        UUID user_id FK
        DATE sent_at
        TIMESTAMP created_at
    }
```

## リレーションシップ説明

| リレーション | カーディナリティ | 説明 |
|-----------|--------------|------|
| users → boards | 1対多 | ユーザーは複数のボードをオーナーとして作成できる |
| boards → tasks | 1対多 | ボードは複数のタスクを持つ |
| tasks → tasks | 1対多 | タスクは複数のサブタスクを持つ（2階層まで） |
| users → tasks | 1対多 | ユーザーは複数のタスクの担当者になれる |
| tasks → notification_logs | 1対多 | タスクは複数の通知履歴を持つ |
| users → notification_logs | 1対多 | ユーザーは複数の通知履歴を受け取る |
