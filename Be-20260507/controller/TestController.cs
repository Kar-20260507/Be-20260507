using Microsoft.AspNetCore.Mvc;

namespace Be_20260507.controller;

[ApiController]
[Route("[controller]")]
public class TestController(ILogger<TestController> log) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        string helloWorld = "hello world";

        log.LogInformation($"Hello {helloWorld}!");

        try
        {
            int a = 0;
            int b = 1 / a;
        }
        catch (Exception e)
        {
            log.LogError(e, "计算发生异常");
        }

        return Ok(helloWorld);
    }
}