using Ch03.Assertions.Production.Catalog;
using Ch03.Assertions.Production.Orders;

namespace Ch03.Assertions.Tests;

// 3.6 组合与结构断言:Multiple/MultipleAsync、Equivalent、EquivalentWithExclusions、StrictEqual。
public class StructuralAssertionTests
{
    private static readonly OrderLine[] oneLine = [new OrderLine("SKU-1", 1, 5m)];

    [Fact]
    public void Multiple_runs_every_check_and_bundles_the_failures()
    {
        var service = new OrderService();
        var order = service.Create("cust-1", oneLine);
        service.Ship(order, "TR-42");

        // 这些断言共同描述一个行为:"发货成功后状态与凭证都正确"。
        Assert.Multiple(
            () => Assert.Equal("Shipped", order.Status),
            () => Assert.Equal("TR-42", order.TrackingNumber),
            () => Assert.Contains("合计 5.00", service.ReceiptFor(order)));
    }

    [Fact]
    public async Task MultipleAsync_collects_async_checks_too()
    {
        var service = new OrderService();
        var order = service.Create("cust-1", oneLine);
        service.Ship(order, "TR-42");

        await Assert.MultipleAsync(
            async () =>
            {
                await Task.Yield();
                Assert.Equal("Shipped", order.Status);
            },
            async () =>
            {
                await Task.Yield();
                Assert.Equal("TR-42", order.TrackingNumber);
            });
    }

    [Fact]
    public void Equivalent_compares_public_members_regardless_of_type()
    {
        var stamp = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var product = new Product { Name = "机械键盘", Price = 399m, UpdatedAt = stamp };
        var dto = new ProductDto { Name = "机械键盘", Price = 399m, UpdatedAt = stamp };

        // Product 与 ProductDto 是两个不相关的类型,公开成员一致即等价。
        Assert.Equivalent(product, dto);

        // 数值按值比较,不看 CLR 类型;这一点连 Equal 都不做(见 3.2)。
        Assert.Equivalent(1, 1.0);
    }

    [Fact]
    public void Equivalent_non_strict_tolerates_extra_members_and_data()
    {
        // 非严格模式:actual 允许比 expected 多出成员/多出数据。
        Assert.Equivalent(
            new { Name = "机械键盘" },
            new ProductDto { Name = "机械键盘", Price = 399m, UpdatedAt = DateTimeOffset.UnixEpoch });
        Assert.Equivalent(new[] { 1 }, new[] { 1, 2 });

        // expected 有而 actual 没有的成员,是真正的缺失,必失败。
        var missing = Record.Exception(() =>
            Assert.Equivalent(new { Name = "机械键盘", Price = 399m }, new { Name = "机械键盘" }));
        Assert.Contains("Mismatched member list", missing!.Message);

        // strict: true 要求成员列表/集合数据精确一致,多出来的也不行。
        var strict = Record.Exception(() =>
            Assert.Equivalent(new { Name = "机械键盘" },
                new ProductDto { Name = "机械键盘", Price = 399m, UpdatedAt = DateTimeOffset.UnixEpoch },
                strict: true));
        Assert.NotNull(strict);
    }

    [Fact]
    public void EquivalentWithExclusions_drops_members_named_by_expressions()
    {
        var stored = new Product { Name = "机械键盘", Price = 399m, UpdatedAt = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero) };
        var dto = new ProductDto { Name = "机械键盘", Price = 399m, UpdatedAt = DateTimeOffset.UtcNow };

        // UpdatedAt 必然不同,整体 Equivalent 失败。
        var failure = Record.Exception(() => Assert.Equivalent(stored, dto));
        Assert.NotNull(failure);

        // 表达式形式:编译期检查成员存在,重构安全。
        Assert.EquivalentWithExclusions(stored, dto, p => p.UpdatedAt);

        // 字符串形式:适合匿名类型等无法写表达式的场合。
        Assert.EquivalentWithExclusions(stored, dto, "UpdatedAt");
    }

    [Fact]
    public void StrictEqual_is_the_type_strict_variant_of_Equal()
    {
        Assert.StrictEqual(new OrderLine("SKU-1", 1, 5m), new OrderLine("SKU-1", 1, 5m));
        Assert.NotStrictEqual(new OrderLine("SKU-1", 1, 5m), new OrderLine("SKU-1", 2, 5m));

        Assert.Equal<IEnumerable<int>>(new[] { 1 }, new List<int> { 1 }); // Equal:通过(序列比较)
        var failure = Record.Exception(() =>
            Assert.StrictEqual<IEnumerable<int>>(new[] { 1 }, new List<int> { 1 })); // StrictEqual:失败
        Assert.NotNull(failure);
    }
}
