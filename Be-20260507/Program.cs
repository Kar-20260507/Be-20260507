using Be_20260507.configuration.common;
using Scalar.AspNetCore;
using Serilog;

Console.TreatControlCAsInput = true; // 警用控制输入 Ctrl+C关闭程序

var builder = WebApplication.CreateBuilder(args);

builder.addCommonConfiguration(); // 本项目注入配置模块

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