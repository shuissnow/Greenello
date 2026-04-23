# バックエンド実装ルール

## アーキテクチャ

レイヤー分離を必ず守ること。

| レイヤー | 責務 |
|---------|------|
| Presentation | HTTP リクエスト/レスポンスの受け渡し |
| Application | ユースケースの調整・トランザクション制御 |
| Domain | ビジネスルール・エンティティ定義 |
| Infrastructure | DB・外部サービスとの実際の通信 |

上位レイヤーが下位レイヤーに直接依存しないこと（依存逆転の原則を適用すること）。

## 削除

- 論理削除を基本とすること（`deleted_at` で管理）
- 物理削除は明示的に設計判断した場合のみ許可（実装計画に理由を記載すること）

## アクセス制御

- オーナー以外からのアクセスは HTTP 403 を返すこと

## セキュリティ必須対応

- パスワードは bcrypt でハッシュ化すること
- CSRF 対策を実装すること
- SQL インジェクション対策を行うこと（ORM のパラメータバインドを使用すること）
- ログイン成功時はセッション ID を再生成すること（セッション固定攻撃対策）

## ディレクトリ構成

```
Greenello.API/
  Controllers/        ← Presentation層
  Services/           ← Application層（IXxxService + XxxService）
  Repositories/       ← Infrastructure層（IXxxRepository + XxxRepository）
  Entities/           ← Domain層
  Dtos/               ← リクエスト・レスポンス・DTO
  Data/               ← DbContext・マイグレーション
  Middleware/         ← グローバル例外ハンドリング

Greenello.Tests/
  Unit/               ← 単体テスト（API と同構造でミラーリング）
```

## 命名規則

| 対象 | 規則 | 例 |
|------|------|----|
| クラス | PascalCase | `TaskService`, `BoardController` |
| インターフェース | I + PascalCase | `ITaskService`, `ITaskRepository` |
| メソッド | PascalCase | `GetByIdAsync`, `CreateTaskAsync` |
| 非同期メソッド | Async サフィックス | `GetByIdAsync`, `SaveChangesAsync` |
| プライベートフィールド | _ + camelCase | `_taskService`, `_dbContext` |
| ローカル変数・引数 | camelCase | `taskId`, `boardName` |
| DTO / Request / Response | PascalCase + 役割サフィックス | `TaskDto`, `CreateTaskRequest`, `TaskListResponse` |
