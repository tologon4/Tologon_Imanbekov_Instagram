using System.Globalization;
using lesson58.Models;
using lesson58.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options => options.ResourcesPath =  "Resources");
builder.Services.AddControllersWithViews().AddViewLocalization();
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<InstagramDb>(options => options.UseNpgsql(connection))
    .AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<InstagramDb>();
builder.Services.AddTransient<CacheService>();
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options => options.EnableForHttps =true);
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
    options.CacheProfiles.Add("Caching", new CacheProfile()
    {
        Duration = 300
    });
    options.CacheProfiles.Add("NoCaching", new CacheProfile()
    {
        Location = ResponseCacheLocation.None,
        NoStore = true
    });
});


var app = builder.Build();
app.UseResponseCompression();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var supportedCultures = new[]
{
    new CultureInfo("ru"),
    new CultureInfo("en")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();