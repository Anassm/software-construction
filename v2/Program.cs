using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.Security.Claims;
using v2.Infrastructure.Data;
using v2.core.Interfaces;
using v2.infrastructure.Services;
using v2.Core.Interfaces;
using v2.Infrastructure.Services;
using v2.Core.DTOs;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];
var ConnectionStrings = builder.Configuration.GetSection("ConnectionStrings");
var defaultConnection = ConnectionStrings["DefaultConnection"];

var isTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";

if (isTest)
{
    builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(
        options => options.UseSqlite($"Data Source={defaultConnection}"));

}

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var jti = context.Principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (TokenBlacklist.Contains(jti))
            {
                context.Fail("Token has been revoked.");
            }

            return Task.CompletedTask;
        }
    };
});



builder.Services.AddAuthorization();

builder.Services.AddScoped<IReservation, ReservationService>();
builder.Services.AddScoped<IVehicles, VehicleService>();
builder.Services.AddScoped<IParkingLots, ParkingLotService>();
builder.Services.AddScoped<IPayment, PaymentService>();
builder.Services.AddScoped<IBilling, BillingService>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IDiscounts, DiscountService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok("Healthy"));
app.Run();


