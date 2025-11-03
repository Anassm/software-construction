using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ChefServe.Infrastructure.Data;
using v2.core.Interfaces;
using v2.infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

#if TEST
{
    builder.Services.AddDbContext<ApplicationDbContext>(
        options =>options.UseInMemoryDatabase("TestDatabase"));

    // builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    //     .AddEntityFrameworkStores<ApplicationDbContext>()
    //     .AddDefaultTokenProviders();
}


#else
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlite("Data Source=infrastructure/data/app.db"));
#endif



builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>() 
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddScoped<IReservation, ReservationService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

#if TEST
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();

        if (!roleManager.RoleExistsAsync("Admin").Result)
            roleManager.CreateAsync(new IdentityRole("Admin")).Wait();
        if (!roleManager.RoleExistsAsync("User").Result)
            roleManager.CreateAsync(new IdentityRole("User")).Wait();

        var admin = new IdentityUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            EmailConfirmed = true
        };
        var user = new IdentityUser
        {
            UserName = "user@test.com",
            Email = "user@test.com",
            EmailConfirmed = true
        };

        if (userManager.FindByEmailAsync(admin.Email).Result == null)
        {
            userManager.CreateAsync(admin, "Admin123!").Wait();
            userManager.AddToRoleAsync(admin, "Admin").Wait();
        }

        if (userManager.FindByEmailAsync(user.Email).Result == null)
        {
            userManager.CreateAsync(user, "User123!").Wait();
            userManager.AddToRoleAsync(user, "User").Wait();
        }

        Console.WriteLine("✅ Seeded ASP.NET Identity users (Admin/User) into InMemoryDatabase.");
    }
}
#endif

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
app.MapIdentityApi<IdentityUser>();
app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager,
    [FromBody] object empty) =>
{
    if (empty != null)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }
    return Results.Unauthorized();
})
.WithOpenApi()
.RequireAuthorization();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
//     options => options.UseInMemoryDatabase("AppDb"));
// builder.Services.AddAuthorization();