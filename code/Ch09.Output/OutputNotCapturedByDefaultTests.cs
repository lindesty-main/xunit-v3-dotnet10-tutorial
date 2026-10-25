using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.2 的对照组:本工程没有 `[assembly: CaptureConsole]` / `[assembly: CaptureTrace]`,
/// 所以生产代码写进 <c>Console</c> / <c>Debug</c> / <c>Trace</c> 的内容不会进入测试输出。
/// 带捕获的版本在 <c>Ch09.Output.Capture</c> 工程里。
/// </summary>
public class OutputNotCapturedByDefaultTests(ITestOutputHelper output)
{
    [Fact(Skip = "演示:去掉 Skip 观察未捕获时 Console/Trace 输出的去向(见 9.2)")]
    public void VerboseReconciler_WritesConsoleAndTrace_NotInTestOutput()
    {
        output.WriteLine("这一行来自 ITestOutputHelper:属于测试输出");

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger, verboseLogging: true);
        output.WriteLine("这一行仍然来自 ITestOutputHelper;上面/reconciler 的日志不在这里");

        Assert.Equal(2, mismatched.Count);
    }
}
