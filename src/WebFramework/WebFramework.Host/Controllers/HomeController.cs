using WebFramework.Host.Framework;
using WebFramework.Host.Framework.Attributes;
using WebFramework.Host.Persistence;
using WebFramework.Host.Utils;

namespace WebFramework.Host.Controllers;

public class HomeController(TestDbContext dbContext) : Controller
{
    [GetPath("/api/test/{id}")]
    public WebResult Test(int id)
    {
        var user = dbContext.Users.FirstOrDefault(x => x.Id == id);
        
        return user == null ? Responses.NotFound() : Responses.Ok(user);
    }
    
    [GetPath(("/api/test"))]
    public WebResult TestList()
    {
        var users = dbContext.Users.ToList();

        return Responses.Ok(users);
    }
    
    [PostPath("/api/test")]
    public WebResult TestPost(DataDto dto)
    {
        var entity = new UserEntity()
        {
            Username = dto.Name,
            Age = dto.Age
        };
        dbContext.Users.Add(entity);
        dbContext.SaveChanges();
        
        return Responses.Ok();
    }
}

public class DataDto
{
    public string Name { get; set; }
    public int Age { get; set; }
}