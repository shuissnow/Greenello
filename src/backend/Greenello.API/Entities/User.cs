namespace Greenello.API.Entities;

/// <summary>
/// 登録ユーザーのアカウント情報を表すエンティティ。
/// ログイン失敗カウントとアカウントロック状態を含む。
/// </summary>
public class User
{
    /// <summary>ユーザーを一意に識別する UUID。DB 側で自動生成される。</summary>
    public Guid Id { get; set; }

    /// <summary>ログインに使用するメールアドレス。一意制約あり。</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>bcrypt でハッシュ化されたパスワード。平文は保存しない。</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 連続ログイン失敗回数。
    /// 一定回数に達するとアカウントがロックされる。成功時は 0 にリセットする。
    /// </summary>
    public int FailedLoginCount { get; set; }

    /// <summary>
    /// アカウントロックの解除日時（UTC）。
    /// NULL の場合はロックされていない。この日時を過ぎるとロックが自動解除される。
    /// </summary>
    public DateTimeOffset? LockedUntil { get; set; }

    /// <summary>レコード作成日時（UTC）。DB 側で NOW() が自動設定される。</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>レコード最終更新日時（UTC）。<see cref="AppDbContext"/> の SaveChangesAsync で自動更新される。</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>このユーザーがオーナーのボード一覧。</summary>
    public ICollection<Board> OwnedBoards { get; set; } = [];

    /// <summary>このユーザーが担当者として割り当てられているタスク一覧。</summary>
    public ICollection<TaskEntity> AssignedTasks { get; set; } = [];

    /// <summary>このユーザーが作成したタスク一覧。</summary>
    public ICollection<TaskEntity> CreatedTasks { get; set; } = [];

    /// <summary>このユーザーへの通知送信ログ一覧。</summary>
    public ICollection<NotificationLog> NotificationLogs { get; set; } = [];
}
