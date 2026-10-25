using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.4 集合断言:Contains/DoesNotContain、All、Single、Collection、Distinct、Empty、Equal+comparer。
public class CollectionAssertionTests
{
    private static readonly OrderLine[] lines = [new OrderLine("SKU-1", 2, 10m), new OrderLine("SKU-2", 1, 5m)];

    private static Order NewOrder() => new OrderService().Create("cust-1", lines);

    [Fact]
    public void Contains_checks_elements_predicates_and_dictionary_keys()
    {
        var order = NewOrder();

        // 元素(按 Equals 判定)。
        Assert.Contains(lines[0], order.Lines);

        // 谓词:v3 没有 Assert.Any,"存在满足条件的元素"就是 Contains 的谓词重载。
        Assert.Contains(order.Lines, line => line.Quantity > 1);
        var none = Record.Exception(() => Assert.Contains(order.Lines, line => line.Quantity > 99));
        Assert.Contains("Filter not matched in collection", none!.Message);

        // 字典按键命中,并且返回对应的值,可以接着断言。
        var prices = new Dictionary<string, decimal> { ["SKU-1"] = 10m };
        var price = Assert.Contains("SKU-1", prices);
        Assert.Equal(10m, price);
        Assert.DoesNotContain("SKU-9", prices);
    }

    [Fact]
    public void All_runs_an_inspector_over_every_element()
    {
        var order = NewOrder();
        Assert.All(order.Lines, line => Assert.True(line.Quantity > 0));

        Assert.All(Array.Empty<OrderLine>(), line => Assert.True(line.Quantity > 0)); // 空集合:通过
        var empty = Record.Exception(() =>
            Assert.All(Array.Empty<OrderLine>(), line => Assert.True(line.Quantity > 0), throwIfEmpty: true));
        Assert.NotNull(empty); // 要求非空:失败
    }

    [Fact]
    public void Single_requires_exactly_one_match_and_returns_it()
    {
        var order = NewOrder();

        var expensive = Assert.Single(order.Lines, line => line.UnitPrice > 9m);
        Assert.Equal("SKU-1", expensive.Sku);

        // 两条明细都满足 quantity >= 1 → 不是 Single。
        var failure = Record.Exception(() => Assert.Single(order.Lines, line => line.Quantity >= 1));
        Assert.NotNull(failure);
    }

    [Fact]
    public void Collection_pairs_each_element_with_an_inspector_in_order()
    {
        var order = NewOrder();
        Assert.Collection(order.Lines,
            line => Assert.Equal("SKU-1", line.Sku),
            line => Assert.Equal("SKU-2", line.Sku));

        // 检查器数量必须与元素数量一致,多一个少一个都失败。
        var mismatch = Record.Exception(() =>
            Assert.Collection(order.Lines, line => Assert.Equal("SKU-1", line.Sku)));
        Assert.Contains("Mismatched item count", mismatch!.Message);
    }

    [Fact]
    public async Task CollectionAsync_is_the_awaitable_form()
    {
        var order = NewOrder();
        await Assert.CollectionAsync(order.Lines,
            async line => { await Task.Yield(); Assert.Equal("SKU-1", line.Sku); },
            async line => { await Task.Yield(); Assert.Equal("SKU-2", line.Sku); });
    }

    [Fact]
    public void Distinct_requires_no_duplicate_elements()
    {
        Assert.Distinct(lines.Select(line => line.Sku));

        var duplicated = Record.Exception(() => Assert.Distinct(new[] { "SKU-1", "SKU-1" }));
        Assert.NotNull(duplicated);
    }

    [Fact]
    public void Empty_and_NotEmpty_check_for_any_elements()
    {
        var order = NewOrder();
        Assert.NotEmpty(order.Lines);

        // "没有满足条件的元素"用 DoesNotContain(谓词);Empty 只用于真正要断言为空的集合。
        Assert.DoesNotContain(order.Lines, line => line.Quantity > 99);
        Assert.Empty(new List<OrderLine>());
    }

    [Fact]
    public void Equality_with_a_custom_comparer()
    {
        // Func 比较器:逐元素用给定函数判定。
        Assert.Equal(new[] { 1, 2 }, new[] { 11, 22 }, (x, y) => y == x * 11);

        Assert.Contains(3, new[] { 1, 2, 40 }, new RangeComparer()); // |x-y| <= 20 即视为相等

        var failure = Record.Exception(() => Assert.Equal(new[] { 1, 2 }, new[] { 1, 3 }, (x, y) => x == y));
        Assert.NotNull(failure);
    }

    sealed class RangeComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y) => Math.Abs(x - y) <= 20;
        public int GetHashCode(int x) => 0;
    }
}
