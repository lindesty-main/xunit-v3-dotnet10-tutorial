using Ch09.Output.Production;
using Xunit.Sdk;
using Xunit.v3;

namespace Ch09.Output.Tests;

/// <summary>
/// 9.5:早期阶段的读写探针。类排序器在每个测试集合开始之前被调用一次;
/// 这里不改变顺序(原样返回),存在的唯一目的就是借这个时机读写一次 <c>KeyValueStorage</c>,
/// 并用诊断消息把"此刻能看见哪些键"打出来。
/// </summary>
public class RunMarkerOrderer : ITestClassOrderer
{
    public IReadOnlyCollection<TTestClass?> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass?> testClasses)
        where TTestClass : ITestClass
    {
        var storage = TestContext.Current.KeyValueStorage;

        TestContext.Current.SendDiagnosticMessage(
            "[kvs] 排序器被调用:阶段={0},此刻可见的键:{1}",
            TestContext.Current.PipelineStage,
            storage.Count == 0 ? "(无)" : string.Join(", ", storage.Keys.OrderBy(key => key)));

        storage.TryAdd("run.startedUtc", DateTimeOffset.UtcNow.ToString("O"));

        return testClasses;
    }
}

/// <summary>9.5:夹具在测试开始之前写入批次号;类型安全的夹具通道与弱类型的 storage 通道并存。</summary>
public class NightBatchContext : IAsyncLifetime
{
    public string BatchId { get; } = "nightly-2026-10-22";

    public ValueTask InitializeAsync()
    {
        TestContext.Current.KeyValueStorage["batch.id"] = BatchId;

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

public class StorageWriterTests(NightBatchContext batch, ITestOutputHelper output) : IClassFixture<NightBatchContext>
{
    [Fact]
    public void Reconcile_PublishesRunMarkersThroughStorage()
    {
        var storage = TestContext.Current.KeyValueStorage;

        Assert.True(storage.TryGetValue("run.startedUtc", out var startedUtc), "排序器没有写入 run.startedUtc");
        output.WriteLine("排序器写入的 run.startedUtc = {0}", startedUtc ?? "(null)");
        Assert.Equal(batch.BatchId, storage["batch.id"]);

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        storage["reconcile.mismatchCount"] = mismatched.Count;
        output.WriteLine("本测试写回 reconcile.mismatchCount = {0}", mismatched.Count);

        Assert.Equal(2, mismatched.Count);
    }
}

/// <summary>9.5:读取方——另一个测试类里的测试,把当时可见的键全部打出来。</summary>
public class StorageReaderTests(ITestOutputHelper output)
{
    [Fact]
    public void Report_ReadsMarkersWrittenByEarlierStages()
    {
        var storage = TestContext.Current.KeyValueStorage;

        Assert.True(storage.TryGetValue("run.startedUtc", out var startedUtc), "排序器没有写入 run.startedUtc");
        output.WriteLine("run.startedUtc = {0}", startedUtc ?? "(null)");

        foreach (var (key, value) in storage.OrderBy(entry => entry.Key))
            output.WriteLine("storage[{0}] = {1}", key, value ?? "(null)");

        var mismatched = InvoiceReconciler.FindMismatchedRows(InvoiceBook.Invoices, InvoiceBook.Ledger);
        Assert.Equal(2, mismatched.Count);
    }
}
