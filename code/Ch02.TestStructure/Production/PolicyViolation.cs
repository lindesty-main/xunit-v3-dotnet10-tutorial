namespace Ch02.TestStructure.Production;

/// <summary>策略校验器可能违反的规则种类。</summary>
public enum PolicyRule
{
    UsernameRequired,
    UsernameTooShort,
    UsernameTooLong,
    UsernameMustStartWithLetter,
    UsernameHasInvalidCharacter,
    PasswordTooShort,
    PasswordMissingLowercase,
    PasswordMissingUppercase,
    PasswordMissingDigit,
    PasswordContainsUsername,
}

/// <summary>一条违规:违反了哪条规则,以及给人看的说明文字。</summary>
public sealed record PolicyViolation(PolicyRule Rule, string Message);
