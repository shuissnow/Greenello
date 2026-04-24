using Greenello.API.Entities;
using Greenello.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Greenello.Tests.Data;

/// <summary>
/// AppDbContext の Fluent API 設定・マイグレーション結果を検証する統合テスト。
/// Testcontainers で起動した実際の PostgreSQL コンテナに対してテストを実行する。
/// Docker Desktop が起動していることが前提条件。
/// </summary>
/// <remarks>
/// フィクスチャから接続文字列を受け取り、DbContext ファクトリを初期化する。
/// <para>
/// <b>データ分離方針:</b> 各テストメソッドは一意なメールアドレス（db001@test.com 等）を使用して
/// テスト間のデータ競合を防ぐ。コンテナはテストクラス実行ごとに新規作成されるため、
/// テスト実行間のデータ汚染はない。同クラス内のテストは xUnit により順次実行されるため、
/// 同時実行による競合も発生しない。
/// </para>
/// </remarks>
public class AppDbContextTests(PostgreSqlContainerFixture fixture)
    : IClassFixture<PostgreSqlContainerFixture>
{
    // PostgreSQL の制約違反エラーコード（SQL State）
    private const string _uniqueViolation = "23505";
    private const string _foreignKeyViolation = "23503";
    private const string _checkViolation = "23514";
    private const string _testPasswordHash = "hash";

    private readonly TestDbContextFactory _factory = new(fixture.ConnectionString);

    /// <summary>
    /// DB 操作が PostgresException（または DbUpdateException でラップされた PostgresException）を
    /// スローすることを検証し、PostgresException を返すヘルパー。
    /// </summary>
    /// <param name="act">制約違反が期待される非同期操作。</param>
    /// <returns>スローされた <see cref="PostgresException"/>。</returns>
    private static async Task<PostgresException> AssertPostgresExceptionAsync(Func<Task> act)
    {
        var ex = await Assert.ThrowsAnyAsync<Exception>(act);
        // ExecuteSqlAsync は PostgresException を直接スローする。
        // SaveChangesAsync は DbUpdateException の InnerException として PostgresException を持つ。
        var pgEx = ex as PostgresException ?? ex.InnerException as PostgresException;
        Assert.NotNull(pgEx);
        return pgEx!;
    }

    /// <summary>DB-001: マイグレーション適用後に4テーブルが作成されることを確認する。</summary>
    [Fact]
    public async Task MigrateAsync_WhenApplied_CreatesFourTables()
    {
        // Given
        await using var ctx = _factory.CreateDbContext();

        // When
        // information_schema でテーブルの存在を確認する（__EFMigrationsHistory は除外）
        await using var connection = (NpgsqlConnection)ctx.Database.GetDbConnection();
        await connection.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*)::int FROM information_schema.tables "
                + "WHERE table_schema = 'public' AND table_type = 'BASE TABLE' "
                + "AND table_name = ANY(ARRAY['users','boards','tasks','notification_logs'])",
            connection
        );

        // Then
        var count = (int)(await cmd.ExecuteScalarAsync())!;
        Assert.Equal(4, count);
    }

    /// <summary>DB-002: users テーブルの email UNIQUE 制約を確認する。</summary>
    [Fact]
    public async Task Users_WhenDuplicateEmailInserted_ThrowsUniqueViolation()
    {
        // Given
        await using var ctx = _factory.CreateDbContext();
        await ctx.Database.ExecuteSqlAsync(
            $"INSERT INTO users (email, password_hash) VALUES ('db002@test.com', 'hash1')"
        );

        // When
        var act = async () =>
            await ctx.Database.ExecuteSqlAsync(
                $"INSERT INTO users (email, password_hash) VALUES ('db002@test.com', 'hash2')"
            );

        // Then
        var pgEx = await AssertPostgresExceptionAsync(act);
        Assert.Equal(_uniqueViolation, pgEx.SqlState);
    }

    /// <summary>DB-003: users テーブルの failed_login_count DB デフォルト値（0）を確認する。</summary>
    [Fact]
    public async Task Users_WhenInsertedWithoutFailedLoginCount_DefaultsToZero()
    {
        // Given
        await using var ctx = _factory.CreateDbContext();
        await ctx.Database.ExecuteSqlAsync(
            $"INSERT INTO users (email, password_hash) VALUES ('db003@test.com', 'hash')"
        );

        // When
        // EF Core 経由で SELECT して DB デフォルト値を検証する
        await using var verifyCtx = _factory.CreateDbContext();
        var user = await verifyCtx.Users.FirstAsync(u => u.Email == "db003@test.com");

        // Then
        Assert.Equal(0, user.FailedLoginCount);
    }

    /// <summary>DB-004: boards テーブルの owner_user_id FK 制約（存在しない users.id）を確認する。</summary>
    [Fact]
    public async Task Boards_WhenInsertedWithNonExistentOwnerUserId_ThrowsForeignKeyViolation()
    {
        // Given
        await using var ctx = _factory.CreateDbContext();
        var nonExistentUserId = Guid.NewGuid();

        // When
        var act = async () =>
            await ctx.Database.ExecuteSqlAsync(
                $"INSERT INTO boards (name, owner_user_id) VALUES ('Test Board', {nonExistentUserId})"
            );

        // Then
        var pgEx = await AssertPostgresExceptionAsync(act);
        Assert.Equal(_foreignKeyViolation, pgEx.SqlState);
    }

    /// <summary>DB-005: boards を持つ users を DELETE しようとすると RESTRICT 制約で失敗することを確認する。</summary>
    [Fact]
    public async Task Users_WhenDeletedWithOwnedBoards_ThrowsForeignKeyRestrict()
    {
        // Given
        // boards を持つ users を作成する
        Guid userId;
        await using (var ctx = _factory.CreateDbContext())
        {
            var user = new User { Email = "db005@test.com", PasswordHash = _testPasswordHash };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            userId = user.Id;

            var board = new Board { Name = "DB005 Board", OwnerUserId = userId };
            ctx.Boards.Add(board);
            await ctx.SaveChangesAsync();
        }

        // When
        // boards をロードしない新しいコンテキストで DELETE を試みて DB の RESTRICT を検証する
        await using var deleteCtx = _factory.CreateDbContext();
        var act = async () =>
            await deleteCtx.Database.ExecuteSqlAsync($"DELETE FROM users WHERE id = {userId}");

        // Then
        var pgEx = await AssertPostgresExceptionAsync(act);
        Assert.Equal(_foreignKeyViolation, pgEx.SqlState);
    }

    /// <summary>DB-006: tasks テーブルの status CHECK 制約（許容値外）を確認する。</summary>
    [Fact]
    public async Task Tasks_WhenInsertedWithInvalidStatus_ThrowsCheckViolation()
    {
        // Given
        // tasks の FK を満たすため user と board を先に作成する
        Guid userId,
            boardId;
        await using (var ctx = _factory.CreateDbContext())
        {
            var user = new User { Email = "db006@test.com", PasswordHash = _testPasswordHash };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            userId = user.Id;

            var board = new Board { Name = "DB006 Board", OwnerUserId = userId };
            ctx.Boards.Add(board);
            await ctx.SaveChangesAsync();
            boardId = board.Id;
        }

        // When
        await using var violationCtx = _factory.CreateDbContext();
        var act = async () =>
            await violationCtx.Database.ExecuteSqlAsync(
                $"INSERT INTO tasks (board_id, title, status, created_by_user_id) VALUES ({boardId}, 'Test Task', 'invalid', {userId})"
            );

        // Then
        var pgEx = await AssertPostgresExceptionAsync(act);
        Assert.Equal(_checkViolation, pgEx.SqlState);
    }

    /// <summary>DB-007: boards 削除時に属する tasks が CASCADE 削除されることを確認する。</summary>
    [Fact]
    public async Task Tasks_WhenParentBoardDeleted_AreCascadeDeleted()
    {
        // Given
        Guid boardId,
            taskId;
        await using (var ctx = _factory.CreateDbContext())
        {
            var user = new User { Email = "db007@test.com", PasswordHash = _testPasswordHash };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var board = new Board { Name = "DB007 Board", OwnerUserId = user.Id };
            ctx.Boards.Add(board);
            await ctx.SaveChangesAsync();
            boardId = board.Id;

            var task = new TaskEntity
            {
                BoardId = boardId,
                Title = "DB007 Task",
                CreatedByUserId = user.Id,
            };
            ctx.Tasks.Add(task);
            await ctx.SaveChangesAsync();
            taskId = task.Id;
        }

        // When
        // tasks をロードしない新しいコンテキストで board を削除して DB の CASCADE を検証する
        await using (var deleteCtx = _factory.CreateDbContext())
        {
            var board = await deleteCtx.Boards.FindAsync(boardId);
            Assert.NotNull(board);
            deleteCtx.Boards.Remove(board);
            await deleteCtx.SaveChangesAsync();
        }

        // Then
        await using var verifyCtx = _factory.CreateDbContext();
        var deletedTask = await verifyCtx.Tasks.FindAsync(taskId);
        Assert.Null(deletedTask);
    }

    /// <summary>DB-008: 親タスク削除時にサブタスクが CASCADE 削除されることを確認する。</summary>
    [Fact]
    public async Task Tasks_WhenParentTaskDeleted_SubtasksAreCascadeDeleted()
    {
        // Given
        Guid parentTaskId,
            subtaskId;
        await using (var ctx = _factory.CreateDbContext())
        {
            var user = new User { Email = "db008@test.com", PasswordHash = _testPasswordHash };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var board = new Board { Name = "DB008 Board", OwnerUserId = user.Id };
            ctx.Boards.Add(board);
            await ctx.SaveChangesAsync();

            var parentTask = new TaskEntity
            {
                BoardId = board.Id,
                Title = "DB008 Parent Task",
                CreatedByUserId = user.Id,
            };
            ctx.Tasks.Add(parentTask);
            await ctx.SaveChangesAsync();
            parentTaskId = parentTask.Id;

            var subtask = new TaskEntity
            {
                BoardId = board.Id,
                ParentTaskId = parentTaskId,
                Title = "DB008 Subtask",
                CreatedByUserId = user.Id,
            };
            ctx.Tasks.Add(subtask);
            await ctx.SaveChangesAsync();
            subtaskId = subtask.Id;
        }

        // When
        // サブタスクをロードしない新しいコンテキストで親タスクを削除して DB の CASCADE を検証する
        await using (var deleteCtx = _factory.CreateDbContext())
        {
            var parent = await deleteCtx.Tasks.FindAsync(parentTaskId);
            Assert.NotNull(parent);
            deleteCtx.Tasks.Remove(parent);
            await deleteCtx.SaveChangesAsync();
        }

        // Then
        await using var verifyCtx = _factory.CreateDbContext();
        var deletedSubtask = await verifyCtx.Tasks.FindAsync(subtaskId);
        Assert.Null(deletedSubtask);
    }

    /// <summary>DB-009: 担当者 users 削除時に tasks.assignee_user_id が SET NULL されることを確認する。</summary>
    [Fact]
    public async Task Tasks_WhenAssigneeUserDeleted_AssigneeUserIdIsSetToNull()
    {
        // Given
        // creator と assignee の2ユーザー、board、task を作成する
        Guid assigneeId,
            taskId;
        await using (var ctx = _factory.CreateDbContext())
        {
            var creator = new User
            {
                Email = "db009-creator@test.com",
                PasswordHash = _testPasswordHash,
            };
            var assignee = new User
            {
                Email = "db009-assignee@test.com",
                PasswordHash = _testPasswordHash,
            };
            ctx.Users.AddRange(creator, assignee);
            await ctx.SaveChangesAsync();
            assigneeId = assignee.Id;

            var board = new Board { Name = "DB009 Board", OwnerUserId = creator.Id };
            ctx.Boards.Add(board);
            await ctx.SaveChangesAsync();

            var task = new TaskEntity
            {
                BoardId = board.Id,
                Title = "DB009 Task",
                CreatedByUserId = creator.Id,
                AssigneeUserId = assigneeId,
            };
            ctx.Tasks.Add(task);
            await ctx.SaveChangesAsync();
            taskId = task.Id;
        }

        // When
        // task をロードしない新しいコンテキストで assignee を削除して DB の SET NULL を検証する
        await using (var deleteCtx = _factory.CreateDbContext())
        {
            var assignee = await deleteCtx.Users.FindAsync(assigneeId);
            Assert.NotNull(assignee);
            deleteCtx.Users.Remove(assignee);
            await deleteCtx.SaveChangesAsync();
        }

        // Then
        await using var verifyCtx = _factory.CreateDbContext();
        var updatedTask = await verifyCtx.Tasks.FindAsync(taskId);
        Assert.NotNull(updatedTask);
        Assert.Null(updatedTask.AssigneeUserId);
    }

    /// <summary>DB-010: notification_logs の (task_id, user_id, sent_at) UNIQUE 制約を確認する。</summary>
    [Fact]
    public async Task NotificationLogs_WhenDuplicateCombinationInserted_ThrowsUniqueViolation()
    {
        // Given
        // notification_logs の FK を満たすため user、board、task を作成する
        Guid userId,
            taskId;
        await using (var ctx = _factory.CreateDbContext())
        {
            var user = new User { Email = "db010@test.com", PasswordHash = _testPasswordHash };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            userId = user.Id;

            var board = new Board { Name = "DB010 Board", OwnerUserId = userId };
            ctx.Boards.Add(board);
            await ctx.SaveChangesAsync();

            var task = new TaskEntity
            {
                BoardId = board.Id,
                Title = "DB010 Task",
                CreatedByUserId = userId,
            };
            ctx.Tasks.Add(task);
            await ctx.SaveChangesAsync();
            taskId = task.Id;
        }

        // 最初の notification_log を INSERT する
        await using var logCtx = _factory.CreateDbContext();
        var sentAt = new DateOnly(2026, 4, 23);
        await logCtx.Database.ExecuteSqlAsync(
            $"INSERT INTO notification_logs (task_id, user_id, sent_at) VALUES ({taskId}, {userId}, {sentAt})"
        );

        // When
        // 同一の (task_id, user_id, sent_at) で2件目を INSERT する
        var act = async () =>
            await logCtx.Database.ExecuteSqlAsync(
                $"INSERT INTO notification_logs (task_id, user_id, sent_at) VALUES ({taskId}, {userId}, {sentAt})"
            );

        // Then
        var pgEx = await AssertPostgresExceptionAsync(act);
        Assert.Equal(_uniqueViolation, pgEx.SqlState);
    }
}
