using Be_20260507.configuration.log;
using Be_20260507.model.configuration.common;

namespace Be_20260507.configuration.common;

public static class CommonConfiguration
{
    public static void addCommonConfiguration(this WebApplicationBuilder builder)
    {
        builder.addOptions<AppConfiguration>(); // 应用配置类

        builder.handleFullLogConfiguration(); // 日志模块
    }

    public static void addOptions<TOptions>(this WebApplicationBuilder builder) where TOptions : class
    {
        builder.Services.AddOptions<TOptions>()
            .BindConfiguration(nameof(TOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}