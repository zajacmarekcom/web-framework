using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace WebFramework.Host.Framework;

public class WebApplication
{
    private ServiceProvider? _serviceProvider;
    private readonly List<PreActionMiddleware> _preActionMiddlewares = [];

    public ServiceCollection Services { get; } = new();

    public void UseMiddleware<T>() where T : PreActionMiddleware
    {
        var middleware = Activator.CreateInstance<T>();
        _preActionMiddlewares.Add(middleware);
    }

    public void Run(string baseUrl = "http://localhost:5000/")
    {
        _serviceProvider = Services.BuildServiceProvider();
        
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add(baseUrl);
        httpListener.Start();

        while (true)
        {
            var context = httpListener.GetContext();
            context = RunMiddlewares(context);
            ThreadPool.QueueUserWorkItem(obj =>
            {
                var handler = new HttpRequestHandler(_serviceProvider);
                handler.Handle(context);
            });
        }
        // ReSharper disable once FunctionNeverReturns
    }
    
    private HttpListenerContext RunMiddlewares(HttpListenerContext context)
    {
        foreach (var middleware in _preActionMiddlewares)
        {
            context = middleware.Handle(context);
        }

        return context;
    }
}