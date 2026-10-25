using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.1.2:与 <see cref="DayshiftReconcileTests" /> 属于不同集合,默认并行模式下同时执行。
/// 两个测试类的 <c>output</c> 各写各的行,用来观察"输出绑定到测试,不会串台"。
/// </summary>
public class NightshiftReconcileTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Reconcile_Nightshift_LogsEachStep()
    {
        output.WriteLine("[nightshift] 载入夜班发票");
        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        await Task.Delay(120, TestContext.Current.CancellationToken);
        output.WriteLine($"[nightshift] 第 1 步完成:差异 {mismatched.Count} 行");

        await Task.Delay(120, TestContext.Current.CancellationToken);
        output.WriteLine("[nightshift] 第 2 步完成:夜班结束");

        Assert.Equal(2, mismatched.Count);
    }
}

/// <summary>9.1.2:另一个集合里的测试,写的是自己的标记行。</summary>
public class DayshiftReconcileTests(ITestOutputHelper output)
{
    [Fact]
    public async Task Reconcile_Dayshift_LogsEachStep()
    {
        output.WriteLine("[dayshift] 载入白班发票");
        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        await Task.Delay(120, TestContext.Current.CancellationToken);
        output.WriteLine($"[dayshift] 第 1 步完成:差异 {mismatched.Count} 行");

        await Task.Delay(120, TestContext.Current.CancellationToken);
        output.WriteLine("[dayshift] 第 2 步完成:白班结束");

        Assert.Equal(2, mismatched.Count);
    }
}
