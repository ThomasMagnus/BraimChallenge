using Microsoft.EntityFrameworkCore;

namespace BraimChallenge.Context
{
    public class ApplicationContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BraimChallenge;Username=postgres;" +
                                 "Password=Hofman95");
        }
    }
}
