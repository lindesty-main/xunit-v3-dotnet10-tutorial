namespace Ch11.Ci.Production;

/// <summary>一次测试运行的计数(与 MTP 摘要里的 total/succeeded/failed/skipped 对应)。</summary>
public sealed record RunCounts(int Succeeded, int Failed, int Skipped);

/// <summary>门禁结论:过与不过,以及为什么。</summary>
public sealed record GateDecision(bool Passed, string Reason);

/// <summary>
/// 构建门禁:决定"这次 CI 构建算不算成功"。
/// 策略本身用构造参数注入,判定逻辑在任何环境都一样;
/// "CI 上更严格、本地宽松"的差异只发生在 FromEnvironment 这一个装配点。
/// </summary>
public sealed class GatePolicy
{
    public bool FailSkips { get; }
    public long MaxTotalBytes { get; }

    public GatePolicy(bool failSkips, long maxTotalBytes)
    {
        FailSkips = failSkips;
        MaxTotalBytes = maxTotalBytes;
    }

    /// <summary>
    /// 按当前环境装配:CI 上不允许任何跳过(否则"集成层没跑起来"会被绿色掩盖);
    /// 本地允许跳过。这就是第 7 章集中判定的价值——策略只写在这一个装配点。
    /// </summary>
    public static GatePolicy FromEnvironment() =>
        new(failSkips: RunEnvironment.IsCi, maxTotalBytes: 200_000_000);

    public GateDecision Evaluate(RunCounts counts, DeploymentInventory inventory)
    {
        if (counts.Failed > 0)
            return new(false, $"{counts.Failed} test(s) failed");
        if (FailSkips && counts.Skipped > 0)
            return new(false, $"{counts.Skipped} test(s) skipped, but this gate does not tolerate skips");
        if (inventory.TotalBytes > MaxTotalBytes)
            return new(false, $"artifact total {inventory.TotalBytes} bytes exceeds limit {MaxTotalBytes}");
        return new(true, "all gates passed");
    }
}
