using System.Net;

namespace WebFramework.Host.Framework.Middleware;

public class LogRequestMiddleware : PreActionMiddleware
{
    public override HttpListenerContext Handle(HttpListenerContext context)
    {
        Console.WriteLine($"{context.Request.HttpMethod}: {context.Request.Url}");
        Console.WriteLine($"Body: {new StreamReader(context.Request.InputStream).ReadToEnd()}");
        
        return context;
    }
}