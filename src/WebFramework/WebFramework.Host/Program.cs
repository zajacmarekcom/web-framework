using WebFramework.Host.Framework;
using WebFramework.Host.Framework.Middleware;

var app = WebFrameworkBuilder.CreateApplication();

app.UseMiddleware<LogRequestMiddleware>();

app.Run();