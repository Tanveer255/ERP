using System.ComponentModel.DataAnnotations;

namespace ERP.Infrastructure.Attributes;

public class NotEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is Guid guidValue)
        {
            return guidValue != Guid.Empty;
        }
        return false; // Return false if value is null or not a Guid
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must not be an empty GUID.";
    }
}
