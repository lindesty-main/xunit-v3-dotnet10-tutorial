namespace Ch01.GettingStarted.Production;

/// <summary>
/// 本章的被测对象:把购物车里各单品的价格汇总,套用会员折扣,再算出含税总价。
/// 逻辑刻意保持简单,读者的注意力可以全部放在测试本身。
/// </summary>
public class PriceCalculator
{
    private const decimal TaxRate = 0.13m;
    private const decimal DiscountThreshold = 100m;
    private const decimal DiscountAmount = 20m;

    /// <summary>各单品价格合计。</summary>
    public decimal Subtotal(IEnumerable<decimal> itemPrices)
    {
        ArgumentNullException.ThrowIfNull(itemPrices);
        return itemPrices.Sum();
    }

    /// <summary>会员折扣:合计满 100 减 20,不满则原价。</summary>
    public decimal ApplyMemberDiscount(decimal subtotal)
    {
        if (subtotal < 0m)
            throw new ArgumentOutOfRangeException(nameof(subtotal), subtotal, "合计金额不能为负数。");

        return subtotal >= DiscountThreshold ? subtotal - DiscountAmount : subtotal;
    }

    /// <summary>按 13% 的税率计算含税总价,四舍五入到分。</summary>
    public decimal TotalWithTax(decimal netAmount)
    {
        if (netAmount < 0m)
            throw new ArgumentOutOfRangeException(nameof(netAmount), netAmount, "金额不能为负数。");

        return Math.Round(netAmount * (1m + TaxRate), 2, MidpointRounding.AwayFromZero);
    }
}
