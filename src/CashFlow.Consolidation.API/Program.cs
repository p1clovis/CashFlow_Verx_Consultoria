using CashFlow.Consolidation.Application.Events;
using CashFlow.Consolidation.Application.Queries;
using CashFlow.Consolidation.Domain.Interfaces;
using CashFlow.Consolidation.Infrastructure.Messaging;
using CashFlow.Consolidation.Infrastructure.Persistence;
using CashFlow.Consolidation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ConsolidationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=consolidation.db"));

// ── MediatR ───────────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetDailyBalanceQuery).Assembly));

// ── Repositories & Handlers ───────────────────────────────────────────────────
builder.Services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
builder.Services.AddScoped<TransactionCreatedEventHandler>();

// ── RabbitMQ ──────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IConnection>(sp =>
{
    var config  = sp.GetRequiredService<IConfiguration>();
    var factory = new ConnectionFactory
    {
        HostName = config["RabbitMQ:Host"] ?? "localhost",
        Port     = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
        UserName = config["RabbitMQ:User"] ?? "guest",
        Password = config["RabbitMQ:Password"] ?? "guest"
    };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});
builder.Services.AddHostedService<RabbitMqConsumerService>();

// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CashFlow Consolidation API", Version = "v1" });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ConsolidationDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConsolidationDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
