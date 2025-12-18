namespace RpgAI;

using Application.Providers;
using Application.Reporters;
using Domain.Factories;
using Domain.Factories.Interfaces;

public static class ApplicationConfiguration {
  public static IServiceCollection AddApplication(this IServiceCollection serviceCollection) {
    serviceCollection.AddDomain();
    serviceCollection.AddScoped<IGameSystemProvider,GameSystemProvider>();
    serviceCollection.AddScoped<IGameSystemReporter,GameSystemReporter>();
    serviceCollection.AddScoped<ICharacterReporter,CharacterReporter>();
    serviceCollection.AddScoped<ICampaignReporter, CampaignReporter>();

    return serviceCollection;
  }
  
  private static IServiceCollection AddDomain(this IServiceCollection serviceCollection) {
    serviceCollection.AddScoped<IGameSystemFactory,GameSystemFactory>();
    serviceCollection.AddScoped<ICampaignFactory, CampaignFactory>();
    serviceCollection.AddScoped<ICharacterFactory, CharacterFactory>();
    return serviceCollection;
  }
}
