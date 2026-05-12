namespace Be_20260507.model.configuration.log;

public class BeLogFilterConfiguration
{
    /// <summary>
    /// 需要打印控制台日志的名称集合
    /// </summary>
    public ISet<string>? logNameSet { get; set; } = new HashSet<string>();

    /// <summary>
    /// 不需要打印控制台日志的名称集合
    /// </summary>
    public ISet<string>? notLogNameSet { get; set; } = new HashSet<string>();
}