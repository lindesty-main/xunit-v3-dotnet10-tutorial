using Ch02.TestStructure.Production;

namespace Ch02.TestStructure.Tests;

/// <summary>
/// 可读句子式命名示范(把行为写成一个英文句子,不出现被测方法名)。
/// 与 UsernamePolicyTests 是同一类测试类,只是命名风格不同,两者任选其一即可。
/// </summary>
public class PasswordPolicyTests
{
    private readonly PasswordPolicy policy = new();

    [Fact]
    public void Password_shorter_than_8_characters_is_rejected()
    {
        var violations = policy.Validate("Abc1d", "alice");

        var tooShort = Assert.Single(violations, v => v.Rule == PolicyRule.PasswordTooShort);
        Assert.Equal("密码至少需要 8 个字符。", tooShort.Message);
    }

    [Fact]
    public void Password_without_uppercase_letter_is_rejected()
    {
        var violations = policy.Validate("abcdefg1", "alice");

        Assert.Contains(violations, v => v.Rule == PolicyRule.PasswordMissingUppercase);
    }

    [Fact]
    public void Password_without_lowercase_letter_is_rejected()
    {
        var violations = policy.Validate("ABCDEFG1", "alice");

        Assert.Contains(violations, v => v.Rule == PolicyRule.PasswordMissingLowercase);
    }

    [Fact]
    public void Password_without_digit_is_rejected()
    {
        var violations = policy.Validate("Abcdefgh", "alice");

        Assert.Contains(violations, v => v.Rule == PolicyRule.PasswordMissingDigit);
    }

    [Fact]
    public void Weak_password_lists_every_missing_requirement_at_once()
    {
        var violations = policy.Validate("abcdef", "alice");
        var rules = violations.Select(v => v.Rule).ToArray();

        // 这个测试验证的行为是"一次性列出所有缺失项",它的证据就是三条规则。
        // Assert.Multiple 会把三条断言全部跑完,失败时一起报告。
        Assert.Multiple(
            () => Assert.Contains(PolicyRule.PasswordTooShort, rules),
            () => Assert.Contains(PolicyRule.PasswordMissingUppercase, rules),
            () => Assert.Contains(PolicyRule.PasswordMissingDigit, rules));
    }

    [Fact]
    public void Password_containing_the_username_is_rejected_regardless_of_letter_case()
    {
        // Arrange 里的讲究:用户名是全小写的 alice,密码里写成 Alice,
        // 就是为了钉死"忽略大小写"这个行为。
        var violations = policy.Validate("HiAlice2024", "alice");

        Assert.Contains(violations, v => v.Rule == PolicyRule.PasswordContainsUsername);
    }

    [Fact]
    public void Password_that_meets_all_rules_is_accepted()
    {
        var violations = policy.Validate("Str0ngPass", "alice");

        Assert.Empty(violations);
    }

    // 故意失败演示:两个期望值都故意写错,顺带展示 Assert.Multiple 如何一次收集
    // 同一个测试里的多个失败。去掉 Skip 参数即可复现(见正文 2.6 节的失败输出)。
    [Fact(Skip = "演示用:故意写错的断言,去掉 Skip 参数即可复现", DisplayName = "过短密码的提示里应写明最小长度")]
    public void Too_short_password_message_mentions_the_minimum_length()
    {
        var violations = policy.Validate("Abc1def", "alice");
        var tooShort = Assert.Single(violations, v => v.Rule == PolicyRule.PasswordTooShort);

        Assert.Multiple(
            () => Assert.Contains("至少需要 10 个字符", tooShort.Message),
            () => Assert.Contains("最多", tooShort.Message));
    }
}
