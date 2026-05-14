using System.ComponentModel.DataAnnotations;

namespace ERP.Infrastructure;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ValidEmailAttribute : ValidationAttribute
{
    public bool Required { get; set; } = true;
    public bool RequireNormalized { get; set; } = true;
    public int MinLength { get; set; } = 5;
    public int MaxLength { get; set; } = 254;
    public bool AutoTrim { get; set; } = true;
    public bool AutoNormalize { get; set; } = false;

    public ValidEmailAttribute()
    {
        ErrorMessage = "The {0} must be a valid email address.";
    }
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        // Handle null/empty
        if (Required && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
        {
            return new ValidationResult($"The {validationContext.DisplayName} is required.");
        }

        string email = value?.ToString() ?? string.Empty;

        // Auto-trim whitespace
        if (AutoTrim)
        {
            email = email.Trim();
        }

        // Length validation
        if (email.Length < MinLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} must be at least {MinLength} characters.");
        }

        if (email.Length > MaxLength)
        {
            return new ValidationResult($"The {validationContext.DisplayName} cannot exceed {MaxLength} characters.");
        }

        // Email format validation
        if (!new EmailAddressAttribute().IsValid(email))
        {
            return new ValidationResult($"The {validationContext.DisplayName} is not a valid email address.");
        }

        // Normalization checks
        if (RequireNormalized && email.Contains('+'))
        {
            if (AutoNormalize)
            {
                email = NormalizeEmail(email);
                UpdateValue(validationContext, email);
            }
            else
            {
                return new ValidationResult($"The {validationContext.DisplayName} cannot contain +aliases.");
            }
        }

        return ValidationResult.Success!;
    }

    private static string NormalizeEmail(string email)
    {
        int plusIndex = email.IndexOf('+');
        if (plusIndex >= 0)
        {
            string localPart = email.Substring(0, plusIndex);
            string domainPart = email.Substring(email.IndexOf('@'));
            return localPart + domainPart;
        }
        return email;
    }

    private void UpdateValue(ValidationContext context, string newValue)
    {
        var property = context.ObjectType.GetProperty(context.MemberName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(context.ObjectInstance, newValue);
        }
    }
}

