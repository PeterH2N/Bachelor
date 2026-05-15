using BsCOpenSearchSync.Business.Services;
using BsCOpenSearchSync.DataAccess.Store;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using OpenSearch.Net;
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
builder.Services.AddScoped<IOpenSearchLowLevelClient>(serviceProvider =>
{
    var nodeAddress = new Uri(builder.Configuration["OpenSearch:BaseUrl"]!);
    var user = Environment.GetEnvironmentVariable("OPENSEARCH_USER");
    var pass = Environment.GetEnvironmentVariable("OPENSEARCH_PASS");
    //var logger = serviceProvider.GetRequiredService<ILogger<OpenSearchLowLevelClient>>();
    var settings = new ConnectionConfiguration(nodeAddress)
        .BasicAuthentication(user, pass)
        .RequestTimeout(TimeSpan.FromMinutes(5))
        .ServerCertificateValidationCallback((o, cert, chain, errors) => true);
    //logger.LogDebug("Connecting to URI: {URI}", nodeAddress);
    //logger.LogDebug("Connecting as user: {User}, {Pass}", user, pass);
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

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();

// Migrate event database
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<EventDbContext>();
context.Database.Migrate(); // applies migrations

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

