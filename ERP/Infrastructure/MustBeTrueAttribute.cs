using System.ComponentModel.DataAnnotations;

namespace ERP.Infrastructure;

public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        return value is bool boolValue && boolValue;
    }
}