namespace Ch12.Extensibility.Production;

/// <summary>目录服务的产品键(生产类型):文本形式为 <正整数分类ID>:<字母数字SKU>。</summary>
public sealed record ProductKey(int CategoryId, string Sku)
{
    public static ProductKey Parse(string text)
    {
        if (!TryParse(text, out var key) || key is null)
            throw new FormatException($"\"{text}\" 不是合法产品键(期望格式:<正整数分类ID>:<字母数字SKU>)");
        return key;
    }

    public static bool TryParse(string text, out ProductKey? key)
    {
        key = null;
        var separator = text.IndexOf(':');
        if (separator <= 0 || separator == text.Length - 1)
            return false;
        if (!int.TryParse(text[..separator], out var categoryId) || categoryId <= 0)
            return false;
        var sku = text[(separator + 1)..];
        if (!sku.All(char.IsAsciiLetterOrDigit))
            return false;
        key = new ProductKey(categoryId, sku);
        return true;
    }

    public override string ToString() => $"{CategoryId}:{Sku}";
}
