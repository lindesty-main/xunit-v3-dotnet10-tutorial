using System.Diagnostics;

namespace Ch09.Output.Capture.Production;

/// <summary>一条账实比对记录。</summary>
public sealed record AuditRow(string Id, decimal Booked, decimal Statement);

/// <summary>
/// 老代码里最常见的日志形态:直接写 <see cref="Console"/>(标准输出/标准错误)和
/// <see cref="Debug"/> / <see cref="Trace"/>。它不知道自己跑在测试里,也不接受任何日志器参数。
/// </summary>
public static class LegacyAuditWriter
{
    public static IReadOnlyList<AuditRow> FindUnbalancedRows(IReadOnlyList<AuditRow> rows)
    {
        Console.WriteLine($"[audit] 开始审计:{rows.Count} 行");
        Console.Error.WriteLine("[audit] 注意:标准错误也走 Console 捕获这条路");
        Debug.WriteLine("[audit] Debug 写入:Release 编译下这一行被编译器剔除");
        Trace.WriteLine("[audit] Trace 写入:任何编译配置下都存在");

        var unbalanced = rows.Where(row => row.Booked != row.Statement).ToList();

        Console.WriteLine($"[audit] 审计结束:不平 {unbalanced.Count} 行");

        return unbalanced;
    }
}
