using SqlDetective.ExecutionEngine.Data;
using SqlDetective.ExecutionEngine.Data.Consumers;
using SqlDetective.ExecutionEngine.Data.MessageQueue;
using SqlDetective.ExecutionEngine.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure RabbitMQ
builder.Services.Configure<RabbitMqConfiguration>(
    builder.Configuration.GetSection("RabbitMq")
);

// Register Execution Service (as Singleton for BackgroundService compatibility)
builder.Services.AddSingleton<IQueryExecutionService, PostgresQueryExecutionService>();

// Register RabbitMQ Consumer as Hosted Service
builder.Services.AddHostedService<ExecutionRequestConsumer>();

// Configure CORS for microservice communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGameService", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowGameService");
app.UseAuthorization();
app.MapControllers();

app.Run();
