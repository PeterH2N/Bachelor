using BsCOpenSearchSync.Business.Services;
using BsCOpenSearchSync.DataAccess.Store;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using CaseDbContext =  BsCCaseApi.DataAccess.Store.AppDbContext;

var builder = WebApplication.CreateBuilder(args);

Env.Load("../../.env");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CaseDbContext>();
builder.Services.AddDbContext<EventDbContext>();
builder.Services.AddScoped<ISyncService, SyncService>();

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

