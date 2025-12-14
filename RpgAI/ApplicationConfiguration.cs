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

    return serviceCollection;
  }
  
  private static IServiceCollection AddDomain(this IServiceCollection serviceCollection) {
    serviceCollection.AddScoped<IGameSystemFactory,GameSystemFactory>();
    return serviceCollection;
  }
}
