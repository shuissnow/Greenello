# コード実装ルール

## 技術スタック

React + Vite（SPA）/ C# .NET Web API / PostgreSQL / Docker / AWS（EC2・RDS・S3・SES）

## 開発フロー（新機能実装時）

ユーザーが呼び出すスキルは 4 つ。レビューが 4/5 以上なら次の実装ステップへ自動的に進む。

```
[ユーザー] /plan-task
    ↓
[ユーザー] /review-test ──── 4/5未満: 修正して再呼び出し
    │ 4/5以上
    ↓ 自動
[自動]     /implement-code
    ↓
[ユーザー] /review-code-impl ── 4/5未満: 修正して再呼び出し
           format→lint→build→test → レビュー
    │ 4/5以上
    ↓ 自動
[自動]     /implement-test
    ↓
[ユーザー] /review-code-test ── 4/5未満: 修正して再呼び出し
           format→lint→build→test → レビュー
    │ 4/5以上
    ↓
    完了
```

### /review-code-impl・/review-code-test の品質チェック順序

| # | フロントエンド（`src/frontend/`） | バックエンド（`src/backend/`） |
|---|----------------------------------|-------------------------------|
| 1 | `npm run format` | `dotnet csharpier .` |
| 2 | `npm run lint` | `dotnet build` |
| 3 | `npm run build` | `dotnet test` |
| 4 | `npm run test` | — |

品質チェックが 1 つでも失敗した場合はレビューを実行しない。

## Git 運用ルール

### ブランチ戦略

- `main` への直接コミットは禁止
- 作業は `feature/機能名` ブランチで行い、PR を通じて `main` にマージする

### コミットタイミング

開発フローの各マイルストーン通過時にコミットする：

| タイミング | コミット対象 |
|---|---|
| `/plan-task` 完了後 | 実装計画・テスト計画ドキュメント |
| `/review-code-impl` 通過後 | 実装コード |
| `/review-code-test` 通過後 | テストコード |

### コミットメッセージ形式

Conventional Commits に従う：

```
feat: ボード作成機能を追加
fix: カード削除時のエラーを修正
docs: 実装計画ドキュメントを追加
test: ボードAPIの単体テストを追加
```

### 作業単位

- 1機能（`/plan-task` 1回分の範囲）= 1PR とする

## 設計上の制約（変更禁止）

### 認証

- セッション認証のみを使用すること
- JWT・OAuth・ソーシャルログインは禁止
- セッションは PostgreSQL にサーバーサイドで永続化すること

### アクセス制御

- ボード操作はそのボードのオーナーのみ許可

### API

- REST（JSON）のみ
- GraphQL・WebSocket は禁止

