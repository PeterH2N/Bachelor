using BsCCaseApi.Library.Services;
using BsCCaseApi.Library.Store;
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
builder.Services.AddScoped<ICaseService, CaseService>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();

// migrate and seed database
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
context.Database.Migrate(); // applies migrations
await DbInitializer.SeedData(context); // seeds data

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();