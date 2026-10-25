using Xunit.Sdk;
using Xunit.v3;

namespace Ch06.Parallel.Tests.Orderers;

/// <summary>
/// 最小方法排序器:让同一个类里的测试方法按源码声明顺序执行。
/// 接口方法(4.0 起)是泛型方法,约束只到标记接口 <see cref="ITestMethod"/>(Xunit.Sdk);
/// 运行时传入的实例总是同时实现 <see cref="IXunitTestMethod"/>,元数据经它读取。
/// MetadataToken 与编译顺序一致,即声明顺序。
/// </summary>
public sealed class DeclarationOrderMethodOrderer : ITestMethodOrderer
{
    public IReadOnlyCollection<TTestMethod?> OrderTestMethods<TTestMethod>(
        IReadOnlyCollection<TTestMethod?> testMethods)
        where TTestMethod : ITestMethod =>
        [.. testMethods.OrderBy(m => ((IXunitTestMethod)m!).MetadataToken)];
}
