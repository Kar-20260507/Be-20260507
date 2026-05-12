using System.Text;
using Be_20260507.model.configuration.common;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Be_20260507.configuration.log;

public static class LogConfiguration
{
    private const string OUTPUT_TEMPLATE =
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3} {ProcessId} --- [{ThreadId,5}] {SourceContext,-50} : {Message:lj}{NewLine}{Exception}";

    public static void handleFullLogConfiguration(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);

                var appConfiguration = services.GetRequiredService<IOptions<AppConfiguration>>().Value;

                configuration.handleBaseLogConfiguration();

                configuration.handleFileLogConfiguration($"/home/logs/{appConfiguration.Name}/info-.log",
                    LogEventLevel.Information);

                configuration.handleFileLogConfiguration($"/home/logs/{appConfiguration.Name}/error-.log",
                    LogEventLevel.Error);
            }
        );
    }

    public static void handleBaseLogConfiguration(this LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Conditional(
                logEvent => BeLogFilter.decide(logEvent),
                config => config.Console(theme: AnsiConsoleTheme.Code,
                    outputTemplate: OUTPUT_TEMPLATE)
            );
    }

    public static void handleFileLogConfiguration(this LoggerConfiguration loggerConfiguration, string path,
        LogEventLevel restrictedToMinimumLevel)
    {
        loggerConfiguration.WriteTo.File(
            path: path,
            rollingInterval: RollingInterval.Day,
            fileSizeLimitBytes: 1024 * 1024 * 20,
            retainedFileCountLimit: null,
            retainedFileTimeLimit: TimeSpan.FromDays(15),
            rollOnFileSizeLimit: true,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1),
            encoding: Encoding.UTF8,
            restrictedToMinimumLevel: restrictedToMinimumLevel,
            outputTemplate: OUTPUT_TEMPLATE
        );
    }
}