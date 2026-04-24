namespace Greenello.API.Entities;

/// <summary>
/// 期限切れタスクに対するメール通知の送信履歴を表すエンティティ。
/// 同一タスク・ユーザー・日付の組み合わせに対して重複送信しないために使用する。
/// </summary>
public class NotificationLog
{
    /// <summary>ログを一意に識別する UUID。DB 側で自動生成される。</summary>
    public Guid Id { get; set; }

    /// <summary>通知対象タスクの ID。タスク削除時にこのログも連動削除される（CASCADE）。</summary>
    public Guid TaskId { get; set; }

    /// <summary>通知先ユーザーの ID。</summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 通知メールを送信した日付（JST 基準）。
    /// TaskId・UserId・SentAt の組み合わせに UNIQUE 制約があり、1日1回の重複送信を防ぐ。
    /// </summary>
    public DateOnly SentAt { get; set; }

    /// <summary>レコード作成日時（UTC）。DB 側で NOW() が自動設定される。</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>通知対象タスク。ナビゲーションプロパティ。</summary>
    public TaskEntity Task { get; set; } = null!;

    /// <summary>通知先ユーザー。ナビゲーションプロパティ。</summary>
    public User User { get; set; } = null!;
}
