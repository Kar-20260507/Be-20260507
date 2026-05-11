using Be_20260507.configuration.log;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.handleFullLogConfiguration(); // 日志模块

builder.Services.AddControllers();

builder.Services.AddOpenApi();

try
{
    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.MapScalarApiReference(); // 接口文档
    }

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