using System.Reflection;
using System.Text.Json;
using Be_20260507.model.configuration.log;
using Be_20260507.model.constant.common;
using Be_20260507.model.constant.log;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Be_20260507.configuration.log;

public static class BeLogFilter
{
    private static BeLogFilterConfiguration beLogFilterConfiguration = new();

    private static readonly Lock lockObj = new();

    // 文件监听对象
    private static IFileProvider fileProvider;

    private static IDisposable fileWatcher;

    private static FileInfo configFile;

    static BeLogFilter()
    {
        try
        {
            // 初始化 beLogFilterConfiguration
            initBeLogFilterConfiguration();
        }
        catch (Exception error)
        {
            Log.Error(error, "初始化 beLogFilterConfiguration报错");
        }
    }

    /// <summary>
    /// 初始化 beLogFilterConfiguration
    /// </summary>
    private static void initBeLogFilterConfiguration()
    {
        configFile = getConfigFile();

        handleConfig("初始化");

        var watchDir = configFile.DirectoryName;

        fileProvider = new PhysicalFileProvider(watchDir);

        // 监听所有变化：创建/修改/删除/重命名
        fileWatcher = fileProvider.Watch("*").RegisterChangeCallback(onFileChanged, null);

        Log.Information("配置文件监听已启动，目录：{Dir}", watchDir);
    }

    /// <summary>
    /// 文件变化触发
    /// </summary>
    private static void onFileChanged(object state)
    {
        try
        {
            // 重新监听（必须，触发后会自动取消）
            fileWatcher.Dispose();

            fileWatcher = fileProvider.Watch("*").RegisterChangeCallback(onFileChanged, null);

            // 判断变化类型
            var type = configFile.Exists ? "修改/创建" : "删除";

            handleConfig(type);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "文件变化处理失败");
        }
    }

    /// <summary>
    /// 获取：文件对象
    /// </summary>
    private static FileInfo getConfigFile()
    {
        string filePath = Path.Combine("/home/conf", "base.yml");

        FileInfo file = new FileInfo(filePath);

        if (file.Exists)
        {
            return file;
        }

        string userDir = AppContext.BaseDirectory;

        var projectRoot = Directory.GetParent(userDir)!.Parent!.Parent!.Parent!.Parent!.FullName;

        string localPath = Path.Combine(projectRoot, "conf", "base.yml");

        FileInfo localFile = new FileInfo(localPath);

        if (localFile.Exists)
        {
            return localFile;
        }

        return file;
    }

    /// <summary>
    /// 处理
    /// </summary>
    private static void handleConfig(string type)
    {
        lock (lockObj)
        {
            try
            {
                string yamlContent;

                if (configFile.Exists)
                {
                    yamlContent = File.ReadAllText(configFile.FullName);
                }
                else
                {
                    yamlContent = readEmbeddedYaml();
                    writeToFile(yamlContent);
                }

                // YAML 反序列化
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .WithTypeMapping<ISet<string>, HashSet<string>>()
                    .Build();

                var newConfig = deserializer.Deserialize<BeLogFilterConfiguration>(yamlContent);

                // 复制属性到全局单例
                updateProperties(newConfig, beLogFilterConfiguration);

                // 日志输出
                Log.Information("BeLogFilter【{Type}】配置已更新：{Config}", type,
                    JsonSerializer.Serialize(beLogFilterConfiguration));
            }
            catch (Exception error)
            {
                Log.Error(error, "BeLogFilter【{Type}】配置处理失败", type);
            }
        }
    }

    /// <summary>
    /// 读取内嵌资源 base.yml
    /// </summary>
    private static string readEmbeddedYaml()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var resourceName = $"{BeCommonConstant.DIR_NAME_UNDERLINE}.resources.base.yml";

        using var stream = assembly.GetManifestResourceStream(resourceName);

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    /// <summary>
    /// 写入内嵌配置到目标文件
    /// </summary>
    private static void writeToFile(string content)
    {
        var dir = configFile.DirectoryName;

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(configFile.FullName, content);
    }

    /// <summary>
    /// 对象属性复制
    /// </summary>
    private static void updateProperties(BeLogFilterConfiguration source, BeLogFilterConfiguration target)
    {
        var properties = typeof(BeLogFilterConfiguration).GetProperties();

        foreach (var prop in properties)
        {
            if (prop.CanWrite)
            {
                var value = prop.GetValue(source);

                prop.SetValue(target, value);
            }
        }
    }

    /// <summary>
    /// 备注：打印日志会影响 tps：2600 -> 2000
    /// 
    /// 默认不打印 be开头的日志，需要配置才会打印
    /// 
    /// 配置 normal之后，默认打印不以 be开头的日志，需要配置才不会打印
    /// </summary>
    public static bool decide(this LogEvent logEvent)
    {
        try
        {
            if (logEvent.Level >= LogEventLevel.Error)
            {
                return true; // 同意
            }

            logEvent.Properties.TryGetValue("SourceContext", out var sourceContext);

            var scalarValue = sourceContext as ScalarValue;

            if (scalarValue == null || scalarValue.Value == null)
            {
                return true;
            }

            var logName = scalarValue.Value.ToString();

            if (string.IsNullOrEmpty(logName))
            {
                return true;
            }

            if (beLogFilterConfiguration.logNameSet != null && beLogFilterConfiguration.logNameSet.Any())
            {
                if (beLogFilterConfiguration.logNameSet.Contains(logName))
                {
                    return true; // 同意
                }

                if (beLogFilterConfiguration.logNameSet.Contains(LogNameConstant.NORMAL) &&
                    !logName.StartsWith(LogNameConstant.PRE_BE))
                {
                    if (beLogFilterConfiguration.notLogNameSet != null &&
                        beLogFilterConfiguration.notLogNameSet.Contains(logName))
                    {
                        return false; // 不打印
                    }

                    return true; // 同意
                }

                return false; // 不打印
            }

            if (logName.StartsWith(LogNameConstant.PRE_BE))
            {
                return false; // 不打印
            }

            return true; // 同意
        }
        catch (Exception e)
        {
            Log.Error(e, "BeLogFilter处理日志报错");
            return true;
        }
    }
}