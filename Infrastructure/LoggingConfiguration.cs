namespace Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

public static class LoggingConfiguration {
  public static void ConfigureLogging(HostBuilderContext context, LoggerConfiguration  configuration) {
    configuration
      .ReadFrom.Configuration(context.Configuration)
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
      .WriteTo.Console();

    if (context.HostingEnvironment.IsProduction()) {
      configuration.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
    }
  }
}
