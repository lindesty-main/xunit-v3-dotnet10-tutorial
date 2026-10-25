namespace Ch04.Parameterized.Production;

public enum ShippingZone
{
    Domestic = 0,
    Eu = 1,
    International = 2,
}

/// <summary>
/// 运费 = 区域首重价 + 每超出一个 500g 阶梯加收;加急另收固定附加费。
/// 首重覆盖 1..500g;501..1000g 加收 1 阶,以此类推。
/// </summary>
public sealed class ShippingEstimator
{
    public const int WeightStepGrams = 500;
    private const decimal ExpressSurcharge = 15.00m;

    private static readonly decimal[] firstWeightPrice = [12.00m, 24.50m, 55.00m];
    private static readonly decimal[] stepPrice = [3.00m, 6.50m, 11.00m];

    public decimal Estimate(int weightGrams, ShippingZone zone, bool express)
    {
        if (weightGrams <= 0)
            throw new ArgumentOutOfRangeException(nameof(weightGrams), weightGrams, "重量必须大于 0。");

        var zoneIndex = (int)zone;
        if (zoneIndex is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(zone), zone, "未知配送区域。");

        var extraSteps = (weightGrams - 1) / WeightStepGrams;
        var total = firstWeightPrice[zoneIndex] + extraSteps * stepPrice[zoneIndex];
        return express ? total + ExpressSurcharge : total;
    }

    /// <summary>多包裹订单:逐包裹估价后求和;空数组返回 0。</summary>
    public decimal EstimateBatch(int[] weightGrams, ShippingZone zone, bool express) =>
        weightGrams.Sum(weight => Estimate(weight, zone, express));
}
