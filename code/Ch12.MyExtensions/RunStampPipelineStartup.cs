using Xunit.Sdk;
using Xunit.v3;

namespace Ch12.MyExtensions;

/// <summary>
/// 管道启动:runner 在命令行解析后、任何发现/执行工作开始前调用 StartAsync,
/// 在退出前调用 StopAsync;发现-only 运行也会跑(程序集夹具只在执行期跑)。
/// 这里给每次运行盖一个"运行号"戳:写入环境变量,并经诊断消息通道报告。
/// </summary>
public sealed class RunStampPipelineStartup : ITestPipelineStartup
{
    public const string RunIdVariable = "CH12_RUN_ID";

    private IMessageSink? diagnosticSink;
    private string? runId;

    public ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        diagnosticSink = diagnosticMessageSink;
        runId = Guid.NewGuid().ToString("N")[..8];
        Environment.SetEnvironmentVariable(RunIdVariable, runId);
        diagnosticMessageSink.OnMessage(
            new DiagnosticMessage($"[Ch12 管道启动] 运行号 {runId};任何发现/执行开始前就已生效"));
        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync()
    {
        diagnosticSink?.OnMessage(
            new DiagnosticMessage($"[Ch12 管道收尾] 运行号 {runId} 结束"));
        return ValueTask.CompletedTask;
    }
}
