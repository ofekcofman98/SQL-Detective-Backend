using SqlDetective.Data.InMemory;
using SqlDetective.Domain.Query.Repository;
using SqlDetective.Domain.Query.Service;
using SqlDetective.Domain.Sessions.Generator;
using SqlDetective.Domain.Sessions.Repository;
using SqlDetective.Domain.Sessions.Service;
using SqlDetective.Domain.Progress.Service;
using SqlDetective.Domain.Progress.Data;
using SqlDetective.Data.Postgres;
using SqlDetective.Domain.Progress.Repository;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnityClients", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// =====================
//      DI REGISTRATION
// =====================

// Repositories (InMemory)
builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>();
builder.Services.AddSingleton<IRelayQueryRepository, InMemoryRelayQueryRepository>();

// DI
builder.Services.AddSingleton<IKeyGenerator, RandomKeyGenerator>();

// Domain services
builder.Services.AddScoped<IQueryRelayService, QueryRelayService>();
builder.Services.AddScoped<ISessionService, SessionService>();

builder.Services.AddScoped<IGameProgressService, GameProgressService>();
builder.Services.AddScoped<IGameProgressRepository>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string connString = config.GetConnectionString("SqlDetectiveDatabase");
    var logger = sp.GetRequiredService<ILogger<PostgresGameProgressRepository>>();

    Console.WriteLine($"[DB] Using connection string: {connString}");
    return new PostgresGameProgressRepository(connString, logger);
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// =====================
//   HTTP PIPELINE
// =====================


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowUnityClients");

app.UseAuthorization();

app.MapControllers();

app.Run();
