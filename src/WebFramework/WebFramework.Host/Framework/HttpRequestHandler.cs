using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using WebFramework.Host.Framework.Attributes;
using WebFramework.Host.Framework.Extensions;
using WebFramework.Host.Utils;

namespace WebFramework.Host.Framework;

public class HttpRequestHandler
{
    private readonly ServiceProvider _serviceProvider;
    private readonly List<PathDescriptor> _pathDescriptors = new();

    public HttpRequestHandler(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        RegisterEndpoints();
    }

    private void RegisterEndpoints()
    {
        var controllers = Assembly.GetEntryAssembly()!
            .GetTypes()
            .Where(x => x.BaseType == typeof(Controller));

        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods()
                .Where(x => x.GetCustomAttributes(typeof(PathAttribute), true).Any());

            foreach (var method in methods)
            {
                var pathAttribute = (PathAttribute)method.GetCustomAttribute(typeof(PathAttribute))!;
                var httpMethod = pathAttribute.HttpMethod;
                var path = pathAttribute.Path;

                var pathDescriptor = new PathDescriptor(path.Split('/'), httpMethod, controller, method);
                _pathDescriptors.Add(pathDescriptor);
            }
        }
    }

    public void Handle(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            var result = RunApiMethod(request);

            if (result is null)
            {
                response.StatusCode = (int)StatusCodes.NotFound;
                response.Close();
            }

            response.StatusCode = (int)result.StatusCode;
            response.OutputStream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result!.Data)));
        }
        catch (Exception e)
        {
            var webResult = Responses.InternalServerError(e.InnerException?.Message);
            response.StatusCode = (int)webResult.StatusCode;
            response.OutputStream.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(webResult.Data)));
        }

        response.Close();
    }

    private WebResult? RunApiMethod(HttpListenerRequest request)
    {
        var pathDescriptor = GetPathDescriptor(request);

        if (pathDescriptor is null)
            return null;

        var method = pathDescriptor.MethodInfo;
        var controller = _serviceProvider.GetService(pathDescriptor.ControllerType);

        var methodParameters = GetInvokeParameters(request, method, pathDescriptor);

        var result = (WebResult?)method.Invoke(controller, methodParameters);

        return result;
    }

    private object?[]? GetInvokeParameters(HttpListenerRequest request, MethodInfo method, PathDescriptor descriptor)
    {
        var queryParameters = GetQueryParameters(request);
        var routeParameters = GetRouteParameters(request, descriptor);
        var bodyParameters = GetBodyParameters(request);
        var methodParameters = GetParameters(method, queryParameters, routeParameters, bodyParameters);
        return methodParameters;
    }

    private PathDescriptor? GetPathDescriptor(HttpListenerRequest request)
    {
        var pathDescriptor = _pathDescriptors
            .FirstOrDefault(x =>
                IsMatch(x.PathElements, request.Url?.AbsolutePath.Split('/') ?? []) &&
                x.HttpMethod == request.HttpMethod.ToHttpMethod());
        return pathDescriptor;

        bool IsMatch(string[] pathElements, string[] requestPathElements)
        {
            if (pathElements.Length != requestPathElements.Length)
                return false;

            for (var i = 0; i < pathElements.Length; i++)
            {
                var pathElement = pathElements[i];
                var requestPathElement = requestPathElements[i];

                if (pathElement.StartsWith('{') && pathElement.EndsWith('}'))
                    continue;

                if (pathElement != requestPathElement)
                    return false;
            }

            return true;
        }
    }

    private Dictionary<string, string> GetQueryParameters(HttpListenerRequest request)
    {
        var parameters = request.Url!.Query;
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
        using (var sr = new StreamReader(request.InputStream, request.ContentEncoding))
        {
            return sr.ReadToEnd();
        }
    }

    private Dictionary<string, string> GetRouteParameters(HttpListenerRequest request, PathDescriptor pathDescriptor)
    {
        var result = new Dictionary<string, string>();

        var pathElements = pathDescriptor.PathElements;
        var requestPathElements = request.Url!.AbsolutePath.Split('/');

        for (var i = 0; i < pathElements.Length; i++)
        {
            var pathElement = pathElements[i];
            var requestPathElement = requestPathElements[i];

            if (pathElement.StartsWith('{') && pathElement.EndsWith('}'))
            {
                var key = pathElement.Replace("{", "").Replace("}", "");
                result.Add(key, requestPathElement);
            }
        }

        return result;
    }

    private object?[]? GetParameters(MethodInfo method, Dictionary<string, string> queryParameters,
        Dictionary<string, string> routeParameters, string bodyParameters)
    {
        var result = new List<object?>();

        var methodParameters = method.GetParameters();

        if (methodParameters.Length == 0)
            return null;

        foreach (var parameter in methodParameters)
        {
            var parameterType = parameter.ParameterType;
            var parameterName = parameter.Name;

            if (queryParameters.TryGetValue(parameterName!, out var queryParameterValue))
            {
                var parameterValueObject = Convert.ChangeType(queryParameterValue, parameterType);
                result.Add(parameterValueObject);
            }

            if (routeParameters.TryGetValue(parameterName!, out var routeParameterValue))
            {
                var parameterValueObject = Convert.ChangeType(routeParameterValue, parameterType);
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