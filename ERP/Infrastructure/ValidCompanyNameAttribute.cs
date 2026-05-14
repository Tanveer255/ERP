using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ERP.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidCompanyNameAttribute : ValidationAttribute
{
    public bool Required { get; set; } = true;
    public int MinLength { get; set; } = 2;
    public int MaxLength { get; set; } = 100;

    private static readonly Regex _validNameRegex = new(@"^[A-Za-zÀ-ÖØ-öø-ÿ0-9 &.,\-'()@#]+$", RegexOptions.Compiled);

    private static readonly Regex _repeatedSpecialCharRegex = new(@"([&.,\-'()@#])\1", RegexOptions.Compiled);

    private static readonly char[] _unsafeChars = new[] { '<', '>', '?', '*', '%', '$', '^', '{', '}', '[', ']', '=', '+', '!', '/', '\\', '|', ':', ';', '"' };

    public ValidCompanyNameAttribute()
    {
        ErrorMessage = $"The {{0}} must be between {MinLength} and {MaxLength} characters and contain only allowed characters.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        string strValue = value?.ToString()?.Trim() ?? string.Empty;

        if (Required && string.IsNullOrWhiteSpace(strValue))
        {
            return new ValidationResult($"The {validationContext.DisplayName} is required.");
        }

        if (strValue.Length < MinLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} must be at least {MinLength} characters long.");
        }

        if (strValue.Length > MaxLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} cannot exceed {MaxLength} characters.");
        }

        foreach (var c in _unsafeChars)
        {
            if (strValue.Contains(c))
            {
                return new ValidationResult($"The {validationContext.DisplayName} contains invalid character '{c}'.");
            }
        }

        if (!_validNameRegex.IsMatch(strValue))
        {
            return new ValidationResult($"The {validationContext.DisplayName} contains invalid characters. Only letters, numbers, spaces, and & . , - ' ( ) @ # are allowed.");
        }

        if (_repeatedSpecialCharRegex.IsMatch(strValue))
        {
            return new ValidationResult($"The {validationContext.DisplayName} cannot contain repeated special characters like '&&' or '..'.");
        }

        return ValidationResult.Success!;
    }
}
