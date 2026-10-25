using Ch02.TestStructure.Production;

namespace Ch02.TestStructure.Tests;

/// <summary>
/// 三段式命名示范(被测方法_条件_期望行为)。
/// 测试类与被测类 UsernamePolicy 一一对应。
/// </summary>
public class UsernamePolicyTests
{
    private readonly UsernamePolicy policy = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_UsernameBlank_ReportsRequiredViolation(string? username)
    {
        var violations = policy.Validate(username);

        var violation = Assert.Single(violations);
        Assert.Equal(PolicyRule.UsernameRequired, violation.Rule);
    }

    [Fact]
    public void Validate_UsernameShorterThanMinimum_ReportsTooShortViolation()
    {
        var violations = policy.Validate("ab");

        Assert.Contains(violations, v => v.Rule == PolicyRule.UsernameTooShort);
    }

    [Fact]
    public void Validate_UsernameLongerThanMaximum_ReportsTooLongViolation()
    {
        var username = new string('a', UsernamePolicy.MaxLength + 1);

        var violations = policy.Validate(username);

        Assert.Contains(violations, v => v.Rule == PolicyRule.UsernameTooLong);
    }

    [Theory]
    [InlineData("3alice")]
    [InlineData("_alice")]
    [InlineData("1st")]
    public void Validate_UsernameNotStartingWithLetter_ReportsMustStartWithLetterViolation(string username)
    {
        var violations = policy.Validate(username);

        Assert.Contains(violations, v => v.Rule == PolicyRule.UsernameMustStartWithLetter);
    }

    [Theory]
    [InlineData("li-si")]
    [InlineData("li si")]
    [InlineData("li.si")]
    public void Validate_UsernameContainsInvalidCharacter_ReportsInvalidCharacterViolation(string username)
    {
        var violations = policy.Validate(username);

        Assert.Contains(violations, v => v.Rule == PolicyRule.UsernameHasInvalidCharacter);
    }

    [Theory]
    [InlineData("bob")]          // 恰好是最小长度 3
    [InlineData("alice_99")]
    [InlineData("ZhangSan")]
    public void Validate_ValidUsername_ReportsNoViolations(string username)
    {
        var violations = policy.Validate(username);

        Assert.Empty(violations);
    }

    [Fact(DisplayName = "用户名长度恰好等于上限 20 时应视为合法")]
    public void Validate_UsernameAtMaxLength_ReportsNoViolations()
    {
        var username = new string('a', UsernamePolicy.MaxLength);

        var violations = policy.Validate(username);

        Assert.Empty(violations);
    }
}
