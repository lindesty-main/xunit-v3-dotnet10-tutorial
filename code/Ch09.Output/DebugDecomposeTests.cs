using Ch09.Output.Production;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.6.5:把大测试拆开定点调试的三种写法——
/// ① 复制一份只做 Arrange,先断言"前提是不是我以为的那样";
/// ② 反过来删掉夹具,只留被测方法与最小输入;
/// ③ 异步测试不关心业务结果,只把"输入 ↔ 线程 ↔ Task 状态"记进测试输出。
/// </summary>
public class DebugDecomposeTests(ITestOutputHelper output)
{
    [Fact]
    public void Decompose_ArrangeOnly_AssertsInputs()
    {
        // 原测试(9.4 的 Reconcile_...)的副本,只保留 Arrange 段,并断言输入本身。
        // 这一步就红 ⇒ 问题出在"前提不对",与被测方法无关。
        Assert.Equal(3, InvoiceBook.Invoices.Length);
        Assert.Equal(90.00m, InvoiceBook.Ledger["INV-1002"]);
        Assert.Contains("INV-1004", InvoiceBook.Ledger);
        Assert.DoesNotContain("INV-1003", InvoiceBook.Ledger);
    }

    [Fact]
    public void Decompose_UnderTestOnly_MinimalInputs()
    {
        // 反过来:删掉全部夹具数据,只给被测方法能触发目标行为的最小输入。
        var invoices = new[]
        {
            new InvoiceRow("INV-2001", "Acme", 10m),
            new InvoiceRow("INV-2002", "Globex", 20m),
        };
        var ledger = new Dictionary<string, decimal> { ["INV-2001"] = 10m, ["INV-2002"] = 21m };

        var mismatched = InvoiceReconciler.FindMismatchedRows(invoices, ledger);

        Assert.Equal(["INV-2002"], mismatched.Select(r => r.Id));
    }

    [Fact]
    public async Task AsyncObservation_LogsInputThreadAndTaskStatus()
    {
        // 这份副本不关心业务逻辑,只观察"每条输入创建了哪个任务、在哪个线程、什么状态"。
        var inputs = new[] { "fast", "slow", "boom" };
        var tasks = inputs.Select(input => Task.Run(() =>
        {
            output.WriteLine($"  [task] input={input} thread={Environment.CurrentManagedThreadId} 开始执行");
            if (input == "slow")
            {
                Thread.Sleep(50);
            }

            if (input == "boom")
            {
                throw new InvalidOperationException("boom");
            }

            return input.Length;
        })).ToList();

        output.WriteLine($"创建任务数={tasks.Count},主线程={Environment.CurrentManagedThreadId}");
        foreach (var (input, task) in inputs.Zip(tasks))
        {
            output.WriteLine($"input={input} 创建后立刻状态={task.Status}");
        }

        var faulted = new List<string>();
        foreach (var (input, task) in inputs.Zip(tasks))
        {
            try
            {
                await task;
            }
            catch (InvalidOperationException)
            {
                faulted.Add(input);
            }

            output.WriteLine($"input={input} 最终状态={task.Status}");
        }

        Assert.Equal(["boom"], faulted);
    }
}
