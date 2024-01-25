namespace WebFramework.Host.Utils.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PathAttribute(string path) : Attribute
{
    public string Path { get; set; } = path;
}

public class GetPathAttribute(string path) : PathAttribute(path);
public class PostPathAttribute(string path) : PathAttribute(path);
