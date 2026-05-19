using BsCCaseApi.Business;
using BsCCaseApi.Business.Helpers;
using BsCCaseApi.Business.Services;
using BsCCaseApi.DataAccess.Store;
using BsCCaseApi.Helpers;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.DataAccess.Store;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Env.Load("../../.env");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddDbContext<EventDbContext>();

// Services
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();

// sync
builder.Services.AddHttpClient<ISyncEventService, SyncEventService>((serviceProvider, client) =>
    {
        client.BaseAddress = new Uri(builder.Configuration["SyncService:BaseUrl"]!);
    })
    .AddTypedClient<ISyncEventService>((httpClient, serviceProvider) =>
    {
        var db = serviceProvider.GetRequiredService<AppDbContext>();
        var eventDb = serviceProvider.GetRequiredService<EventDbContext>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return new SyncEventService(eventDb, db, httpClient, loggerFactory.CreateLogger<SyncEventService>());
    })
    .AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.AddScoped<IModelFaker, ModelFaker>();
builder.Services.AddScoped<IDbSeeder, DbSeeder>();

builder.Services.AddControllers();

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    
    using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging();

app.MapControllers();

// migrate and seed database
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var initializer = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
await context.Database.MigrateAsync(); // applies migrations
await initializer.SeedData(); // seeds data

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