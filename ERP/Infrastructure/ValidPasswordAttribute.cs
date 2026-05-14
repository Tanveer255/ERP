using System.ComponentModel.DataAnnotations;

namespace ERP.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidPasswordAttribute : ValidationAttribute
{
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 100;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;

    public ValidPasswordAttribute()
    {
        ErrorMessage = $"The {{0}} must be {MinLength}-{MaxLength} characters with complexity requirements.";
    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult($"The {validationContext.DisplayName} is required.");
        }

        string password = value.ToString()!;

        if (password.Length < MinLength)
        {
            return new ValidationResult(
                $"The {validationContext.DisplayName} must be at least {MinLength} characters long.");
        }

        if (password.Length > MaxLength)
        {
            return new ValidationResult(
                $"The {validationContext.DisplayName} cannot exceed {MaxLength} characters.");
        }

        var errors = new List<string>();

        if (RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("uppercase letter (A-Z)");
        }

        if (RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("lowercase letter (a-z)");
        }

        if (RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("number (0-9)");
        }

        if (RequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            errors.Add("special character");
        }

        if (errors.Count > 0)
        {
            var requirements = string.Join(", ", errors);
            return new ValidationResult(
                $"The {validationContext.DisplayName} must contain at least one {requirements}.");
        }

        return ValidationResult.Success!;
    }
}

