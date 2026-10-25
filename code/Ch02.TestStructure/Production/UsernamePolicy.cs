namespace Ch02.TestStructure.Production;

/// <summary>
/// 本章的被测对象之一:用户名策略。规则:
/// 长度 3-20;必须以 ASCII 字母开头;只允许 ASCII 字母、数字和下划线。
/// Validate 返回所有被违反的规则(空列表表示合法),而不是遇到第一条就停下。
/// </summary>
public class UsernamePolicy
{
    public const int MinLength = 3;
    public const int MaxLength = 20;

    public IReadOnlyList<PolicyViolation> Validate(string? username)
    {
        var violations = new List<PolicyViolation>();

        if (string.IsNullOrWhiteSpace(username))
        {
            violations.Add(new PolicyViolation(PolicyRule.UsernameRequired, "用户名不能为空。"));
            return violations;
        }

        if (username.Length < MinLength)
            violations.Add(new PolicyViolation(PolicyRule.UsernameTooShort, $"用户名至少需要 {MinLength} 个字符。"));
        if (username.Length > MaxLength)
            violations.Add(new PolicyViolation(PolicyRule.UsernameTooLong, $"用户名最多允许 {MaxLength} 个字符。"));
        if (!IsAsciiLetter(username[0]))
            violations.Add(new PolicyViolation(PolicyRule.UsernameMustStartWithLetter, "用户名必须以字母开头。"));

        var invalidChars = username.Where(c => !IsAllowedCharacter(c)).Distinct().ToArray();
        if (invalidChars.Length > 0)
            violations.Add(new PolicyViolation(
                PolicyRule.UsernameHasInvalidCharacter,
                $"用户名只能包含字母、数字和下划线,出现非法字符:{string.Join(" ", invalidChars)}。"));

        return violations;
    }

    private static bool IsAsciiLetter(char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';

    private static bool IsAllowedCharacter(char c) => IsAsciiLetter(c) || char.IsAsciiDigit(c) || c == '_';
}
