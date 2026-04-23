# フロントエンド実装ルール

## アーキテクチャ原則

- コンポーネントは表示のみ担当すること
- データ取得・ビジネスロジックはカスタムフックに分離すること

## ディレクトリ構成

```
src/
  components/     ← 再利用可能なUIコンポーネント
  hooks/          ← カスタムフック（データ取得・状態管理）
  pages/          ← ページコンポーネント（ルート単位）
  api/            ← API通信層
  types/          ← TypeScript型定義
  utils/          ← ユーティリティ関数
```

## 命名規則

| 対象 | 規則 | 例 |
|------|------|----|
| コンポーネント | PascalCase | `TaskCard`, `BoardList` |
| コンポーネントファイル | PascalCase.tsx | `TaskCard.tsx`, `BoardList.tsx` |
| カスタムフック | use + PascalCase | `useTaskList`, `useBoardQuery` |
| フックファイル | camelCase.ts | `useTaskList.ts` |
| 型・インターフェース | PascalCase | `Task`, `Board`, `TaskStatus` |
| Props 型 | ComponentName + Props | `TaskCardProps`, `BoardListProps` |
| 変数・関数 | camelCase | `taskId`, `handleSubmit` |
| 定数 | UPPER_SNAKE_CASE | `MAX_TASK_COUNT` |

## TypeScript 型定義方針

- 型は必ず明示的に記載すること（型推論に頼らない）
- `any` は使わないこと。`unknown` + 型ガードで代替する
- 関数の引数・戻り値すべてに型を付けること
- API リクエスト/レスポンス型はバックエンドの DTO に合わせて定義すること
