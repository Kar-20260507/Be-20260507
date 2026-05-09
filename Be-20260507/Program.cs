using Be_20260507.configuration.log;
using Be_20260507.model.configuration;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<AppConfiguration>()
    .BindConfiguration(nameof(AppConfiguration))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services);

        var appConfiguration = services.GetRequiredService<IOptions<AppConfiguration>>().Value;

        configuration.handleBaseConfiguration();

        configuration.handleFileConfiguration($"/home/logs/{appConfiguration.Name}/info-.log",
            LogEventLevel.Information);

        configuration.handleFileConfiguration($"/home/logs/{appConfiguration.Name}/error-.log",
            LogEventLevel.Error);
    }
);

try
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.MapScalarApiReference();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "程序启动失败");
}
finally
{
    Log.CloseAndFlush();
}