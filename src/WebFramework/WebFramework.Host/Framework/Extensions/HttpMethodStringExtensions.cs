namespace WebFramework.Host.Framework.Extensions;

public static class HttpMethodStringExtensions
{
    public static HttpMethod ToHttpMethod(this string method)
    {
        return method.ToUpper() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH" => HttpMethod.Patch,
            _ => throw new NotSupportedException()
        };
    }
}