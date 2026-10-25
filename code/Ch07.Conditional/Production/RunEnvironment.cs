namespace Ch07.Conditional.Production;

/// <summary>
/// 运行环境判定。所有条件跳过都读这里,而不是散落在各个测试方法里:
/// 测试只表达"这件事需要什么前提",前提怎么判定只有一处可改。
/// </summary>
public static class RunEnvironment
{
    /// <summary>
    /// CI 判定。GitHub Actions、Azure Pipelines、GitLab CI、Jenkins 等都会设置 CI 环境变量,
    /// 值各不相同(true / 1 / ...),所以只判断"存在且非空"。
    /// </summary>
    public static bool IsCi => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    public static bool IsWindows => OperatingSystem.IsWindows();

    public static bool IsLinux => OperatingSystem.IsLinux();

    /// <summary>本地手工开关:值非空即视为打开(例如 CH07_RUN_EXPERIMENTAL=1)。</summary>
    public static bool SwitchOn(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value &&
        value is not ("0" or "false" or "off");
}
