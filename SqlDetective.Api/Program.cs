using SqlDetective.Data.InMemory;
using SqlDetective.Domain.Sessions.Generator;
using SqlDetective.Domain.Sessions.Repository;
using SqlDetective.Domain.Sessions.Service;
using SqlDetective.Domain.Progress.Service;
using SqlDetective.Domain.Progress.Data;
using SqlDetective.Data.Postgres;
using SqlDetective.Domain.Progress.Repository;
using SqlDetective.Data.Postgres.Schema;
using SqlDetective.Domain.Schema.Service;
using SqlDetective.Data.Postgres.Case;
using SqlDetective.Domain.Cases.Service;
using SqlDetective.Data.Postgres.Persons;
using SqlDetective.Domain.Persons.Service;
using SqlDetective.Data.Postgres.Session;
using SqlDetective.Domain.Sessions;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using SqlDetective.Domain.Schema;
using SqlDetective.Domain.Persons;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SupabaseOptions>(builder.Configuration.GetSection("Supabase"));
builder.Services.AddHttpClient<ISupabaseSchemaClient, SupabaseSchemaClient>();
builder.Services.AddHttpClient<ICaseService, SupabaseCaseService>();
builder.Services.AddHttpClient<IPersonService, SupabasePersonService>();
builder.Services.AddScoped<ISchemaService, SupabaseSchemaService>();

// Add services to the container.

//builder.Services.AddControllers();

builder.Services.AddControllers()
    .AddNewtonsoftJson(); // שימוש ב-NewtonsoftJson מבטיח ש-List<Dictionary> ו-JArray יעברו תקין ליוניטי

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
builder.Services.AddSingleton<ISessionRepository, PostgresSessionRepository>();

// DI
builder.Services.AddSingleton<IKeyGenerator, RandomKeyGenerator>();

// Domain services
builder.Services.AddScoped<ISessionService, SessionService>();

// Cache

builder.Services.AddSingleton<ISchemaCache, InMemorySchemaCache>();
builder.Services.AddSingleton<IPersonCache, InMemoryPersonCache>();
builder.Services.AddSingleton<ISessionCache, InMemorySessionCache>();


builder.Services.AddScoped<IGameProgressService, GameProgressService>();
builder.Services.AddScoped<IGameProgressRepository>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string connString = "Host=aws-0-eu-central-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.vwudsbcqlhwajpkmcpsz;Password=SqlDetective123!;Ssl Mode=Require;Trust Server Certificate=true;Pooling=false;";//config.GetConnectionString("SqlDetectiveDatabase");
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

//app.UseWelcomePage();
app.MapGet("/", () => "API is running! Use /api/persons to get data.");
app.MapGet("/test", () => "Server is alive and routing works!"); 

app.Run();
