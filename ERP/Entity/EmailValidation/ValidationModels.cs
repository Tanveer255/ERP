namespace ERP.Entity.EmailValidation;

public class Request
{
    public string email { get; set; } = string.Empty;
    public string source { get; set; } = string.Empty;
}

public class Response
{
    public ValidationResult result { get; set; }
}

public class ValidationResult
{
    public string verdict { get; set; }
    public ValidationChecks checks { get; set; }
}

public class ValidationChecks
{
    public DomainChecks domain { get; set; }
    public AdditionalChecks additional { get; set; }
}

public class DomainChecks
{
    public bool has_valid_address_syntax { get; set; }
    public bool has_mx_or_a_record { get; set; }
    public bool is_suspected_disposable_address { get; set; }
}

public class AdditionalChecks
{
    public bool has_known_bounces { get; set; }
    public bool has_suspected_bounces { get; set; }
}
