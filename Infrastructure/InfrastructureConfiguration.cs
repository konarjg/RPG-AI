namespace Infrastructure;

using Domain.Ports.Infrastructure;
using Domain.Ports.Persistence;
using Infrastructure;
using Infrastructure.AiClient;
using Infrastructure.AiClient.Clients;
using Infrastructure.AiClient.Clients.Interfaces;
using Infrastructure.Engine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Options;
using Persistence;

public static class InfrastructureConfiguration {
  public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection, IConfiguration configuration) {
    string connectionString = configuration.GetConnectionString("Dev");
    NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    dataSourceBuilder.UseVector();
    NpgsqlDataSource dataSource = dataSourceBuilder.Build();
    
    serviceCollection.AddDbContext<DatabaseContext>(options => options.UseNpgsql(dataSource,
      o => {
        o.MigrationsAssembly("Infrastructure");
        o.UseVector();
      }));
    serviceCollection.Configure<AiClientOptions>(configuration.GetSection("AiClient"));
    serviceCollection.AddSingleton<IGuidGenerator, GuidGenerator>();
    serviceCollection.AddSingleton<IDateTimeProvider,DateTimeProvider>();
    serviceCollection.AddSingleton<ISchemaProvider,SchemaProvider>();
    serviceCollection.AddSingleton<IRuleEngine,RoslynRuleEngine>();

    serviceCollection.AddScoped<IGameSystemRepository,GameSystemRepository>();
    serviceCollection.AddScoped<ICampaignRepository,CampaignRepository>();
    serviceCollection.AddScoped<IUnitOfWork,UnitOfWork>();
    
    serviceCollection.AddScoped<IRulebookProcessingClient,GeminiRulebookProcessingClient>();
    serviceCollection.AddScoped<IEmbeddingClient,OpenAiEmbeddingClient>();
    serviceCollection.AddScoped<ICharacterGenerationClient,OpenAiCharacterGenerationClient>();
    serviceCollection.AddScoped<IAiClient, AiClient>();

    serviceCollection.AddHttpClient("GenerativeAi")
      .AddStandardResilienceHandler();

    TracingOptions tracingOptions = configuration.GetSection("Tracing").Get<TracingOptions>() ?? new TracingOptions();
    serviceCollection.AddTracing(tracingOptions);

    return serviceCollection;
  }
}
