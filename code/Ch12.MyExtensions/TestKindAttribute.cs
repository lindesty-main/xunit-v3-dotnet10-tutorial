using Xunit.v3;

namespace Ch12.MyExtensions;

// v3 移除了 v2 的 TraitDiscoverer:特性自己负责报告自己提供什么 Trait。
// 任何实现 ITraitAttribute 的特性(包括派生自 FactAttribute 的)都会被收集 Trait。

/// <summary>Fact + Trait("kind", kind):把"测试种类"这类标签封装成带构造参数的特性。</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TestKindAttribute(string kind) : FactAttribute, ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() => [new("kind", kind)];
}

/// <summary>等价于 [TestKind("integration")]:标记为集成测试,可用 --filter-trait "kind=integration" 单独筛出。</summary>
public sealed class IntegrationTestAttribute() : TestKindAttribute("integration");
