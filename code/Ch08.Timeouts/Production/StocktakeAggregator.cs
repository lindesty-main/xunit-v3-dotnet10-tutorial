namespace Ch08.Timeouts.Production;

/// <summary>CPU 密集的台账审计:没有任何 await,取消只能靠主动检查令牌。</summary>
public static class StocktakeAggregator
{
    /// <summary>做 rounds 轮全表扫描;每轮开头检查一次令牌,取消一到就抛 OperationCanceledException。</summary>
    public static int AuditLedger(
        IReadOnlyList<LedgerEntry> snapshot, int rounds, CancellationToken cancellationToken)
    {
        var discrepancies = 0;

        for (var round = 0; round < rounds; round++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var entry in snapshot)
                if (entry.Quantity < 0)
                    discrepancies++;
        }

        return discrepancies;
    }
}
