namespace Greenello.API.Entities;

/// <summary>
/// タスクおよびサブタスクを表すエンティティ。
/// <see cref="ParentTaskId"/> が NULL の場合が親タスク、値を持つ場合がサブタスク。
/// サブタスクの下にさらにサブタスクを作ることはできない（2階層制限）。
/// </summary>
public class TaskEntity
{
    /// <summary>タスクを一意に識別する UUID。DB 側で自動生成される。</summary>
    public Guid Id { get; set; }

    /// <summary>このタスクが属するボードの ID。</summary>
    public Guid BoardId { get; set; }

    /// <summary>
    /// 親タスクの ID。NULL の場合はこのタスク自体が親タスク。
    /// 値を持つ場合はサブタスクであり、親タスクの削除に連動して削除される（CASCADE）。
    /// </summary>
    public Guid? ParentTaskId { get; set; }

    /// <summary>タスクのタイトル。最大 100 文字。</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// タスクの進捗ステータス。
    /// 許容値: <c>inbox</c>（未着手）/ <c>in_progress</c>（進行中）/ <c>done</c>（完了）。
    /// 新規作成時のデフォルトは <c>inbox</c>。
    /// </summary>
    public string Status { get; set; } = "inbox";

    /// <summary>
    /// タスクの優先度。
    /// 許容値: <c>high</c> / <c>medium</c> / <c>low</c> / NULL（未設定）。
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>タスクのカテゴリ。最大 20 文字の自由入力。NULL は未分類を意味する。</summary>
    public string? Category { get; set; }

    /// <summary>
    /// タスクの期限日。JST 基準の日付のみ（時刻なし）。
    /// NULL は期限なしを意味する。
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// タスクの担当者ユーザーの ID。NULL は未割り当てを意味する。
    /// 担当者ユーザーが削除された場合は NULL に更新される（SET NULL）。
    /// </summary>
    public Guid? AssigneeUserId { get; set; }

    /// <summary>タスクを作成したユーザーの ID。作成後は変更不可。</summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>レコード作成日時（UTC）。DB 側で NOW() が自動設定される。</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>レコード最終更新日時（UTC）。<see cref="AppDbContext"/> の SaveChangesAsync で自動更新される。</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>このタスクが属するボード。ナビゲーションプロパティ。</summary>
    public Board Board { get; set; } = null!;

    /// <summary>親タスク。このタスクがサブタスクの場合にセットされる。ナビゲーションプロパティ。</summary>
    public TaskEntity? ParentTask { get; set; }

    /// <summary>このタスクに属するサブタスク一覧。ナビゲーションプロパティ。</summary>
    public ICollection<TaskEntity> Subtasks { get; set; } = [];

    /// <summary>担当者ユーザー。NULL の場合は未割り当て。ナビゲーションプロパティ。</summary>
    public User? Assignee { get; set; }

    /// <summary>作成者ユーザー。ナビゲーションプロパティ。</summary>
    public User CreatedBy { get; set; } = null!;

    /// <summary>このタスクに関する通知送信ログ一覧。ナビゲーションプロパティ。</summary>
    public ICollection<NotificationLog> NotificationLogs { get; set; } = [];
}
