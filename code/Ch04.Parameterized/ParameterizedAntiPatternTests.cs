using Ch04.Parameterized.Production;

namespace Ch04.Parameterized.Tests;

/// <summary>4.9 的反例:能跑通,但不该这样写。全部通过,故意留在工程里对照。</summary>
public class ParameterizedAntiPatternTests
{
    private readonly ShippingEstimator estimator = new();

    // 反例一:把断言逻辑塞进数据 —— 数据行里写死了实现细节(异常文案)
    public static IEnumerable<object?[]> MessageRows() =>
    [
        new object?[] { "SF12345678901", "“SF12345678901” 不是合法的运单号。" },
        new object?[] { "AB123456789012", "“AB123456789012” 不是合法的运单号。" },
    ];

    [Theory]
    [MemberData(nameof(MessageRows))]
    public void Parse_InvalidNumber_ExceptionMessageTakenFromData(string input, string expectedMessage)
    {
        var exception = Assert.Throws<FormatException>(() => TrackingNumberParser.Parse(input));

        Assert.Equal(expectedMessage, exception.Message);
    }

    // 反例二:一行数据覆盖多个行为 —— 解析与报价挤在同一条测试里
    public static IEnumerable<object?[]> MixedBehaviorRows() =>
    [
        new object?[] { "SF123456789012", "SF", 500, 12.00m },
        new object?[] { "YT000000000001", "YT", 1_000, 15.00m },
    ];

    [Theory]
    [MemberData(nameof(MixedBehaviorRows))]
    public void ParseAndEstimate_OneRowCoversTwoBehaviors(
        string input, string expectedCarrier, int weightGrams, decimal expectedPrice)
    {
        Assert.True(TrackingNumberParser.TryParse(input, out var parsed));
        Assert.Equal(expectedCarrier, parsed.Carrier);
        Assert.Equal(expectedPrice, estimator.Estimate(weightGrams, ShippingZone.Domestic, express: false));
    }

    // 反例三:矩阵爆炸 —— 承运商这一维与"数字部分非法"无关,
    // 3 × 4 = 12 行里只有 4 种真实分支,其余 8 行是重复覆盖。
    public static readonly MatrixTheoryData<string, string> CarrierByBadDigits = new(
        ["SF", "YT", "JD"],
        ["12345678901", "1234567890123", "123456789O12", "123456789012X"]);

    [Theory]
    [MemberData(nameof(CarrierByBadDigits))]
    public void TryParse_BadDigitsForEveryCarrier_ReturnsFalse(string carrier, string badDigits)
        => Assert.False(TrackingNumberParser.TryParse(carrier + badDigits, out _));
}
