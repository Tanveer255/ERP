using System.ComponentModel.DataAnnotations;

namespace ERP.Infrastructure;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSizeInBytes;

    public MaxFileSizeAttribute(int maxFileSizeInBytes)
    {
        _maxFileSizeInBytes = maxFileSizeInBytes;
        ErrorMessage = $"Maximum allowed file size is {maxFileSizeInBytes / 1024 / 1024} MB.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is IFormFile file && file.Length > _maxFileSizeInBytes)
        {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
