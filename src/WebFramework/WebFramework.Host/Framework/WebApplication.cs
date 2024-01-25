using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WebFramework.Host.Utils;
using WebFramework.Host.Utils.Attributes;

namespace WebFramework.Host.Framework;

public class WebApplication
{
    private ServiceProvider _serviceProvider;

    private HttpListener _listener;

    private IEnumerable<Controller> _controllers;
    
    private List<PreActionMiddleware> _preActionMiddlewares = new();

    public WebApplication()
    {
        this.Services = new ServiceCollection();
        Assembly.GetEntryAssembly()!
            .GetTypes()
            .Where(x => x.BaseType == typeof(Controller))
            .ToList()
            .ForEach(x => this.Services.AddTransient(typeof(Controller), x));

        _listener = new HttpListener();
    }

    public ServiceCollection Services { get; }
    
    public void UseMiddleware<T>() where T : PreActionMiddleware
    {
        var middleware = Activator.CreateInstance<T>();
        _preActionMiddlewares.Add(middleware);
    }

    public void Run()
    {
        _serviceProvider = Services.BuildServiceProvider();

        _listener.Prefixes.Add("http://localhost:5000/");
        _listener.Start();

        while (true)
        {
            var context = _listener.GetContext();

            var response = HandleRequest(context);

            response.Close();
        }
    }

    private HttpListenerResponse HandleRequest(HttpListenerContext context)
    {
        foreach (var middleware in _preActionMiddlewares)
        {
            context = middleware.Handle(context);
        }
        
        // Controller Context
        var controllers = _serviceProvider.GetServices<Controller>();

        var request = context.Request;
        var response = context.Response;
        var httpMethod = request.HttpMethod;

        Type attributeType = typeof(PathAttribute);

        switch (httpMethod)
        {
            case "GET":
                attributeType = typeof(GetPathAttribute);
                break;
            case "POST":
                attributeType = typeof(PostPathAttribute);
                break;
        }

        var controller = controllers
            .FirstOrDefault(x => x
                .GetType()
                .GetMethods()
                .Any(m => m.GetCustomAttributes(attributeType, false)
                    .Any(m => ((PathAttribute)m).Path == request.Url!.AbsolutePath)));


        if (controller is null)
        {
            response.StatusCode = 404;
            response.Close();
            return response;
        }

        var method = controller
            .GetType()
            .GetMethods()
            .First(m => m.GetCustomAttributes(attributeType, false).Any());

        var parameters = request.Url!.Query;
        var queryParameters = GetQueryParameters(parameters);
        var bodyParameters = GetBodyParameters(request);
        var methodParameters = GetParameters(method, queryParameters, bodyParameters);

        var result = (WebResult?)method.Invoke(controller, methodParameters);

        response.OutputStream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result!.Data)));
        response.StatusCode = 200;

        return response;
    }

    private Dictionary<string, string> GetQueryParameters(string parameters)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(parameters))
            return result;

        parameters = parameters.Replace("?", "");

        var parametersArray = parameters.Split('&');

        foreach (var parameter in parametersArray)
        {
            var keyValue = parameter.Split('=');
            result.Add(keyValue[0], keyValue[1]);
        }

        return result;
    }

    private string GetBodyParameters(HttpListenerRequest request)
    {
        var body = request.InputStream;
        var encoding = request.ContentEncoding;
        var reader = new StreamReader(body, encoding);
        var bodyString = reader.ReadToEnd();
        return bodyString;
    }

    private object?[]? GetParameters(MethodInfo method, Dictionary<string, string> parameters,
        string bodyParameters)
    {
        var result = new List<object?>();

        var methodParameters = method.GetParameters();

        if (methodParameters.Length == 0)
            return null;

        foreach (var parameter in methodParameters)
        {
            var parameterType = parameter.ParameterType;
            var parameterName = parameter.Name;

            if (parameters.TryGetValue(parameterName!, out var parameterValue))
            {
                var parameterValueObject = Convert.ChangeType(parameterValue, parameterType);
                result.Add(parameterValueObject);
            }
            else if (!string.IsNullOrEmpty(bodyParameters))
            {
                var parameterValueObject = JsonSerializer.Deserialize(bodyParameters, parameterType);
                result.Add(parameterValueObject);
            }
            else
            {
                result.Add(null);
            }
        }

        return result.ToArray();
    }
}