using Microsoft.EntityFrameworkCore;

namespace WebFramework.Host.Persistence;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }
    
    public DbSet<UserEntity> Users { get; set; }
}