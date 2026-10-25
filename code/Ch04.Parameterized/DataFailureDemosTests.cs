using Ch04.Parameterized.Production;

namespace Ch04.Parameterized.Tests;

/// <summary>故意失败/跳过的演示:去掉 Skip 才能复现正文里的输出(见 4.5、4.6)。</summary>
public class DataFailureDemosTests
{
    private readonly ShippingEstimator estimator = new();

    // 4.5:未类型化的行,参数顺序写反只能到运行时才炸
    public static IEnumerable<object?[]> SwappedUntypedRows =>
    [
        new object?[] { 12.00m, 500 },
    ];

    [Theory(Skip = "演示:去掉 Skip 后,这行数据要到运行时才失败(见 4.5)")]
    [MemberData(nameof(SwappedUntypedRows))]
    public void Estimate_SwappedUntypedRow_FailsOnlyAtRuntime(int weightGrams, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, ShippingZone.Domestic, express: false));

    // 4.6:单行超时
    public static IEnumerable<TheoryDataRow<string>> SlowDependencyRows() =>
    [
        new TheoryDataRow<string>("SF123456789012")
            .WithTestDisplayName("慢依赖演示(这一行超时 100ms)")
            .WithTimeout(100),
    ];

    [Theory(Skip = "演示:去掉 Skip 后,这一行会在 100ms 后被判定超时(见 4.6)")]
    [MemberData(nameof(SlowDependencyRows))]
    public async Task Parse_SlowDependency_HitsRowTimeout(string input)
    {
        await Task.Delay(500, TestContext.Current.CancellationToken);

        Assert.Equal(input, TrackingNumberParser.Parse(input).ToString());
    }
}
