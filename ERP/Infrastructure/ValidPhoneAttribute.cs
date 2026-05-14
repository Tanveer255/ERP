using System.ComponentModel.DataAnnotations;

namespace ERP.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidPhoneAttribute : ValidationAttribute
{
    public bool Required { get; set; } = true;
    public int MinLength { get; set; } = 9;
    public int MaxLength { get; set; } = 15;

    public ValidPhoneAttribute()
    {
        ErrorMessage = $"The {{0}} must be between {MinLength} and {MaxLength} digits.";
    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Handle null/empty (Required check)
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return Required
                ? new ValidationResult($"The {validationContext.DisplayName} is required.")
                : ValidationResult.Success;
        }

        string strValue = value?.ToString()?.Trim() ?? string.Empty;

        // Digits-only validation
        if (!System.Text.RegularExpressions.Regex.IsMatch(strValue, @"^\d+$"))
        {
            return new ValidationResult($"The {validationContext.DisplayName} must contain digits only.");
        }

        // Length validation
        if (strValue.Length < MinLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} must be at least {MinLength} digits long.");
        }

        if (strValue.Length > MaxLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} cannot exceed {MaxLength} digits.");
        }

        return ValidationResult.Success!;
    }
}
