namespace Ch03.Assertions.Production.Customers;

// 故意不重写 Equals:演示"引用类型走 Equals"时,字段相同也不相等的行为。
public sealed class Customer
{
    public required string Name { get; init; }
    public string? Nickname { get; init; }
}
