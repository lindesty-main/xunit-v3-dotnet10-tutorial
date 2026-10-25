using System.Diagnostics;
using System.Text;

namespace Ch09.Output.Production;

/// <summary>
/// 把发票行与台账金额逐行比对。<paramref name="verboseLogging" /> 为 true 时,
/// 过程日志直接写 <see cref="Console"/> 与 <see cref="Trace"/>/<see cref="Debug"/>——
/// 这是"现有代码自带日志"的典型形态,第 9 章用它演示捕获行为。
/// </summary>
public static class InvoiceReconciler
{
    public static IReadOnlyList<InvoiceRow> FindMismatchedRows(
        IReadOnlyList<InvoiceRow> invoices,
        IReadOnlyDictionary<string, decimal> ledgerAmounts,
        bool verboseLogging = false)
    {
        if (verboseLogging)
        {
            Console.WriteLine($"[reconciler] 开始对账:发票 {invoices.Count} 行,台账 {ledgerAmounts.Count} 行");
            Trace.WriteLine($"[reconciler/trace] Trace 写入:任何配置下都存在({DateTimeOffset.Now:HH:mm:ss})");
            Debug.WriteLine("[reconciler/debug] Debug 写入:只有 Debug 编译下存在");
        }

        var mismatched = invoices
            .Where(row => !ledgerAmounts.TryGetValue(row.Id, out var expected) || expected != row.Amount)
            .ToList();

        if (verboseLogging)
        {
            foreach (var row in mismatched)
                Console.Error.WriteLine($"[reconciler] 差异 {row.Id}:发票 {row.Amount:0.00},台账 {(ledgerAmounts.TryGetValue(row.Id, out var expected) ? expected : 0m):0.00}");

            Console.WriteLine($"[reconciler] 对账结束:差异 {mismatched.Count} 行");
        }

        return mismatched;
    }

    /// <summary>生成一份纯文本差异报告;第 9 章把它作为附件挂到测试结果上。</summary>
    public static string BuildDiffReport(
        IReadOnlyList<InvoiceRow> invoices,
        IReadOnlyDictionary<string, decimal> ledgerAmounts)
    {
        var builder = new StringBuilder();
        builder.AppendLine("nightly reconciliation diff");
        builder.AppendLine($"generated: {DateTimeOffset.Now:O}");
        builder.AppendLine($"invoices: {invoices.Count}, ledger rows: {ledgerAmounts.Count}");
        builder.AppendLine(new string('-', 40));

        foreach (var row in invoices)
        {
            var hasLedgerRow = ledgerAmounts.TryGetValue(row.Id, out var ledgerAmount);
            var matched = hasLedgerRow && ledgerAmount == row.Amount;
            builder.AppendLine(matched
                ? $"OK   {row.Id} {row.Vendor} {row.Amount:0.00}"
                : $"DIFF {row.Id} {row.Vendor} invoice={row.Amount:0.00} ledger={(hasLedgerRow ? ledgerAmount : 0m):0.00}");
        }

        return builder.ToString();
    }
}
