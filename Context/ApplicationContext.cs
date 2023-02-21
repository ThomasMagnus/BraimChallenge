using Microsoft.EntityFrameworkCore;

namespace BraimChallenge.Context
{
    // Подключение к базе PostgreSql
    public class ApplicationContext : DbContext
    {
        public static string connectionString = "Host=localhost;Port=5432;Database=BraimChallenge;Username=postgres;" +
                                 "Password=Hofman95";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
    }
}
