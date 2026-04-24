namespace Greenello.API.Entities;

/// <summary>
/// カンバンボードの基本情報を表すエンティティ。
/// ボードはオーナーユーザーが作成し、複数のタスクを持つ。
/// </summary>
public class Board
{
    /// <summary>ボードを一意に識別する UUID。DB 側で自動生成される。</summary>
    public Guid Id { get; set; }

    /// <summary>ボードの表示名。最大 100 文字。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>このボードを作成・所有するユーザーの ID。</summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>レコード作成日時（UTC）。DB 側で NOW() が自動設定される。</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>レコード最終更新日時（UTC）。<see cref="AppDbContext"/> の SaveChangesAsync で自動更新される。</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>このボードのオーナーユーザー。ナビゲーションプロパティ。</summary>
    public User Owner { get; set; } = null!;

    /// <summary>このボードに属するタスク一覧（親タスク・サブタスク含む）。</summary>
    public ICollection<TaskEntity> Tasks { get; set; } = [];
}
