namespace Infrastructure;

using Domain.Ports.Infrastructure;
using Domain.Ports.Persistence;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Options;
using Persistence;

public static class InfrastructureConfiguration {
  public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection, IConfiguration configuration) {
    string connectionString = configuration.GetConnectionString("Dev");
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    dataSourceBuilder.UseVector();
    var dataSource = dataSourceBuilder.Build();
    
    serviceCollection.AddDbContext<DatabaseContext>(options => options.UseNpgsql(dataSource,
      o => {
        o.MigrationsAssembly("Infrastructure");
        o.UseVector();
      }));
    serviceCollection.Configure<AiClientOptions>(configuration.GetSection("AiClient"));
    serviceCollection.AddSingleton<IGuidGenerator, GuidGenerator>();
    serviceCollection.AddSingleton<IDateTimeProvider,DateTimeProvider>();
    serviceCollection.AddSingleton<ISchemaProvider,SchemaProvider>();

    serviceCollection.AddScoped<IGameSystemRepository,GameSystemRepository>();
    serviceCollection.AddScoped<IUnitOfWork,UnitOfWork>();
    serviceCollection.AddScoped<IAiClient, AiClient>();

    return serviceCollection;
  }
}
