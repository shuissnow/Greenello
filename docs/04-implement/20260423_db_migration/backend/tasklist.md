# タスクリスト

## 実装

- [ ] `Greenello.API/Models/User.cs` を作成する
- [ ] `Greenello.API/Models/Board.cs` を作成する
- [ ] `Greenello.API/Models/Task.cs` を作成する
- [ ] `Greenello.API/Models/NotificationLog.cs` を作成する
- [ ] `Greenello.API/Data/AppDbContext.cs` を作成し、4エンティティを DbSet として登録する
- [ ] Fluent API で users テーブルの制約・インデックスを設定する
- [ ] Fluent API で boards テーブルの制約・インデックス・外部キーを設定する
- [ ] Fluent API で tasks テーブルの制約・インデックス・外部キー（自己参照含む）を設定する
- [ ] Fluent API で notification_logs テーブルの制約・ユニークインデックス・外部キーを設定する
- [ ] `Program.cs` に AppDbContext を DI 登録し、`DATABASE_URL` 環境変数から接続文字列を読み込む設定を追加する
- [ ] `dotnet ef migrations add InitialCreate` を実行してマイグレーションファイルを生成する
- [ ] `dotnet ef database update` を実行してマイグレーションが正常に適用されることを確認する
