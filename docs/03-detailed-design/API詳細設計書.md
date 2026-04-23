# API詳細設計書

## 概要

エンドポイント定義・リクエスト/レスポンス仕様・バリデーションルール・エラーコードは OpenAPI スペックを正とする。
本書はスペックに表現できない実装上の判断・背景を記録する。

- **OpenAPI スペックファイル:** `backend/openapi.yaml`
- **Scalar UI:** 開発時は `GET /scalar/v1` で参照可能

---

## エンドポイント別 実装メモ

### POST /auth/register

- パスワードは bcrypt でハッシュ化して保存する（平文保存禁止）
- メール重複チェックは DB のユニーク制約に依存せず、Service 層で事前チェックして `CONFLICT` を返す

### POST /auth/login

- 認証失敗ごとに `failed_login_count` をインクリメントする
- 10回失敗で `locked_until = NOW() + 30分` を設定し `ACCOUNT_LOCKED` を返す
- 認証成功時は `failed_login_count` を0にリセットする

### PATCH /auth/password

- パスワード変更成功後は全セッションを無効化する（他端末からの強制ログアウト）

### GET /boards/:boardId/tasks/export

- CSV は UTF-8 BOM 付きで出力する（Excel での文字化け防止）
- 対象タスクが0件の場合はファイル生成せず `EXPORT_NO_TASKS` を返す

### PATCH /tasks/:taskId

- フロントエンドのオプティミスティック更新に対応するため、更新後の完全なタスクオブジェクトを返す
- ステータス変更（DnD 操作）もこのエンドポイントを使用する

### POST /tasks/:taskId/subtasks

- サブタスクへのさらなるサブタスク追加は禁止（2階層制限）
- `:taskId` が既にサブタスクである場合は `VALIDATION_ERROR` を返す

### DELETE /tasks/:taskId

- 親タスク削除時はサブタスクを先に削除してから親を削除する（外部キー制約対応）
