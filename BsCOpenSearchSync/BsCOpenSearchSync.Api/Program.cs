using System.Text;
using BsCOpenSearchSync.Business.Helpers;
using BsCOpenSearchSync.Business.Services;
using BsCOpenSearchSync.DataAccess.Store;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenSearch.Net;
using Quartz;
using Serilog;
using CaseDbContext =  BsCCaseApi.DataAccess.Store.AppDbContext;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", false);

var builder = WebApplication.CreateBuilder(args);

Env.Load("../../.env");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CaseDbContext>();
builder.Services.AddDbContext<EventDbContext>();
builder.Services.AddScoped<IStatsService, StatsService>(serviceProvider =>
{
    var caseDbContext = serviceProvider.GetRequiredService<CaseDbContext>();
    var openSearchClient = serviceProvider.GetRequiredService<IOpenSearchLowLevelClient>();
    return new StatsService(openSearchClient, caseDbContext);
});
builder.Services.AddScoped<IOpenSearchLowLevelClient>(_ =>
{
    var nodeAddress = new Uri(builder.Configuration["OpenSearch:BaseUrl"]!);
    var user = Environment.GetEnvironmentVariable("OPENSEARCH_USER");
    var pass = Environment.GetEnvironmentVariable("OPENSEARCH_PASS");
    var settings = new ConnectionConfiguration(nodeAddress)
        .BasicAuthentication(user, pass)
        .RequestTimeout(TimeSpan.FromMinutes(5))
        .ServerCertificateValidationCallback((o, cert, chain, errors) => true);
    return new OpenSearchLowLevelClient(settings);
});
builder.Services.AddScoped<ISyncService, SyncService>(serviceProvider =>
{
    var eventDbContext = serviceProvider.GetRequiredService<EventDbContext>();
    var caseDbContext = serviceProvider.GetRequiredService<CaseDbContext>();
    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    var openSearchClient = serviceProvider.GetRequiredService<IOpenSearchLowLevelClient>();
    
    return new SyncService(eventDbContext, caseDbContext, openSearchClient, loggerFactory.CreateLogger<SyncService>());
});
builder.Services.AddScoped<IOpenSearchHealthCheck, OpenSearchHealthCheck>();

// Health check job
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(builder.Configuration["OpenSearch:JobKey"]!);
    q.AddJob<OpenSearchHealthJob>(jobKey);
    q.AddTrigger(t => t
        .ForJob(jobKey)
        .WithCronSchedule("0 */1 * * * ?")); // every minute
});
builder.Services.AddQuartzHostedService();

builder.Services.AddControllers();

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Correlation from case api
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() 
                        ?? Guid.NewGuid().ToString();
    
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();

app.MapControllers();

// Migrate event database
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();
await context.Database.MigrateAsync(); // applies migrations

// do Sync on startup
var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
await syncService.DoAllSyncs();

// Health check
app.MapGet("/health", () => Results.Ok());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.Run();




