using System.ComponentModel.DataAnnotations;

namespace ERP.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrimAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is string stringValue)
        {
            var trimmedValue = stringValue.Trim();

            var property = validationContext.ObjectType.GetProperty(validationContext.MemberName!);
            if (property != null && property.CanWrite)
            {
                property.SetValue(validationContext.ObjectInstance, trimmedValue);
            }
        }

        return ValidationResult.Success!;
    }
}
