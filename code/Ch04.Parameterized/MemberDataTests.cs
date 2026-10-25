using Ch04.Parameterized.Production;
using Ch04.Parameterized.Tests.TestData;

namespace Ch04.Parameterized.Tests;

/// <summary>MemberData:数据来自方法/属性/字段,行格式有三种。</summary>
public class MemberDataTests
{
    // 规格表:测试侧独立写一遍,不引用生产代码里的常量,避免"用实现验证实现"。
    private const decimal ExpressSurcharge = 15.00m;

    private readonly ShippingEstimator estimator = new();

    // 数据源:属性;行格式一:object?[]
    public static IEnumerable<object?[]> InvalidTrackingNumbers =>
    [
        new object?[] { null },
        new object?[] { "" },
        new object?[] { "SF12345678901" },
        new object?[] { "SF1234567890123" },
        new object?[] { "AB123456789012" },
        new object?[] { "SF123456789O12" },
    ];

    [Theory]
    [MemberData(nameof(InvalidTrackingNumbers))]
    public void TryParse_InvalidTrackingNumber_ReturnsFalse(string? input)
        => Assert.False(TrackingNumberParser.TryParse(input, out _));

    // 数据源:方法;行格式二:元组(字段名会出现在参数列表里,可读性好)
    public static IEnumerable<(ShippingZone Zone, int JustInside, int JustOutside, decimal StepPrice)> StepBoundaryRows() =>
    [
        (ShippingZone.Domestic, 500, 501, 3.00m),
        (ShippingZone.Eu, 1_000, 1_001, 6.50m),
        (ShippingZone.International, 1_500, 1_501, 11.00m),
    ];

    [Theory]
    [MemberData(nameof(StepBoundaryRows))]
    public void Estimate_CrossingWeightStep_AddsExactlyOneStepPrice(
        ShippingZone zone, int justInside, int justOutside, decimal stepPrice)
    {
        var insidePrice = estimator.Estimate(justInside, zone, express: false);
        var outsidePrice = estimator.Estimate(justOutside, zone, express: false);

        Assert.Equal(stepPrice, outsidePrice - insidePrice);
    }

    // 数据源:方法;行格式三:ITheoryDataRow(可携带元数据,4.6 展开)
    public static IEnumerable<TheoryDataRow<string, string, string>> KnownCarrierRows() =>
        [
            new TheoryDataRow<string, string, string>("SF", "SF123456789012", "123456789012"),
            new TheoryDataRow<string, string, string>("YT", "YT000000000001", "000000000001"),
            new TheoryDataRow<string, string, string>("JD", "JD999999999999", "999999999999")
            {
                Skip = "JD 号段规则尚未冻结,先挂起",
            },
        ];

    [Theory]
    [MemberData(nameof(KnownCarrierRows))]
    public void TryParse_KnownCarrier_SplitsCarrierAndDigits(string carrier, string input, string expectedDigits)
    {
        Assert.True(TrackingNumberParser.TryParse(input, out var parsed));

        Assert.Equal(carrier, parsed.Carrier);
        Assert.Equal(expectedDigits, parsed.Digits);
    }

    // 数据源:另一个类型里的 async 方法(见 TestData/LegacyTrackingNumberRows.cs)
    [Theory]
    [MemberData(nameof(LegacyTrackingNumberRows.RoundTripRowsAsync), MemberType = typeof(LegacyTrackingNumberRows))]
    public void Parse_LegacyTrackingNumber_RoundTripsToSameString(string input)
    {
        var parsed = TrackingNumberParser.Parse(input);

        Assert.Equal(input, parsed.ToString());
    }

    // 带参数的数据源:Arguments 只喂给数据方法,测试方法想要同一个值就把它写进行里
    public static IEnumerable<object?[]> PriceRowsFor(ShippingZone zone)
    {
        var (firstWeightPrice, stepPrice) = ZonePriceTable(zone);

        yield return new object?[] { 500, zone, false, firstWeightPrice };
        yield return new object?[] { 1_000, zone, false, firstWeightPrice + stepPrice };
        yield return new object?[] { 500, zone, true, firstWeightPrice + ExpressSurcharge };
    }

    private static (decimal FirstWeightPrice, decimal StepPrice) ZonePriceTable(ShippingZone zone) => zone switch
    {
        ShippingZone.Domestic => (12.00m, 3.00m),
        ShippingZone.Eu => (24.50m, 6.50m),
        ShippingZone.International => (55.00m, 11.00m),
        _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, "未知区域"),
    };

    [Theory]
    [MemberData(nameof(PriceRowsFor), ShippingZone.Eu)]
    public void Estimate_EuZonePriceTable_ReturnsExpectedPrice(
        int weightGrams, ShippingZone zone, bool express, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, zone, express));

    [Theory]
    [MemberData(nameof(PriceRowsFor), ShippingZone.International)]
    public void Estimate_InternationalZonePriceTable_ReturnsExpectedPrice(
        int weightGrams, ShippingZone zone, bool express, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, zone, express));
}
