using System.Net;

namespace WebFramework.Host.Framework;

public abstract class PreActionMiddleware
{
    public abstract HttpListenerContext Handle(HttpListenerContext context);
}