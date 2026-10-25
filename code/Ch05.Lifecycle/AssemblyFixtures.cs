// 程序集夹具:整个测试程序集共用一个实例,在任何测试开始前构造,全部结束后释放。
// 两种等价的注册写法——typeof 形式在所有支持的运行时可用;
// 泛型形式要求 .NET 8+(.NET Framework 不支持泛型特性)。
// 程序集级特性必须写在命名空间声明之前,所以这里用完全限定名。
[assembly: AssemblyFixture(typeof(Ch05.Lifecycle.Tests.TestRunClock))]
[assembly: AssemblyFixture<Ch05.Lifecycle.Production.AuditSink>]

namespace Ch05.Lifecycle.Tests;

/// <summary>
/// 程序集夹具示例:记录本次测试运行的开始时间。
/// 任何测试类都可以直接把它作为构造参数注入,不需要实现任何标记接口。
/// </summary>
public sealed class TestRunClock
{
    public static int InstancesCreated;
    public static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, byte> SeenIds = new();

    public Guid Id { get; } = Guid.NewGuid();

    public DateTimeOffset RunStartedAt { get; }

    public TestRunClock()
    {
        InstancesCreated++;
        RunStartedAt = DateTimeOffset.UtcNow;
    }
}
