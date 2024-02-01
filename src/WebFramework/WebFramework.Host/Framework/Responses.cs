using WebFramework.Host.Utils;

namespace WebFramework.Host.Framework;

public static class Responses
{
    public static WebResult Ok(object? value = null)
    {
        return new WebResult(value, StatusCodes.Ok);
    }
    
    public static WebResult NotFound()
    {
        return new WebResult(null, StatusCodes.NotFound);
    }
    
    public static WebResult InternalServerError(string? errorMessage = null)
    {
        return new WebResult(errorMessage, StatusCodes.InternalServerError);
    }
    
    public static WebResult BadRequest(string? errorMessage = null)
    {
        return new WebResult(errorMessage, StatusCodes.BadRequest);
    }
}