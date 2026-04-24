using Greenello.API.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 接続文字列は環境変数 DATABASE_URL から取得する。
// 未設定の場合は起動時に例外をスローして、設定漏れを早期に検知する。
var databaseUrl =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("DATABASE_URL environment variable is not set.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(databaseUrl));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(options =>
        options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json"
    );
}

app.UseHttpsRedirection();

app.Run();
