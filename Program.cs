// Used for swagger
using Microsoft.OpenApi.Models;
// Used for DB stuff
using Microsoft.EntityFrameworkCore;

// MY STUFF:
using BobsBetting.DBModels;
using BobsBetting.DTOs;
using BobsBetting.Hub;
using BobsBetting.DataService;
using BobsBetting.Services;

// Init web app builder
var builder = WebApplication.CreateBuilder(args);

// Database connection
var connectionString = builder.Configuration.GetConnectionString("POKER_DB_CONNECTION_STRING");
builder.Services.AddNpgsql<BBDb>(connectionString);

builder.Services.AddMemoryCache();

builder.Services.AddSingleton<SharedDb>();

builder.Services.AddScoped<DeckService>();
builder.Services.AddScoped<GameCacheService>();
builder.Services.AddScoped<PokerGameService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo { Title = "BobsBetting", Description = "Texas Hold 'Em Backend API", Version = "v1" });
});

// Cors
builder.Services.AddCors(opt => {
    opt.AddPolicy("reactApp", builder => {
        builder.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// SignalR
builder.Services.AddSignalR();

// Init app
var app = builder.Build();

// Cors
app.UseCors("reactApp");

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

// SignalR
app.MapHub<GameHub>("/game/lobby");
    
app.MapGet("/", () => "Hello World!");

app.MapPost("/login", async (BBDb db, LoginDto loginDto) =>
{
    // Find user with matching username
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

    // If no user is found or the password doesn't match, return unauthorized
    if (user is null || user.Password != loginDto.Password)
    {
        return Results.Unauthorized();
    }

    // If user is found and password matches, return success
    return Results.Ok(new LoginResDto(user.Id, user.Username, user.Email, user.Chips));
});

app.MapGet("/user/{id}", async (BBDb db, int id) => {
    var user = await db.Users.FirstOrDefaultAsync(u=> u.Id == id);
    return user;
});
    
app.Run();
