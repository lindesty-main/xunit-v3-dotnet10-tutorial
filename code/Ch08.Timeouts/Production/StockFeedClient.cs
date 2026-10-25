namespace Ch08.Timeouts.Production;

/// <summary>
/// 模拟库存行情源。对外方法一律接受 CancellationToken,并把它一路传到真正会等待的地方
/// (这里是 Task.Delay;真实代码里是 HttpClient / Socket)。
/// </summary>
public sealed class StockFeedClient
{
    private readonly int pageDelayMs;

    public StockFeedClient(int pageDelayMs = 400) => this.pageDelayMs = pageDelayMs;

    /// <summary>分页拉取快照:每页等待 pageDelayMs,令牌透传给 Task.Delay,取消立即生效。</summary>
    public async Task<IReadOnlyList<LedgerEntry>> FetchSnapshotAsync(
        int pages = 3, CancellationToken cancellationToken = default)
    {
        var entries = new List<LedgerEntry>();

        for (var page = 0; page < pages; page++)
        {
            await Task.Delay(pageDelayMs, cancellationToken);

            for (var sku = 0; sku < 4; sku++)
                entries.Add(new LedgerEntry($"SKU-{page}{sku}", (page + 1) * (sku + 1) * 10));
        }

        return entries;
    }

    /// <summary>
    /// 每次调用自带截止时间:把调用方的令牌与本地 CancelAfter 链接成同一个令牌,
    /// 两种取消(调用方放弃 / 本次调用超时)都会取消进行中的抓取。
    /// </summary>
    public async Task<IReadOnlyList<LedgerEntry>> FetchSnapshotWithDeadlineAsync(
        TimeSpan perCallDeadline, CancellationToken callerToken)
    {
        using var deadlineCts = CancellationTokenSource.CreateLinkedTokenSource(callerToken);
        deadlineCts.CancelAfter(perCallDeadline);

        return await FetchSnapshotAsync(3, deadlineCts.Token);
    }

    /// <summary>反例:遗留同步方法,不接受任何令牌,外部无从取消。</summary>
    public IReadOnlyList<LedgerEntry> FetchSnapshotBlocking()
    {
        var entries = new List<LedgerEntry>();

        for (var page = 0; page < 3; page++)
        {
            Thread.Sleep(pageDelayMs);

            for (var sku = 0; sku < 4; sku++)
                entries.Add(new LedgerEntry($"SKU-{page}{sku}", (page + 1) * (sku + 1) * 10));
        }

        return entries;
    }
}
