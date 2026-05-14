using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ERP.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidNameAttribute : ValidationAttribute
{
    public bool Required { get; set; } = true;
    public int MinLength { get; set; } = 2;
    public int MaxLength { get; set; } = 50;

    private static readonly Regex _validNameRegex = new(@"^(?!.*['’\-\s]{2,})(?!.*['’\-\s]$)[A-Za-zÀ-ÖØ-öø-ÿ]+(?:[ '\-’][A-Za-zÀ-ÖØ-öø-ÿ]+)*$", RegexOptions.Compiled);

    public ValidNameAttribute()
    {
        ErrorMessage = $"The {{0}} must be between {MinLength} and {MaxLength} characters.";
    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Handle null/empty (Required check)
        if (Required && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
        {
            return new ValidationResult($"The {validationContext.DisplayName} is required.");
        }

        string strValue = value?.ToString()?.Trim() ?? string.Empty;

        // Length validation
        if (strValue.Length < MinLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} must be at least {MinLength} characters long.");
        }

        if (strValue.Length > MaxLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} cannot exceed {MaxLength} characters.");
        }

        if (!_validNameRegex.IsMatch(strValue))
        {
            return new ValidationResult($"The {validationContext.DisplayName} can only contain letters, spaces, hyphens, or apostrophes (no numbers, symbols, or emojis).");
        }

        return ValidationResult.Success!;
    }
}
