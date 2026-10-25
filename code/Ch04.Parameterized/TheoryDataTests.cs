using Ch04.Parameterized.Production;

namespace Ch04.Parameterized.Tests;

/// <summary>强类型数据:TheoryData&lt;...&gt; 让参数类型与顺序错误停在编译期。</summary>
public class TheoryDataTests
{
    private readonly ShippingEstimator estimator = new();

    // 强类型字段 + 集合初始化器:类型、顺序、个数写错都编译不过
    public static readonly TheoryData<int, ShippingZone, bool, decimal> EstimateRows = new()
    {
        { 500, ShippingZone.Domestic, false, 12.00m },
        { 501, ShippingZone.Domestic, false, 15.00m },
        { 1_000, ShippingZone.Eu, false, 31.00m },
        { 1_000, ShippingZone.International, true, 81.00m },
    };

    [Theory]
    [MemberData(nameof(EstimateRows))]
    public void Estimate_StronglyTypedRows_ReturnsExpectedPrice(
        int weightGrams, ShippingZone zone, bool express, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, zone, express));

    // 同样一张表用 Add 逐行写:与初始化器等价,适合在生成数据时加逻辑
    public static TheoryData<string, string> ValidNumberRows { get; } = BuildValidNumberRows();

    private static TheoryData<string, string> BuildValidNumberRows()
    {
        var rows = new TheoryData<string, string>();

        foreach (var carrier in TrackingNumberParser.KnownCarriers)
            rows.Add($"{carrier}123456789012", carrier);

        return rows;
    }

    [Theory]
    [MemberData(nameof(ValidNumberRows))]
    public void TryParse_StronglyTypedGeneratedRows_ExtractsCarrier(string input, string expectedCarrier)
    {
        Assert.True(TrackingNumberParser.TryParse(input, out var parsed));

        Assert.Equal(expectedCarrier, parsed.Carrier);
    }

    // 没有类型参数的写法:编译器不再检查任何东西,顺序或类型写错要到运行时才看得见
    public static IEnumerable<object?[]> EstimateRowsUntyped =>
    [
        new object?[] { 500, ShippingZone.Domestic, false, 12.00m },
        new object?[] { 501, ShippingZone.Domestic, false, 15.00m },
        new object?[] { 1_000, ShippingZone.Eu, false, 31.00m },
        new object?[] { 1_000, ShippingZone.International, true, 81.00m },
    ];

    [Theory]
    [MemberData(nameof(EstimateRowsUntyped))]
    public void Estimate_UntypedRows_ReturnsExpectedPrice(
        int weightGrams, ShippingZone zone, bool express, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, zone, express));

    // 4.5 的反例:数据行是 (decimal, int),测试方法要的是 (int, decimal)。
    // 保留 #pragma 只为让章工程零警告;去掉它,编译期就会报 xUnit1039(正文有真实输出)。
#pragma warning disable xUnit1039
    [Theory(Skip = "演示:数据行类型与参数类型不匹配,xUnit1039 在编译期拦下(见 4.5)")]
    [MemberData(nameof(SwappedRows))]
    public void Estimate_SwappedArgumentOrder(int weightGrams, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, ShippingZone.Domestic, express: false));

    public static readonly TheoryData<decimal, int> SwappedRows = new()
    {
        { 12.00m, 500 },
    };
#pragma warning restore xUnit1039
}
