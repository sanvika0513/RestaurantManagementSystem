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
// API URLs
// ======================================================

builder.WebHost.UseUrls(
    "http://127.0.0.1:5000",
    "http://localhost:5168"
);

// ======================================================
// DATABASE
// ======================================================

var connectionString =
    builder.Configuration.GetConnectionString("RestaurantDatabase")
    ?? "server=127.0.0.1;port=3307;database=restaurant_management;user=root;password=1234;";

var serverVersion = ServerVersion.Parse("8.0.33-mysql");

builder.Services.AddDbContext<RestaurantContext>(options =>
    options.UseMySql(
        connectionString,
        serverVersion,
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure();
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
    ?? "RestaurantManagementSecretKey12345";

var issuer =
    jwtSettings["Issuer"]
    ?? "RestaurantManagementAPI";

var audience =
    jwtSettings["Audience"]
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
    options.RequireHttpsMetadata = false;
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
    options.AddPolicy("AllowReactLocalhost", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174"
            )
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

// ======================================================
// SWAGGER
// ======================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ======================================================
// MIDDLEWARE
// ======================================================

// React frontend can communicate with the API
app.UseCors("AllowReactLocalhost");

// We are using HTTP locally, so don't redirect to HTTPS.
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ======================================================
// START APPLICATION
// ======================================================

app.Run();