using BsCCaseApi.Library;
using BsCCaseApi.Library.database;
using BsCCaseApi.Library.services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<ICaseService, CaseService>();

// migrate and seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate(); // applies migrations
    await DbInitializer.SeedData(context); // seeds data
}
