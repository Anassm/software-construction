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

var builder = WebApplication.CreateBuilder(args);

var jwtKey = "thisIsASuperSecretKeyWithAtLeast32Bytes!";
var jwtIssuer = "yourIssuer";
var jwtAudience = "yourAudience";

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
        options => options.UseSqlite("Data Source=infrastructure/data/app.db"));

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
});

// --- Authorization ---
builder.Services.AddAuthorization();

// --- Services ---
builder.Services.AddScoped<IReservation, ReservationService>();
builder.Services.AddScoped<IVehicles, VehicleService>();

// --- Controllers & Swagger ---
builder.Services.AddControllers();
builder.Services.AddScoped<IPayment, PaymentService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


