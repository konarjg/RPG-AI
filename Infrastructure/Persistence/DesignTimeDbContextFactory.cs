using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
  public DatabaseContext CreateDbContext(string[] args)
  {
    const string designTimeConnectionString = "Host=localhost;Database=design_time_db;Username=postgres;Password=password";

    var builder = new DbContextOptionsBuilder<DatabaseContext>();
        
    builder.UseNpgsql(designTimeConnectionString, b => 
    {
      b.MigrationsAssembly("Infrastructure");
      b.UseVector(); 
    });

    return new DatabaseContext(builder.Options);
  }
}
