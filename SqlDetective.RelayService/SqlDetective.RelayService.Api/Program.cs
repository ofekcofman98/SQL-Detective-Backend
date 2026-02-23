using SqlDetective.RelayService.Domain.Services;
using SqlDetective.RelayService.Infrastructure.Consumers;
using SqlDetective.RelayService.Infrastructure.Hubs;
using SqlDetective.RelayService.Infrastructure.MessageQueue;
using SqlDetective.RelayService.Infrastructure.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqConfiguration>(
    builder.Configuration.GetSection("RabbitMq")
);

builder.Services.AddSingleton<IMessageQueueService, RabbitMqService>();
builder.Services.AddScoped<ISessionValidationService, SessionValidationService>();
builder.Services.AddHttpClient();

// Register ExecutionResultConsumer as hosted service
builder.Services.AddHostedService<ExecutionResultConsumer>();

// Add SignalR
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(_ => true)//WithOrigins("http://localhost:3000", "http://localhost:5173") // React/Vite dev servers
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

// Map SignalR Hub
app.MapHub<GameRelayHub>("/hubs/relay");

app.Run();
