using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using System.Text;
using TaskAi.Api.Data;
using TaskAi.Api.Models;
using TaskAi.Api.Services;

// This helps see the exact reason for JWT failures in the console output
IdentityModelEventSource.ShowPII = true;

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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Bind JwtSettings from appsettings.json
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var jwtSettings = jwtSection.Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new Exception("JWT SecretKey is missing in appsettings.json");
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
        // Temporary relaxed validation to bypass "Unable to decode" ghosts
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        // Hardcoded key to match the one in AuthController.cs
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("abcdefghijklmnopqrstuvwxyzabcdef")),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("========== DETAILED AUTH FAILURE ==========");
            // This prints the Stack Trace, which tells us the exact line in the .NET library that failed
            Console.WriteLine(context.Exception.ToString());
            Console.WriteLine("===========================================");
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

builder.Services.AddControllers();

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