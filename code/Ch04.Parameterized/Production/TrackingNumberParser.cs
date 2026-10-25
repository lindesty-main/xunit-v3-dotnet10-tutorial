namespace Ch04.Parameterized.Production;

/// <summary>运单号 = 2 位承运商代码 + 12 位数字。</summary>
public readonly record struct TrackingNumber(string Carrier, string Digits)
{
    public override string ToString() => $"{Carrier}{Digits}";
}

public static class TrackingNumberParser
{
    public const int DigitsLength = 12;
    public static readonly string[] KnownCarriers = ["SF", "YT", "JD"];

    public static bool TryParse(string? input, out TrackingNumber parsed)
    {
        parsed = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;
        if (input.Length != 2 + DigitsLength)
            return false;

        var carrier = input[..2];
        if (Array.IndexOf(KnownCarriers, carrier) < 0)
            return false;

        var digits = input[2..];
        foreach (var ch in digits)
            if (!char.IsAsciiDigit(ch))
                return false;

        parsed = new TrackingNumber(carrier, digits);
        return true;
    }

    public static TrackingNumber Parse(string? input) =>
        TryParse(input, out var parsed)
            ? parsed
            : throw new FormatException($"“{input}” 不是合法的运单号。");
}
