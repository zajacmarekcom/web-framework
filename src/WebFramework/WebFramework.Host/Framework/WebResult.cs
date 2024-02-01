using WebFramework.Host.Framework;

namespace WebFramework.Host.Utils;

public class WebResult
{
    public WebResult(object? data, StatusCodes? statusCode = null)
    {
        Data = data;
        if (statusCode != null)
            StatusCode = statusCode.Value;
    }

    public object? Data { get; set; }
    public StatusCodes StatusCode { get; init; } = StatusCodes.Ok;
}