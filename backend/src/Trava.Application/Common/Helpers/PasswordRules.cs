using FluentValidation;

namespace Trava.Application.Common.Helpers;

public static class PasswordRules
{
    /// <summary>
    /// Applies strong password rules:
    /// - At least 8 characters
    /// - At least one uppercase letter (A-Z)
    /// - At least one lowercase letter (a-z)
    /// - At least one digit (0-9)
    /// - At least one special character (!@#$%^&*...)
    /// </summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter (A-Z).")
            .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter (a-z).")
            .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one digit (0-9).")
            .Matches(@"[!@#$%^&*()_\-+=\[\]{};':""\\|,.<>\/?`~]")
                .WithMessage("Password must contain at least one special character (e.g. !@#$%^&*).");
    }
}
