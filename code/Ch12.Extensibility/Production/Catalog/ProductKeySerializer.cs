using Xunit.Sdk;

namespace Ch12.Extensibility.Production;

/// <summary>
/// 外部序列化器(IXunitSerializer):把 ProductKey 序列化成它的规范文本形式,
/// 再反序列化回来。注册见 AssemblyInfo.cs 的 [assembly: RegisterXunitSerializer(...)]。
/// 有了它,含 ProductKey 参数的 Theory 数据行才能被 runner 跨进程交还、逐行运行。
/// </summary>
public sealed class ProductKeySerializer : IXunitSerializer
{
    public bool IsSerializable(Type type, object? value, out string failureReason)
    {
        if (value is ProductKey or null)
        {
            failureReason = string.Empty;
            return true;
        }

        failureReason = $"无法序列化 {type}:值不是 {nameof(ProductKey)}";
        return false;
    }

    public string Serialize(object value) =>
        value is ProductKey key
            ? key.ToString()
            : throw new ArgumentException($"无法序列化 {value.GetType()}", nameof(value));

    public object Deserialize(Type type, string serializedValue) => ProductKey.Parse(serializedValue);
}
