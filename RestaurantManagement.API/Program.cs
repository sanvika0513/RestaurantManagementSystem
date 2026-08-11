using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantManagement.API.Data;
using RestaurantManagement.API.Models;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// API URLS
// ======================================================

// Render provides the PORT environment variable.
// Locally, we use port 5000 if PORT is not available.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ======================================================
// DATABASE
// ======================================================

// Production:
// Set RestaurantDatabase in Render Environment Variables.
//
// Local:
// If no environment variable is provided, this fallback
// connection string is used.
var connectionString =
    builder.Configuration.GetConnectionString("RestaurantDatabase")
    ?? Environment.GetEnvironmentVariable("RestaurantDatabase")
    ?? "Host=localhost;Port=5432;Database=restaurant_management;Username=postgres;Password=1234;";

builder.Services.AddDbContext<RestaurantContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure();
        }
    )
);

// ======================================================
// IDENTITY
// ======================================================

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<RestaurantContext>()
.AddDefaultTokenProviders();

// ======================================================
// JWT SETTINGS
// ======================================================

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

var secretKey =
    jwtSettings["Secret"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? "RestaurantManagementSecretKey12345";

var issuer =
    jwtSettings["Issuer"]
    ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? "RestaurantManagementAPI";

var audience =
    jwtSettings["Audience"]
    ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? "RestaurantManagementClient";

// ======================================================
// JWT AUTHENTICATION
// ======================================================

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;

    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = issuer,
            ValidAudience = audience,

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)
                )
        };
});

// ======================================================
// AUTHORIZATION
// ======================================================

builder.Services.AddAuthorization();

// ======================================================
// CORS
// ======================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var frontendUrl =
            Environment.GetEnvironmentVariable("FRONTEND_URL");

        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174"
            );

        // Add Render frontend URL if provided
        if (!string.IsNullOrWhiteSpace(frontendUrl))
        {
            policy.WithOrigins(frontendUrl);
        }

        policy
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ======================================================
// CONTROLLERS + JSON
// ======================================================

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

// ======================================================
// SWAGGER
// ======================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ======================================================
// BUILD APPLICATION
// ======================================================

var app = builder.Build();

// Apply database migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RestaurantContext>();
    db.Database.Migrate();
}
// ======================================================
// SWAGGER
// ======================================================

// Enable Swagger in both Development and Production.
// This makes it easier to test the deployed API.
app.UseSwagger();
app.UseSwaggerUI();

// ======================================================
// MIDDLEWARE
// ======================================================

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ======================================================
// START APPLICATION
// ======================================================

app.Run();