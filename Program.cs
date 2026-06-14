using Microsoft.EntityFrameworkCore;
using project_web2.Data;
var builder = WebApplication.CreateBuilder(args);

AppDomain.CurrentDomain.SetData("DataDirectory",
    Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
builder.Services.AddDbContext<project_web2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("project_web2Context") ?? throw new InvalidOperationException("Connection string 'project_web2Context' not found.")));


// Add Session
builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(15); });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Global exception handler — active in all environments
app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();

// Session must be before routing so it is available to all middleware
app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=orders}/{action=CatalogueBuy}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<project_web2Context>();
    DbInitializer.Initialize(context);
}

app.Run();
