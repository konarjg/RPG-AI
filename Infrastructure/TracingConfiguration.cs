namespace Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Options;

public static class TracingConfiguration {
  public static IServiceCollection AddTracing(this IServiceCollection services, TracingOptions options) {
    if (string.IsNullOrWhiteSpace(options.PublicKey) || string.IsNullOrWhiteSpace(options.SecretKey)) {
      return services;
    }

    services.AddOpenTelemetry()
      .WithTracing(tracerProviderBuilder => {
        tracerProviderBuilder
          .AddSource("RpgAI")
          .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
              .AddService(serviceName: "RpgAI", serviceVersion: "1.0.0"))
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddProcessor(new SimpleActivityExportProcessor(new OtlpTraceExporter(new OtlpExporterOptions {
              Endpoint = new Uri(options.Host + "/api/public/otel/v1/traces"),
              Protocol = OtlpExportProtocol.HttpProtobuf,
              Headers = $"Authorization=Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{options.PublicKey}:{options.SecretKey}"))}"
          })));
      });

    return services;
  }
}
