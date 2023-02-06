using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;

namespace WebApplication3.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> opt): base(opt)
        {
            
        }

        public DbSet<Platform> Platforms { get; set; }
    }
}