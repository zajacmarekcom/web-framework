namespace WebFramework.Host.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PathAttribute(string path) : Attribute
{
    public string Path { get; } = path;
    public HttpMethod HttpMethod { get; protected init; } = HttpMethod.Get;
}

public class GetPathAttribute : PathAttribute
{
    public GetPathAttribute(string path) : base(path)
    {
        HttpMethod = HttpMethod.Get;
    }
}

public class PostPathAttribute : PathAttribute
{
    public PostPathAttribute(string path) : base(path)
    {
        HttpMethod = HttpMethod.Post;
    }
}
