using CashFlow.Transactions.API.Middleware;
using CashFlow.Transactions.Application.Commands;
using CashFlow.Transactions.Application.Interfaces;
using CashFlow.Transactions.Domain.Interfaces;
using CashFlow.Transactions.Infrastructure.Messaging;
using CashFlow.Transactions.Infrastructure.Persistence;
using CashFlow.Transactions.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<TransactionsDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=transactions.db"));

// ── MediatR (CQRS) ─────────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateTransactionCommand).Assembly));

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// ── RabbitMQ ─────────────────────────────────────────────────────────────────
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
builder.Services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

// ── API ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CashFlow Transactions API", Version = "v1" });
});

// ── Health check ──────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TransactionsDbContext>();

var app = builder.Build();

// ── Migrate on start ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Needed for integration tests
public partial class Program { }
