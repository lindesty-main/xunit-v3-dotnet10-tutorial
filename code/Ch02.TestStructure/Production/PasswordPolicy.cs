namespace Ch02.TestStructure.Production;

/// <summary>
/// 本章的被测对象之二:密码策略。规则:
/// 长度至少 8;必须同时包含小写字母、大写字母和数字;不得包含用户名(忽略大小写)。
/// Validate 返回所有被违反的规则(空列表表示合法),而不是遇到第一条就停下。
/// </summary>
public class PasswordPolicy
{
    public const int MinLength = 8;

    public IReadOnlyList<PolicyViolation> Validate(string password, string username)
    {
        var violations = new List<PolicyViolation>();

        if (password.Length < MinLength)
            violations.Add(new PolicyViolation(PolicyRule.PasswordTooShort, $"密码至少需要 {MinLength} 个字符。"));
        if (!password.Any(char.IsLower))
            violations.Add(new PolicyViolation(PolicyRule.PasswordMissingLowercase, "密码必须包含小写字母。"));
        if (!password.Any(char.IsUpper))
            violations.Add(new PolicyViolation(PolicyRule.PasswordMissingUppercase, "密码必须包含大写字母。"));
        if (!password.Any(char.IsDigit))
            violations.Add(new PolicyViolation(PolicyRule.PasswordMissingDigit, "密码必须包含数字。"));
        if (username.Length > 0 && password.Contains(username, StringComparison.OrdinalIgnoreCase))
            violations.Add(new PolicyViolation(PolicyRule.PasswordContainsUsername, "密码不能包含用户名。"));

        return violations;
    }
}
