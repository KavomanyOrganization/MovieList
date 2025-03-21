using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVC.Models;
using DotNetEnv;
using MVC.Data;
using MVC.Models;
Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<MVC.Services.MovieService>();
builder.Services.AddScoped<MVC.Services.UserService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(Env.GetString("CONNECTION_STRING")));
builder.Services.AddIdentity<User, IdentityRole>(options => 
{
    options.Password.RequireNonAlphanumeric=false;
    
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail=true;
    options.SignIn.RequireConfirmedAccount=false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
var app = builder.Build();

// Seed data
if (args.Length > 0 && args[0].ToLower() == "seeddata")
{
    // initialize the database
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            SeedData.InitializeAsync(services).Wait();
            Console.WriteLine("DataBase successfully initialize.");
            return; 
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DataBaseInitialization error: {ex.Message}");
            return; 
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movie}/{action=ViewRating}/{id?}");

app.Run();
