using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebFramework.Host.Framework;
using WebFramework.Host.Framework.Extensions;
using WebFramework.Host.Framework.Middleware;
using WebFramework.Host.Persistence;

var app = WebFrameworkBuilder.CreateApplication();

app.Services.AddControllers();
app.Services.AddDbContext<TestDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
app.UseMiddleware<LogRequestMiddleware>();

app.Run();