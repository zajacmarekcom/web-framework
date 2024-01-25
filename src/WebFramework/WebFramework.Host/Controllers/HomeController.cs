using WebFramework.Host.Framework;
using WebFramework.Host.Utils;
using WebFramework.Host.Utils.Attributes;

namespace WebFramework.Host.Controllers;

public class HomeController : Controller
{
    [GetPath("/api/test")]
    public WebResult Test(string name, int age)
    {
        var dto = new DataDto()
        {
            Age = age,
            Name = name
        };
        
        return new WebResult(dto);
    }
    
    [PostPath("/api/test")]
    public WebResult TestPost(DataDto dto)
    {
        return new WebResult(dto);
    }
}

public class DataDto
{
    public string? Name { get; set; }
    public int Age { get; set; }
}