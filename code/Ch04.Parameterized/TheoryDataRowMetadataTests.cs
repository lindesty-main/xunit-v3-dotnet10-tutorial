using Ch04.Parameterized.Production;

namespace Ch04.Parameterized.Tests;

/// <summary>数据行元数据:显示名、跳过原因、超时、标签。字段名是 TestDisplayName,不是 DisplayName。</summary>
public class TheoryDataRowMetadataTests
{
    private const int ProbeTimeoutMs = 2_000;

    private readonly ShippingEstimator estimator = new();

    // 写法一:对象初始化器,属性就是数据行的元数据字段
    public static IEnumerable<TheoryDataRow<string, string>> CarrierSampleRows() =>
    [
        new TheoryDataRow<string, string>("SF123456789012", "SF")
        {
            TestDisplayName = "顺丰单号",
        },
        new TheoryDataRow<string, string>("YT000000000001", "YT")
        {
            TestDisplayName = "圆通单号",
            Skip = "圆通 2026 年换号段,等新格式冻结后再启用",
        },
    ];

    [Theory]
    [MemberData(nameof(CarrierSampleRows))]
    public void TryParse_CarrierSample_ExtractsCarrier(string input, string expectedCarrier)
    {
        Assert.True(TrackingNumberParser.TryParse(input, out var parsed));

        Assert.Equal(expectedCarrier, parsed.Carrier);
    }

    // 写法二:fluent 的 WithTestDisplayName / WithSkip / WithTrait / WithTimeout
    public static IEnumerable<TheoryDataRow<int, ShippingZone, decimal>> PriceTableRows() =>
    [
        new TheoryDataRow<int, ShippingZone, decimal>(500, ShippingZone.Eu, 24.50m)
            .WithTestDisplayName("欧区首重")
            .WithTrait("kind", "pricing"),
        new TheoryDataRow<int, ShippingZone, decimal>(1_000, ShippingZone.Eu, 31.00m)
            .WithTestDisplayName("欧区第二个阶梯")
            .WithTrait("kind", "pricing"),
        new TheoryDataRow<int, ShippingZone, decimal>(1_000, ShippingZone.International, 66.00m)
            .WithTestDisplayName("国际区费率待供应商确认")
            .WithSkip("供应商费率表尚未确认")
            .WithTrait("kind", "pricing"),
    ];

    [Theory]
    [MemberData(nameof(PriceTableRows))]
    public void Estimate_PriceTable_ReturnsExpectedPrice(int weightGrams, ShippingZone zone, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, zone, express: false));

    // Traits 让单行也能被 --filter-trait 挑出来;Timeout 只给这一行设时限
    public static IEnumerable<TheoryDataRow<string>> ProbeRows() =>
    [
        new TheoryDataRow<string>("SF123456789012")
            .WithTestDisplayName("解析单号(单行超时 2 秒)")
            .WithTimeout(ProbeTimeoutMs)
            .WithTrait("kind", "probe"),
    ];

    [Theory]
    [MemberData(nameof(ProbeRows))]
    public async Task Parse_ProbeRow_CompletesWithinRowTimeout(string input)
    {
        // 设了 Timeout 的测试要观察取消令牌:超时与取消都通过它通知
        await Task.Delay(10, TestContext.Current.CancellationToken);

        Assert.Equal(input, TrackingNumberParser.Parse(input).ToString());
    }
}
