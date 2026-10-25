namespace Ch07.Conditional.Production;

/// <summary>
/// 被测代码:同一份告警在 CI 与本地走不同格式化器。
/// 两个分支各自有测试覆盖,另一个分支运行时跳过 —— 见 EnvironmentGateTests。
/// </summary>
public static class ReportFormatter
{
    public static string Select() => RunEnvironment.IsCi ? "github-actions" : "console";

    public static string Warning(string title, string details) =>
        Select() == "github-actions"
            ? $"::warning title={title}::{details}"
            : $"WARN {title}: {details}";
}
