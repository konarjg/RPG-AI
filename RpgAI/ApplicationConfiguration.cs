namespace RpgAI;

using Application.Providers;
using Application.Providers.Interfaces;
using Application.Reporters;
using Application.Reporters.Interfaces;
using Application.Util;
using Application.Util.Interfaces;
using Domain.Factories;
using Domain.Factories.Interfaces;

public static class ApplicationConfiguration {
  public static IServiceCollection AddApplication(this IServiceCollection serviceCollection) {
    serviceCollection.AddDomain();
    serviceCollection.AddScoped<IGameSystemProvider,GameSystemProvider>();
    serviceCollection.AddScoped<IGameSystemReporter,GameSystemReporter>();
    serviceCollection.AddSingleton<ICharacterGenerationService, CharacterGenerationService>();
    serviceCollection.AddScoped<ICharacterProvider, CharacterProvider>();
    serviceCollection.AddScoped<ICharacterReporter,CharacterReporter>();
    serviceCollection.AddScoped<ICampaignProvider,CampaignProvider>();
    serviceCollection.AddScoped<ICampaignReporter, CampaignReporter>();
    serviceCollection.AddScoped<ISessionProvider,SessionProvider>();
    serviceCollection.AddScoped<ISessionReporter,SessionReporter>();

    return serviceCollection;
  }
  
  private static IServiceCollection AddDomain(this IServiceCollection serviceCollection) {
    serviceCollection.AddScoped<IGameSystemFactory,GameSystemFactory>();
    serviceCollection.AddScoped<ICampaignFactory, CampaignFactory>();
    serviceCollection.AddScoped<ICharacterFactory, CharacterFactory>();
    serviceCollection.AddScoped<ISessionFactory,SessionFactory>();
    
    return serviceCollection;
  }
}
