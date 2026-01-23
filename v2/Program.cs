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

// --- Database ---
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

// --- Identity ---
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- JWT Authentication ---
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
                // If token is revoked, fail authentication
                context.Fail("Token has been revoked.");
            }

            return Task.CompletedTask;
        }
    };
});



// --- Authorization ---
builder.Services.AddAuthorization();

// --- Services ---
builder.Services.AddScoped<IReservation, ReservationService>();
builder.Services.AddScoped<IVehicles, VehicleService>();
builder.Services.AddScoped<IParkingLots, ParkingLotService>();
builder.Services.AddScoped<IPayment, PaymentService>();
builder.Services.AddScoped<IBilling, BillingService>();


// --- Controllers & Swagger ---
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
// --- App ---
var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// --- Login endpoint returning JWT ---
// app.MapPost("/login", async (UserManager<IdentityUser> userManager,
//                               [FromBody] LoginDto login) =>
// {
//     var user = await userManager.FindByNameAsync(login.username);
//     if (user == null) return Results.Unauthorized();

//     var passwordValid = await userManager.CheckPasswordAsync(user, login.password);
//     if (!passwordValid) return Results.Unauthorized();

//     var claims = new[]
//     {
//         new Claim(ClaimTypes.NameIdentifier, user.Id),
//         new Claim(ClaimTypes.Name, user.UserName), 
//     };

//     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
//     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//     var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
//         issuer: jwtIssuer,
//         audience: jwtAudience,
//         claims: claims,
//         expires: DateTime.UtcNow.AddHours(1),
//         signingCredentials: creds
//     );

//     return Results.Ok(new
//     {
//         tokentype = "Bearer",
//         accesstoken = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token)
//     });
// });

// --- Map controllers ---
app.MapControllers();

app.MapGet("/health", () => Results.Ok("Healthy"));
app.Run();


