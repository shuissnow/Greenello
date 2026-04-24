# Greenello Frontend

React + Vite + TypeScript + Tailwind CSS で構築した SPA です。

## セットアップ

### プロジェクト作成

```bash
npm create vite@latest . -- --template react-ts
npm install
```

### Tailwind CSS インストール

```bash
npm install tailwindcss @tailwindcss/vite
```

### Prettier インストール

```bash
npm install -D prettier eslint-config-prettier
```

### Vitest インストール

```bash
npm install -D vitest
```

---

## 実行コマンド

### 開発

| コマンド | 内容 |
|---------|------|
| `npm run dev` | 開発サーバー起動 |
| `npm run preview` | ビルド成果物のプレビューサーバー起動 |

### ビルド

| コマンド | 内容 |
|---------|------|
| `npm run build` | TypeScript コンパイル + Vite ビルド |

### コード品質

| コマンド | 内容 |
|---------|------|
| `npm run lint` | ESLint でコードチェック |
| `npm run format` | Prettier でソースを自動整形 |
| `npm run format:check` | フォーマットのチェックのみ（CI 向け） |

### テスト

| コマンド | 内容 |
|---------|------|
| `npm run test` | 単体テスト実行（Vitest） |

### その他（スクリプト未登録）

| コマンド | 内容 |
|---------|------|
| `npx tsc --noEmit` | 型チェックのみ（ビルドなし） |
| `npx eslint . --fix` | ESLint の自動修正 |

---

## React Compiler

このテンプレートでは React Compiler が有効になっています。詳細は[公式ドキュメント](https://react.dev/learn/react-compiler)を参照してください。

注意: Vite の開発・ビルドパフォーマンスに影響する場合があります。

## ESLint 設定の拡張

本番アプリケーションでは、型情報を活用したリントルールの有効化を推奨します。

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // tseslint.configs.recommended の代わりにこちらを使用
      tseslint.configs.recommendedTypeChecked,
      // より厳格なルールを使う場合
      tseslint.configs.strictTypeChecked,
      // スタイルルールを追加する場合
      tseslint.configs.stylisticTypeChecked,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
])
```

React 専用のリントルールを追加する場合は [eslint-plugin-react-x](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-x) と [eslint-plugin-react-dom](https://github.com/Rel1cx/eslint-react/tree/main/packages/plugins/eslint-plugin-react-dom) も利用できます。

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // React 向けリントルール
      reactX.configs['recommended-typescript'],
      // React DOM 向けリントルール
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
    },
  },
])
```

