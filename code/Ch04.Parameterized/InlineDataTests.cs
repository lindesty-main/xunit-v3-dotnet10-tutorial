using Ch04.Parameterized.Production;

namespace Ch04.Parameterized.Tests;

/// <summary>字面量数据:每一行 InlineData 都是一个独立测试用例。</summary>
public class InlineDataTests
{
    private const int FirstWeight = 500;
    private const string CarrierJdTrackingNumber = "JD999999999999";

    private readonly ShippingEstimator estimator = new();

    [Theory]
    [InlineData("SF123456789012", "SF", "123456789012")]
    [InlineData("YT000000000001", "YT", "000000000001")]
    [InlineData(CarrierJdTrackingNumber, "JD", "999999999999")]
    public void TryParse_ValidTrackingNumber_SplitsCarrierAndDigits(string input, string expectedCarrier, string expectedDigits)
    {
        Assert.True(TrackingNumberParser.TryParse(input, out var parsed));

        Assert.Equal(expectedCarrier, parsed.Carrier);
        Assert.Equal(expectedDigits, parsed.Digits);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("SF12345678901")]
    [InlineData("SF1234567890123")]
    [InlineData("AB123456789012")]
    [InlineData("sf123456789012")]
    [InlineData("SF123456789O12")]
    public void TryParse_InvalidTrackingNumber_ReturnsFalse(string? input)
        => Assert.False(TrackingNumberParser.TryParse(input, out _));

    [Theory]
    [InlineData(FirstWeight, ShippingZone.Domestic, false, 12.00)]
    [InlineData(FirstWeight + 1, ShippingZone.Domestic, false, 15.00)]
    [InlineData(2 * FirstWeight, ShippingZone.Eu, false, 31.00)]
    [InlineData(FirstWeight, ShippingZone.Eu, true, 39.50)]
    [InlineData(1_000, ShippingZone.International, true, 81.00)]
    public void Estimate_KnownCombination_ReturnsExpectedPrice(
        int weightGrams, ShippingZone zone, bool express, decimal expected)
        => Assert.Equal(expected, estimator.Estimate(weightGrams, zone, express));

    [Theory]
    [InlineData(new[] { 500, 1000 }, ShippingZone.Eu, false, 55.50)]
    [InlineData(new int[0], ShippingZone.Eu, true, 0.00)]
    public void EstimateBatch_MultiParcelOrder_SumsEveryParcel(
        int[] weightGrams, ShippingZone zone, bool express, decimal expected)
        => Assert.Equal(expected, estimator.EstimateBatch(weightGrams, zone, express));

    // 4.2 的陷阱演示:这一行被 xUnit1010 在编译期拦下(object?[] 与 int[] 类型不符)。
    // 保留 #pragma 只为让章工程零警告;去掉它,编译期就会报 xUnit1010(正文有真实输出)。
    // 只去掉下面的 Skip 时测试其实会通过——运行时 xUnit 能把 object?[] 逐元素转成 int[],
    // 拦住这种写法的是分析器,不是运行时。
    [Theory(Skip = "演示:object?[] 与 int[] 类型不符,xUnit1010 在编译期拦截(见 4.2)")]
#pragma warning disable xUnit1010
    [InlineData(new object?[] { 500, 1000 }, ShippingZone.Eu, false, 55.50)]
#pragma warning restore xUnit1010
    public void EstimateBatch_WrongArrayShape(
        int[] weightGrams, ShippingZone zone, bool express, decimal expected)
        => Assert.Equal(expected, estimator.EstimateBatch(weightGrams, zone, express));
}
