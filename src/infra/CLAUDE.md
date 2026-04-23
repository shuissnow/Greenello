# コード実装ルール

## 技術スタック

React + Vite（SPA）/ C# .NET Web API / PostgreSQL / Docker / AWS（EC2・RDS・S3・SES）

## アーキテクチャ

レイヤー分離を必ず守ること。

| レイヤー | 責務 |
|---------|------|
| Presentation | HTTP リクエスト/レスポンスの受け渡し |
| Application | ユースケースの調整・トランザクション制御 |
| Domain | ビジネスルール・エンティティ定義 |
| Infrastructure | DB・外部サービスとの実際の通信 |

上位レイヤーが下位レイヤーに直接依存しないこと（依存逆転の原則を適用すること）。

# 開発フロー

各ステップで対応するスキルを呼び出す。

```
実装計画     /plan-task
  ↓ レビュー  /review-test
コード実装   /implement-code
  ↓ レビュー  /review-code
テスト実装   /implement-test
  ↓ レビュー  /review-code
```

---

# 

## 設計上の制約（変更禁止）

### 認証

- セッション認証のみを使用すること
- JWT・OAuth・ソーシャルログインは禁止
- セッションは PostgreSQL にサーバーサイドで永続化すること

### 削除

- 物理削除のみを使用すること
- 論理削除フラグ（`deleted_at`・`is_deleted` 等）は追加禁止

### アクセス制御

- ボード操作はそのボードのオーナーのみ許可
- オーナー以外からのアクセスは HTTP 403 を返すこと

### API

- REST（JSON）のみ
- GraphQL・WebSocket は禁止

## セキュリティ必須対応

- パスワードは bcrypt でハッシュ化すること
- CSRF 対策を実装すること
- SQL インジェクション対策を行うこと（ORM のパラメータバインドを使用すること）
- ログイン成功時はセッション ID を再生成すること（セッション固定攻撃対策）

## ディレクトリ構成

`[TBD]` — 実装計画（`docs/04-implementation/`）確定後に追記する。

## 命名規則

`[TBD]` — 実装計画確定後に追記する。

---

# Docker 構成ルール

## サービス構成

| サービス | ポート | 環境 | 備考 |
|---------|--------|------|------|
| frontend | 5173 | 全環境 | dev: Vite dev server / prod: nginx |
| backend  | 8080  | 全環境 | ASP.NET Core Web API |
| postgres | 5432  | 全環境 | PostgreSQL 16 |
| mailpit  | 8025（UI）/ 1025（SMTP） | dev のみ | メール送信モック |

## Compose ファイル構成

```
docker-compose.yml          # 全環境共通のベース定義
docker-compose.dev.yml      # 開発環境の上書き定義
docker-compose.prod.yml     # 本番環境の上書き定義
```

起動コマンドは以下の通り。

```bash
# 開発
docker compose -f docker-compose.yml -f docker-compose.dev.yml up

# 本番
docker compose -f docker-compose.yml -f docker-compose.prod.yml up
```

## ホットリロード（開発環境）

- **frontend**: `src/frontend` をボリュームマウントし、Vite HMR を有効にすること
- **backend**: `dotnet watch run` を使用すること
- `node_modules` / `bin` / `obj` は名前付きボリュームで除外し、ホストと競合させないこと

## 環境変数管理

- `.env` はリポジトリにコミットしないこと（`.gitignore` に追加すること）
- `.env.example` をリポジトリにコミットし、必要な変数の一覧とコメントを記載すること
- 開発用は `.env.dev`、本番用は `.env.prod` として管理すること

## Dockerfile ルール

- 本番イメージは **multi-stage build** を使用し、最終イメージにビルドツールを含めないこと
- frontend: build stage（node）→ 実行 stage（nginx:alpine）
- backend: build stage（mcr.microsoft.com/dotnet/sdk）→ 実行 stage（mcr.microsoft.com/dotnet/aspnet）
- 開発用と本番用は `--target` で切り替えること（Dockerfile は1ファイルに統一）
