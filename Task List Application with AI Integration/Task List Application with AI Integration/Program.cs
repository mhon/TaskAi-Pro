using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using System.Text;
using TaskAi.Api.Data;
using TaskAi.Api.Models;
using TaskAi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// Gemini AI-Powered Features
// ---------------------------------------------------------

builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<AiService>();

// ---------------------------------------------------------
// 1. SERVICES CONFIGURATION
// ---------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Database setup
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        var databaseUrl = builder.Configuration["DATABASE_URL"]
            ?? throw new InvalidOperationException("DATABASE_URL or ConnectionStrings:DefaultConnection must be configured.");
        var databaseUri = new Uri(databaseUrl);
        var databaseName = Uri.UnescapeDataString(databaseUri.AbsolutePath.TrimStart('/'));
        connectionString = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
            Database = databaseName,
            Username = Uri.UnescapeDataString(databaseUri.UserInfo.Split(':')[0]),
            Password = Uri.UnescapeDataString(databaseUri.UserInfo.Split(':').Skip(1).FirstOrDefault() ?? ""),
            SslMode = Npgsql.SslMode.Disable
        }.ConnectionString;
    }

    options.UseNpgsql(connectionString);
});

// Bind JwtSettings from appsettings.json
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    jwtSettings.SecretKey = builder.Configuration["SESSION_SECRET"] ?? string.Empty;

if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
{
    throw new InvalidOperationException("Configure a JWT secret of at least 32 bytes using JwtSettings:SecretKey or SESSION_SECRET.");
}

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.GetType().Name}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("SUCCESS!");
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddAuthorization();

// --- Angular Policy ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---------------------------------------------------------
// 2. MIDDLEWARE PIPELINE
// ---------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- Enable the Policy ---
app.UseRouting();

app.UseCors("AngularPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();