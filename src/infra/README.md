# Greenello — Docker インフラ

## ディレクトリ構成

```
src/infra/
  Dockerfile.frontend      # React + Vite (dev) / nginx (prod)
  Dockerfile.backend       # .NET SDK (dev) / ASP.NET Runtime (prod)
  nginx.conf               # フロントエンド本番用 nginx 設定
  docker-compose.yml       # 全環境共通のベース定義
  docker-compose.dev.yml   # 開発環境の上書き定義
  docker-compose.prod.yml  # 本番環境の上書き定義
  .env.example             # 環境変数のテンプレート
```

---

## 初回セットアップ

```bash
# .env.example をコピーして開発用環境変数ファイルを作成
cp .env.example .env.dev

# パスワード等を編集
vi .env.dev
```

---

## よく使うコマンド

すべてのコマンドは `src/infra/` ディレクトリから実行する。

### 開発環境

```bash
# 起動（ホットリロード有効）
docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up

# バックグラウンドで起動
docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up -d

# イメージを再ビルドして起動
docker compose -f docker-compose.yml -f docker-compose.dev.yml --env-file .env.dev up --build

# 停止
docker compose -f docker-compose.yml -f docker-compose.dev.yml down

# 停止＋ボリューム削除（DBを初期化したい場合）
docker compose -f docker-compose.yml -f docker-compose.dev.yml down -v
```

### 本番環境

```bash
# 起動
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d

# イメージを再ビルドして起動
docker compose -f docker-compose.yml -f docker-compose.prod.yml --env-file .env.prod up -d --build

# 停止
docker compose -f docker-compose.yml -f docker-compose.prod.yml down
```

### ログ確認

```bash
# 全サービスのログ
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs -f

# 特定サービスのログ
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs -f backend
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs -f frontend
docker compose -f docker-compose.yml -f docker-compose.dev.yml logs -f postgres
```

### コンテナ操作

```bash
# 実行中コンテナの確認
docker compose -f docker-compose.yml -f docker-compose.dev.yml ps

# コンテナに入る（シェル）
docker compose -f docker-compose.yml -f docker-compose.dev.yml exec backend bash
docker compose -f docker-compose.yml -f docker-compose.dev.yml exec frontend sh

# PostgreSQL に接続
docker compose -f docker-compose.yml -f docker-compose.dev.yml exec postgres \
  psql -U ${POSTGRES_USER} -d ${POSTGRES_DB}
```

### 特定サービスだけ再起動・再ビルド

```bash
# サービスを再起動
docker compose -f docker-compose.yml -f docker-compose.dev.yml restart backend

# サービスのみ再ビルド（起動はしない）
docker compose -f docker-compose.yml -f docker-compose.dev.yml build backend

# 再ビルドして再起動
docker compose -f docker-compose.yml -f docker-compose.dev.yml up -d --build backend
```

---

## 起動後のアクセス先

| サービス | URL | 備考 |
|---------|-----|------|
| Frontend | http://localhost:5173 | 開発時 / Vite HMR 有効 |
| Frontend | http://localhost:80 | 本番時 / nginx 経由 |
| Backend API | http://localhost:8080 | ASP.NET Core Web API |
| Mailpit UI | http://localhost:8025 | 開発時のみ / 送信メール確認 |
| PostgreSQL | localhost:5432 | 開発時のみ / 外部クライアント接続用 |

---

## Docker 構成の流れ

```
docker-compose.yml          ← ベース（サービス定義・ボリューム）
        +
docker-compose.dev.yml      ← 開発用上書き（ホットリロード・ポート・mailpit）
        or
docker-compose.prod.yml     ← 本番用上書き（ビルドターゲット・ポート）
```

### Dockerfile のステージ構成

**Dockerfile.frontend**
```
dev   → node:22-alpine   Vite dev server（ホットリロード）
build → node:22-alpine   npm run build でビルド成果物を生成
prod  → nginx:alpine     dist/ を配信（build ステージからコピー）
```

**Dockerfile.backend**
```
dev   → dotnet/sdk:8.0    dotnet watch run（ホットリロード）
build → dotnet/sdk:8.0    dotnet publish で成果物を生成
prod  → dotnet/aspnet:8.0 成果物のみ（SDK はイメージに含まれない）
```

ターゲットは Compose ファイルの `build.target` で切り替える。

### ボリュームの役割（開発環境）

| ボリューム | マウント先 | 目的 |
|-----------|-----------|------|
| bind mount | `/app` (frontend) | ソースコードのホットリロード |
| `frontend_node_modules` | `/app/node_modules` | ホストの node_modules と競合防止 |
| bind mount | `/app` (backend) | ソースコードのホットリロード |
| `backend_obj` | `/app/Greenello.API/obj` | ビルドキャッシュ保持 |
| `backend_bin` | `/app/Greenello.API/bin` | ビルドキャッシュ保持 |
| `postgres_data` | PostgreSQL データ領域 | DB データの永続化 |
