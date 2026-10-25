using System.Runtime.InteropServices;
using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.3:诊断消息。默认不显示,`--xunit-diagnostics on`(MTP)/ `-diagnostics`(原生)才打印。
/// 夹具、orderer、测试体里都能发——它属于运行日志,不属于某个测试的输出。
/// </summary>
public class DiagnosticsTests(RunEnvironmentProbeFixture environment) : IClassFixture<RunEnvironmentProbeFixture>
{
    [Fact]
    public void Reconcile_SendsDiagnosticMessagesAroundTheWork()
    {
        TestContext.Current.SendDiagnosticMessage("对账开始:发票 {0} 行 / 台账 {1} 行", InvoiceBook.Invoices.Length, InvoiceBook.Ledger.Count);

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);

        TestContext.Current.SendDiagnosticMessage("对账结束:差异 {0} 行;运行环境:{1}", mismatched.Count, environment.Snapshot);
        Assert.Equal(2, mismatched.Count);
    }
}

/// <summary>
/// 9.3:夹具里的诊断消息。它的价值在于"测试还没开始跑就已经能看到环境信息",
/// 以及卡死时能看出流水线停在哪一步。
/// </summary>
public class RunEnvironmentProbeFixture : IAsyncLifetime
{
    /// <summary>初始化时抓一次运行环境快照,测试里可直接读。</summary>
    public string Snapshot { get; private set; } = "(unset)";

    public ValueTask InitializeAsync()
    {
        Snapshot = $"{RuntimeInformation.ProcessArchitecture}, {Environment.ProcessorCount} 处理器";

        TestContext.Current.SendDiagnosticMessage(
            "夹具初始化:机器 {0};运行时 {1};OS {2}",
            Environment.MachineName,
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription);

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
