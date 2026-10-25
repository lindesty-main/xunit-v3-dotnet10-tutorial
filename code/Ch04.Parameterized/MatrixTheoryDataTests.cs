using Ch04.Parameterized.Production;

namespace Ch04.Parameterized.Tests;

/// <summary>矩阵数据:维度正交时,用笛卡尔积代替手写表格。</summary>
public class MatrixTheoryDataTests
{
    private const decimal ExpressSurcharge = 15.00m;

    private readonly ShippingEstimator estimator = new();

    private static decimal FirstWeightPriceOf(ShippingZone zone) => zone switch
    {
        ShippingZone.Domestic => 12.00m,
        ShippingZone.Eu => 24.50m,
        ShippingZone.International => 55.00m,
        _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, "未知区域"),
    };

    private static decimal StepPriceOf(ShippingZone zone) => zone switch
    {
        ShippingZone.Domestic => 3.00m,
        ShippingZone.Eu => 6.50m,
        ShippingZone.International => 11.00m,
        _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, "未知区域"),
    };

    // 两个维度:重量 × 区域 → 3 × 2 = 6 行
    public static readonly MatrixTheoryData<int, ShippingZone> WeightByZone = new(
        [500, 1_000, 1_500],
        [ShippingZone.Domestic, ShippingZone.Eu]);

    [Theory]
    [MemberData(nameof(WeightByZone))]
    public void Estimate_EveryWeightAndZone_QuoteLandsOnStepGrid(int weightGrams, ShippingZone zone)
    {
        var quoted = estimator.Estimate(weightGrams, zone, express: false);
        var firstWeightPrice = FirstWeightPriceOf(zone);
        var stepPrice = StepPriceOf(zone);

        Assert.True(quoted >= firstWeightPrice, $"{zone} 区域 {weightGrams}g 报价 {quoted} 不应低于首重价 {firstWeightPrice}。");
        Assert.Equal(0m, (quoted - firstWeightPrice) % stepPrice);
    }

    // 三个维度:重量 × 区域 × 是否加急 → 2 × 2 × 2 = 8 行
    public static readonly MatrixTheoryData<int, ShippingZone, bool> WeightByZoneBySpeed = new(
        [500, 1_000],
        [ShippingZone.Eu, ShippingZone.International],
        [false, true]);

    [Theory]
    [MemberData(nameof(WeightByZoneBySpeed))]
    public void Estimate_ExpressOnlyAddsFixedSurcharge(int weightGrams, ShippingZone zone, bool express)
    {
        var quoted = estimator.Estimate(weightGrams, zone, express);
        var standard = estimator.Estimate(weightGrams, zone, express: false);

        Assert.Equal(express ? standard + ExpressSurcharge : standard, quoted);
    }

    // 笛卡尔积之外还能补例外行:Add / AddRange 接受元组或 TheoryDataRow
    public static MatrixTheoryData<int, ShippingZone> WeightByZoneWithExceptions()
    {
        var matrix = new MatrixTheoryData<int, ShippingZone>([500, 1_000], [ShippingZone.Domestic]);

        matrix.Add(1_500, ShippingZone.Eu);
        matrix.AddRange([(2_000, ShippingZone.International)]);
        matrix.Add(new TheoryDataRow<int, ShippingZone>(2_500, ShippingZone.Eu) { TestDisplayName = "手工补充的超大件" });

        return matrix;
    }

    [Theory]
    [MemberData(nameof(WeightByZoneWithExceptions))]
    public void Estimate_ExtendedMatrix_QuoteLandsOnStepGrid(int weightGrams, ShippingZone zone)
    {
        var quoted = estimator.Estimate(weightGrams, zone, express: false);
        var stepPrice = StepPriceOf(zone);

        Assert.Equal(0m, (quoted - FirstWeightPriceOf(zone)) % stepPrice);
    }
}
