using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.EntityFrameworkCore.SqlServer;

using TodoWebApp.Logging;
using TodoWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

#region [Process any cmd line args]
// grab "--port=1234" from the raw args
var portArg = args.FirstOrDefault(a => a.StartsWith("--port=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1];

if (!string.IsNullOrEmpty(portArg) && int.TryParse(portArg, out var port))
{
    // bind HTTP on the chosen port
    builder.WebHost.UseUrls($"http://localhost:{port}");
}
//else // fallback or could throw
//{    
//    builder.WebHost.UseUrls("http://localhost:5050");
//}
#endregion

#region [Wire-up Logging]
var logFile = Path.Combine(builder.Environment.ContentRootPath, "Logs", "WebApp.log");
Debug.WriteLine($"[INFO] Log file path is here '{logFile}'");  // D:\source\repos\ASP.NET\TodoWebApp\Logs\WebApp.log
//var logFilewww = Path.Combine(builder.Environment.WebRootPath, "Logs", "WebApp.log");
//Debug.WriteLine($"[INFO] wwwroot log file path is here '{logFilewww}'"); // D:\source\repos\ASP.NET\TodoWebApp\wwwroot\Logs\WebApp.log

builder.Logging.ClearProviders(); // clear default providers (optional, but keeps console/debug separate)

// Filter out the hosting-lifetime noise
//builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
//builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);

// (Optional) only log your own namespace at Information
builder.Logging.AddFilter("TodoWebApp", LogLevel.Information);

builder.Logging.AddConsole();     // register Microsoft's console output
builder.Logging.AddDebug();       // register Microsoft's debug output
builder.Logging.AddFile(logFile);

/* [Also can be done via "appsettings.json"]
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "None",
      "TodoWebApp": "Information"
    }
  }
}
*/
#endregion

#region [Register EF Core with InMemory DB]
// Uncomment the following line to use an in-memory database for testing purposes (needs Microsoft.EntityFrameworkCore.InMemory package)
//builder.Services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase("TodoList"));
#endregion

#region [Register EF Core with SQL Server]
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(connectionString));

// Alternatively, you can use the following line to register the DbContext with a connection string from appsettings.json
//builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration["ConnectionStrings:DefaultConnection"]));
#endregion

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this
    // for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// Middleware pipeline
app.UseStaticFiles();  // serves "wwwroot/*"
//app.UseStaticFiles(new StaticFileOptions { RequestPath = "/images" }); // serves "wwwroot/images/*"

// This is needed to serve static files from the wwwroot folder when not running from the VisualStudio IDE.
Microsoft.AspNetCore.Hosting.StaticWebAssets.StaticWebAssetsLoader
    .UseStaticWebAssets(app.Environment, builder.Configuration);

//app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=TodoItems}/{action=Index}/{id?}");

// Let's go!
app.Run();
