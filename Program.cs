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

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Application Insights
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo { Title = "BobsBetting", Description = "Texas Hold 'Em Backend API", Version = "v1" });
});

// Cors
builder.Services.AddCors(opt => {
    opt.AddPolicy("reactApp", builder => {
        builder.WithOrigins("https://agreeable-meadow-066fe7903.5.azurestaticapps.net")
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

// Configure middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Handling request: " + context.Request.Path);
        await next.Invoke();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while handling the request.");
        throw;
    }
    logger.LogInformation("Finished handling request");
});


// ---------------- Endpoints ----------------


// SignalR Game
app.MapHub<GameHub>("/game/lobby");

// Testing endpoint
app.MapGet("/", () => "Hello World!");

// User endpoints
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

app.MapPost("/signup", async (BBDb db, SignupDto signupDto) =>
{
    // Check for an existing user with the same username
    var existingUsername = await db.Users.FirstOrDefaultAsync(u => u.Username == signupDto.Username);
    if (existingUsername is not null)
    {
        return Results.BadRequest("User with that username already exists.");
    }
    var existingEmail = await db.Users.FirstOrDefaultAsync(u => u.Email == signupDto.Email);
    if (existingEmail is not null)
    {
        return Results.BadRequest("User with that email already exists.");
    }

    // Create and add the new user
    var newUser = new User(signupDto.Username, signupDto.Email, signupDto.Password);
    var addUserResult = await db.Users.AddAsync(newUser);

    // Save changes to the database
    try
    {
        await db.SaveChangesAsync();
        return Results.Ok("User created successfully.");
    }
    catch (Exception ex)
    {
        // Log the exception (if you have a logging mechanism)
        // Return a server error result
        return Results.Problem($"An error occurred while creating the user: {ex.Message}");
    }
});

app.MapGet("/user/{id}", async (BBDb db, int id) => {
    var user = await db.Users.FirstOrDefaultAsync(u=> u.Id == id);
    return user;
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080"; // Default to 8080 if PORT is not set
app.Urls.Add($"http://*:{port}");
    
app.Run();
