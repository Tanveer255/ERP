using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ERP.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidCountryCodeAttribute : ValidationAttribute
{
    public bool Required { get; set; } = true;

    private const string Pattern = @"^\+(\d+(-\d+)*)$";

    public ValidCountryCodeAttribute()
    {
        ErrorMessage = "Invalid country code format.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return Required
                ? new ValidationResult($"{validationContext.DisplayName} is required.")
                : ValidationResult.Success;
        }

        var strValue = value.ToString()!.Trim();

        if (!Regex.IsMatch(strValue, Pattern))
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
