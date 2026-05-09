using System.Text;
using Serilog;
using Serilog.Events;

namespace Be_20260507.configuration.log;

public static class LogConfiguration
{
    private const string OUTPUT_TEMPLATE =
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{ThreadId}:{SourceContext}:{LineNumber}] {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration handleBaseConfiguration(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: OUTPUT_TEMPLATE
            );
    }

    public static LoggerConfiguration handleFileConfiguration(this LoggerConfiguration loggerConfiguration, string path,
        LogEventLevel restrictedToMinimumLevel)
    {
        return loggerConfiguration.WriteTo.File(
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