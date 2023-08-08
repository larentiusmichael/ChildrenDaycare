using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ChildrenDaycare.Data;
using ChildrenDaycare.Areas.Identity.Data;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ChildrenDaycareContextConnection") ?? throw new InvalidOperationException("Connection string 'ChildrenDaycareContextConnection' not found.");

builder.Services.AddDbContext<ChildrenDaycareContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<ChildrenDaycareUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>().AddEntityFrameworkStores<ChildrenDaycareContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Toast
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 3;
    config.IsDismissable = true;
    config.Position = NotyfPosition.TopCenter;
});

builder.Services.AddHttpClient();

var app = builder.Build();

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
app.UseAuthentication();    //direct login page
app.UseAuthorization();     //check permission

app.UseNotyf();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
