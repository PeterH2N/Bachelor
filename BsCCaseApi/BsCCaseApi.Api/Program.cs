using BsCCaseApi.Business;
using BsCCaseApi.Business.Services;
using BsCCaseApi.DataAccess.Store;
using BsCOpenSearchSync.Client;
using BsCOpenSearchSync.DataAccess.Store;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load("../../.env");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddDbContext<EventDbContext>();
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
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
    });

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();

// migrate and seed database
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
context.Database.Migrate(); // applies migrations
await initializer.SeedData(); // seeds data

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();