// Used for swagger
using Microsoft.OpenApi.Models;
// Used for DB stuff
using Microsoft.EntityFrameworkCore;

// MY STUFF:
using BobsBetting.DBModels;
using BobsBetting.CacheModels;
using BobsBetting.DTOs;

// Cors
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Init web app builder
var builder = WebApplication.CreateBuilder(args);

// Database connection
var connectionString = builder.Configuration.GetConnectionString("PokerDB");
builder.Services.AddNpgsql<BBDb>(connectionString);

builder.Services.AddMemoryCache();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo { Title = "BobsBetting", Description = "Texas Hold 'Em Backend API", Version = "v1" });
});

// Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
      builder =>
      {
          builder.WithOrigins("*")
            .AllowAnyMethod()
            .AllowAnyHeader();
      });
});

// Init app
var app = builder.Build();

// Cors
app.UseCors(MyAllowSpecificOrigins);

// Use swagger if in Dev env
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI(c =>
   {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "BobsBetting API V1");
   });
}

// ---------------- Endpoints ----------------
    
app.MapGet("/", () => "Hello World!");
    
app.Run();
