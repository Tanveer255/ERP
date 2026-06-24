namespace ERP.Data.DTO.Auth;

/// <summary>
/// Represents application settings loaded from configuration.
/// </summary>
public class AppSetting
{
    public static string Environment { get; set; }
    public static string KnownProxiesUrl { get; set; }
    public static string ConnectionString { get; set; }
    public static string ShipStationOrderCreateUpdate { get; set; }
    public static string ShopifyBuildaBoxStoreUrl { get; set; }
    public static string ToEmailForPPCRequest { get; set; }
    public static string OracleConfigurationDirectory { get; set; }
    public static IConfigurationSection IpRateLimiting { get; set; }
    public static string ReCaptchaSiteKey { get; set; }
    public static string ReCaptchaSecretKey { get; set; }
    public static string ReCaptchaMapAPI { get; set; }
    public static string ReCaptchaMapUrl { get; set; }
    public static string ReCaptchaUrl { get; set; }
    public static string ShopifyApiSecretKey { get; set; }
    public static string WooCommerceApiKey { get; set; }
    public static string WooCommerceApiSecretKey { get; set; }
    public static string DHLRateUrl { get; set; }
    public static string FedExAuthUrl { get; set; }
    public static string FedExRateUrl { get; set; }
    public static string UpsRateUrl { get; set; }
    public static string WooCommerceAppName { get; set; }
    public static string MagentoAppName { get; set; }
    public static string MagentoApiKey { get; set; }
    public static string MagentoApiSecretKey { get; set; }
    public static string MagentoScopes { get; set; }
    public static string baseUrl { get; set; }
    public static string WooCommerceScopes { get; set; }
    public static string MaxMindUrl { get; set; }
    public static string MaxMindUserId { get; set; }
    public static string MaxMindLicenseKey { get; set; }
    public static string UIUrl { get; set; }
    public string SupportEmail { get; set; }
    public string Support { get => SupportEmail; set => SupportEmail = value; }
    public bool EmailValidationEnabled { get; set; }
    public IConfigurationSection Saml2Section { get; set; }
    public static string Saml2IdPMetadata { get; set; }
    public static string Saml2Issuer { get; set; }
    public static string Saml2SignatureAlgorithm { get; set; }
    public static string Saml2SigningCertificateFile { get; set; }
    public static string Saml2SigningCertificatePassword { get; set; }
    public static string Saml2CertificateValidationMode { get; set; }
    public static string Saml2RevocationMode { get; set; }
    public static string Saml2RelayStateReturnUrl { get; set; }
    public StripeSettings StripeSettings { get; set; }
    public FedExSettings FedExSettings { get; set; }
    public CalculatorApiCredentials CalculatorApiSettings { get; set; }
    public CalculatorApiCredentials DefaultDutyTaxApiCredentials { get; set; }
    public SendGridSettings SendGridSettings { get; set; }
    public ApplicationSettings ApplicationSettings { get; set; }
    public ShopifySettings ShopifySettings { get; set; }
    public BigCommerceSettings BigCommerceSettings { get; set; }
    public FxRatesSettings FxRates { get; set; }
    public DhlSettings DhlSettings { get; set; }
    public UpsSettings UpsSettings { get; set; }
    public BackgroundJobsSettings BackgroundJobsSettings { get; set; }

}
public class CalculatorApiCredentials
{
    public string Url { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
public class DeniedPartyScreeningSettings
{
    public string Url { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
public class DefaultDutyTaxApiCredentials
{
    public string Url { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
}
public class SendGridSettings
{
    public string EmailApiKey { get; set; }
    public string ValidationApiKey { get; set; }
    public string ValidationUrl { get; set; } = "https://api.sendgrid.com/v3/validations/email";
    public string ToEmail { get; set; }
    public string CCEmail { get; set; }
    public string AttachmentDisposition { get; set; }
    public string TestingCCEmail { get; set; }
    public string TestingToEmail { get; set; }
    public string FromEmail { get; set; }
    public string ToSupportEmail { get => ToEmail; set => ToEmail = value; }
    public string AttachmentFileName { get; set; }
    public string AttachmentType { get; set; }
}
public class ApplicationSettings
{
    public string RunningUrl { get; set; }
    public string Url { get; set; }
    public string FraudValidationEnabled { get; set; }
    public bool EmailValidationEnabled { get; set; }
    public bool CPCDummyResponse { get; set; }
    public bool DPSDummyResponse { get; set; }
    public string SupportEmail { get; set; }
    public string ServicesInterfaceUrl { get; set; }
    public string UIUrl { get; set; }
    public string UiUrl { get => UIUrl; set => UIUrl = value; }
    public string Support { get => SupportEmail; set => SupportEmail = value; }
    public string StaffUiUrl { get; set; }
    public string DevUrl { get; set; }
    public string Environment { get; set; }
    public string SandBoxEnvironment { get; set; }
    public string EncryptionKey { get; set; }
    public string OcelotGatewayURL { get; set; }
}
public class BigCommerceSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string ShippingCarrierId { get; set; }
    public string FrameAncestorsUrl { get; set; }
    public string StoreUrl { get; set; }
    public string TokenEndpoint { get; set; }
}
public class ShopifySettings
{
    public string AppName { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecretKey { get; set; }
    public string ApiVersion { get; set; }
    public string IsTestPayment { get; set; }
    public string Scopes { get; set; }
    public string WebhookCallBackURL { get; set; }
    public string GraphqlAddress { get; set; }
    public string RestAddress { get; set; }
    public string FrameAncestorsUrl { get; set; }
}
public class StripeSettings
{
    public string PublishableKey { get; set; }
    public string SecretKey { get; set; }
}
public class FedExSettings
{
    public bool UseSandbox { get; set; }
    public string LiveAuthUrl { get; set; }
    public string SandboxAuthUrl { get; set; }
    public string SandboxUrl { get; set; }
    public string LiveUrl { get; set; }
    public string AuthUrl => UseSandbox ? SandboxAuthUrl : LiveAuthUrl;
    public string BaseUrl => UseSandbox ? SandboxUrl : LiveUrl;
    public string FedExAuthUrl { get; set; }
    public string FedExRateUrl { get; set; }
}
public class CommodityPreClassificationSettings
{
    public string OAuthTokenUrl { get; set; }
    public string Url { get; set; }
    public string CheckStatusUrl { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string CustomerUUId { get; set; }
    public string CustomerAuthToken { get; set; }
    public string TargetSystemId { get; set; }
    public string WorkOrderToken { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}

public class FxRatesSettings
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public List<string> RunTimesUtc { get; set; } = new();
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMinutes { get; set; } = 2;
    public bool Enabled { get; set; }
}

public class DhlSettings
{
    public string SandboxUrl { get; set; }
    public string ProductionUrl { get; set; }
}

public class UpsSettings
{
    public string SandboxUrl { get; set; }
    public string ProductionUrl { get; set; }
}

public sealed class UpdateClassifyStatusJobSettings
{
    public bool Enabled { get; set; } = true;

    // Daily run time in UTC
    public int RunHourUtc { get; set; } = 0;    // 0 = midnight UTC
    public int RunMinuteUtc { get; set; } = 10; // 00:10 UTC
}
public sealed class ShopifyMonthlyimportJobSettings
{
    public bool Enabled { get; set; } = true;

    // Daily run time in UTC
    public int RunHourUtc { get; set; } = 0;    // 0 = midnight UTC
    public int RunMinuteUtc { get; set; } = 10; // 00:10 UTC
}
public class BackgroundJobsSettings
{
    public UpdateClassifyStatusJobSettings UpdateClassifyStatusJobSettings { get; set; }
}
public class FileSettings
{
    public List<string> ValidImageContentTypes { get; set; }
}
