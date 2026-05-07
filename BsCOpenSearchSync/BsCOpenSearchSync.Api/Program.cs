using BsCOpenSearchSync.Library.Services;
using BsCOpenSearchSync.Library.Store;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load("../../.env");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<CaseDbContext>();
builder.Services.AddScoped<ISyncService, SyncService>();

var app = builder.Build();
app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    
    app.MapOpenApi();
}

app.UseHttpsRedirection();

