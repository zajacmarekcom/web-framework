namespace WebFramework.Host.Utils;

public class WebResult
{
    public WebResult(object? data)
    {
        Data = data;
    }

    public object? Data { get; set; }
}