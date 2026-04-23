# CI/CD設計書

## 方針

- GitHub Actions を使用する
- `main` ブランチへの push がデプロイのトリガー
- 全てのチェックが Pass しない限りデプロイに進まない
- デプロイ先は AWS EC2（Docker Compose）

---

## ブランチ戦略

| ブランチ種別 | 命名規則 | 用途 |
|------------|---------|------|
| メイン | `main` | 本番環境に対応する常にデプロイ可能なブランチ |
| 機能開発 | `feature/<概要>` | 新機能の開発 |
| バグ修正 | `fix/<概要>` | 通常のバグ修正 |
| 緊急修正 | `hotfix/<概要>` | 本番障害の緊急対応 |
| リファクタリング | `refactor/<概要>` | 機能変更を伴わないコード改善 |
| 雑務 | `chore/<概要>` | 依存関係の更新・設定変更など |

- **マージ方式:** Squash merge（PR 単位で `main` の履歴を 1 コミットにまとめる）
- **Branch Protection:** 現時点では未設定（個人開発のため）

---

## パイプライン一覧

| パイプライン名 | トリガー | 目的 |
|-------------|---------|------|
| CI（検証） | Pull Request → main | コード品質・テストの検証 |
| CD（デプロイ） | Push to main | 本番環境への自動デプロイ |

---

## パイプライン: CI（Pull Request 検証）

- **トリガー:** `main` ブランチへの Pull Request 作成・更新
- **ステージ一覧:**
  | ステージ | 処理内容 | 失敗時の挙動 |
  |---------|---------|------------|
  | frontend-format | Prettier によるフォーマットチェック (`prettier --check`) | PR マージをブロック |
  | frontend-lint | ESLint + TypeScript 型チェック (`tsc --noEmit`) | PR マージをブロック |
  | frontend-build | `npm run build` で成果物を生成（ビルドエラー検出） | PR マージをブロック |
  | frontend-test | Vitest による単体テスト実行 | PR マージをブロック |
  | backend-format | `dotnet csharpier . --check` | PR マージをブロック |
  | backend-lint | `dotnet build` | PR マージをブロック |
  | backend-test | `dotnet test` による単体・統合テスト実行 | PR マージをブロック |

- **デプロイ先:** なし（CI のみ）
- **ロールバック手順:** なし

---

## パイプライン: CD（本番デプロイ）

> **未確定 - 後回し**  
> 本番環境へのデプロイが必要になった時点で設計する。
