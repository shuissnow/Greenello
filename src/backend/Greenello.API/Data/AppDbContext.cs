using Greenello.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Greenello.API.Data;

/// <summary>
/// アプリケーション全体の EF Core DbContext。
/// 各エンティティの DbSet と Fluent API による物理スキーマ設定を管理する。
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>users テーブルへのアクセスポイント。</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>boards テーブルへのアクセスポイント。</summary>
    public DbSet<Board> Boards => Set<Board>();

    /// <summary>tasks テーブルへのアクセスポイント。</summary>
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    /// <summary>notification_logs テーブルへのアクセスポイント。</summary>
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    /// <summary>
    /// 変更を非同期で保存する。
    /// <see cref="User"/>・<see cref="Board"/>・<see cref="TaskEntity"/> の
    /// 更新時に <c>UpdatedAt</c> を現在時刻（UTC）に自動セットする。
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>DB に書き込まれた行数。</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
        {
            switch (entry.Entity)
            {
                case User user:
                    user.UpdatedAt = now;
                    break;
                case Board board:
                    board.UpdatedAt = now;
                    break;
                case TaskEntity task:
                    task.UpdatedAt = now;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Fluent API でテーブル名・カラム名・制約・インデックス・外部キーを設定する。
    /// 属性（Data Annotations）ではなく Fluent API に統一することで設定を一か所に集約する。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureUsers(modelBuilder);
        ConfigureBoards(modelBuilder);
        ConfigureTasks(modelBuilder);
        ConfigureNotificationLogs(modelBuilder);
    }

    /// <summary>users テーブルのスキーマを設定する。</summary>
    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);

            entity
                .Property(u => u.Id)
                .HasColumnName("id")
                // PostgreSQL の pgcrypto 関数で UUID を自動生成する
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity
                .Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255)
                .IsRequired();
            entity
                .Property(u => u.FailedLoginCount)
                .HasColumnName("failed_login_count")
                .HasDefaultValue(0);
            entity.Property(u => u.LockedUntil).HasColumnName("locked_until");
            entity
                .Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");
            entity
                .Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("users_email_key");
        });
    }

    /// <summary>boards テーブルのスキーマを設定する。</summary>
    private static void ConfigureBoards(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(entity =>
        {
            entity.ToTable("boards");
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(b => b.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(b => b.OwnerUserId).HasColumnName("owner_user_id").IsRequired();
            entity
                .Property(b => b.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");
            entity
                .Property(b => b.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            // ボードを持つユーザーは直接削除できない（RESTRICT）。
            // アプリケーション層でボードを先に削除してからユーザーを削除する。
            entity
                .HasOne(b => b.Owner)
                .WithMany(u => u.OwnedBoards)
                .HasForeignKey(b => b.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_boards_owner_user_id");

            entity.HasIndex(b => b.OwnerUserId).HasDatabaseName("idx_boards_owner_user_id");
        });
    }

    /// <summary>tasks テーブルのスキーマを設定する。CHECK 制約と自己参照 FK を含む。</summary>
    private static void ConfigureTasks(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable(
                "tasks",
                t =>
                {
                    // status・priority の許容値を DB レベルで制限する
                    t.HasCheckConstraint(
                        "chk_tasks_status",
                        "status IN ('inbox', 'in_progress', 'done')"
                    );
                    t.HasCheckConstraint(
                        "chk_tasks_priority",
                        "priority IN ('high', 'medium', 'low') OR priority IS NULL"
                    );
                }
            );
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(t => t.BoardId).HasColumnName("board_id").IsRequired();
            entity.Property(t => t.ParentTaskId).HasColumnName("parent_task_id");
            entity.Property(t => t.Title).HasColumnName("title").HasMaxLength(100).IsRequired();
            entity
                .Property(t => t.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .HasDefaultValue("inbox")
                .IsRequired();
            entity.Property(t => t.Priority).HasColumnName("priority").HasMaxLength(10);
            entity.Property(t => t.Category).HasColumnName("category").HasMaxLength(20);
            entity.Property(t => t.DueDate).HasColumnName("due_date");
            entity.Property(t => t.AssigneeUserId).HasColumnName("assignee_user_id");
            entity
                .Property(t => t.CreatedByUserId)
                .HasColumnName("created_by_user_id")
                .IsRequired();
            entity
                .Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");
            entity
                .Property(t => t.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("NOW()");

            // ボード削除時にタスクも連動削除する（CASCADE）
            entity
                .HasOne(t => t.Board)
                .WithMany(b => b.Tasks)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_tasks_board_id");

            // 親タスク削除時にサブタスクも連動削除する（CASCADE）。自己参照 FK。
            entity
                .HasOne(t => t.ParentTask)
                .WithMany(t => t.Subtasks)
                .HasForeignKey(t => t.ParentTaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_tasks_parent_task_id")
                .IsRequired(false);

            // 担当者ユーザーが削除された場合は assignee_user_id を NULL にする（SET NULL）
            entity
                .HasOne(t => t.Assignee)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssigneeUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_tasks_assignee_user_id")
                .IsRequired(false);

            // タスクを作成したユーザーは直接削除できない（RESTRICT）
            entity
                .HasOne(t => t.CreatedBy)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_tasks_created_by_user_id");

            entity.HasIndex(t => t.BoardId).HasDatabaseName("idx_tasks_board_id");
            entity.HasIndex(t => t.ParentTaskId).HasDatabaseName("idx_tasks_parent_task_id");
            entity.HasIndex(t => t.AssigneeUserId).HasDatabaseName("idx_tasks_assignee_user_id");
            // 通知ジョブが「期限切れ・未完了タスク」を効率的に抽出するための複合インデックス
            entity
                .HasIndex(t => new { t.Status, t.DueDate })
                .HasDatabaseName("idx_tasks_status_due_date");
        });
    }

    /// <summary>notification_logs テーブルのスキーマを設定する。</summary>
    private static void ConfigureNotificationLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("notification_logs");
            entity.HasKey(nl => nl.Id);

            entity
                .Property(nl => nl.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(nl => nl.TaskId).HasColumnName("task_id").IsRequired();
            entity.Property(nl => nl.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(nl => nl.SentAt).HasColumnName("sent_at").IsRequired();
            entity
                .Property(nl => nl.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("NOW()");

            // タスク削除時に通知ログも連動削除する（CASCADE）
            entity
                .HasOne(nl => nl.Task)
                .WithMany(t => t.NotificationLogs)
                .HasForeignKey(nl => nl.TaskId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_notification_logs_task_id");

            entity
                .HasOne(nl => nl.User)
                .WithMany(u => u.NotificationLogs)
                .HasForeignKey(nl => nl.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_notification_logs_user_id");

            // (task_id, user_id, sent_at) の組み合わせを UNIQUE にして1日1回の重複送信を防ぐ
            entity
                .HasIndex(nl => new
                {
                    nl.TaskId,
                    nl.UserId,
                    nl.SentAt,
                })
                .IsUnique()
                .HasDatabaseName("idx_notification_logs_task_user_date");
        });
    }
}
