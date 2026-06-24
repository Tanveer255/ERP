using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ERP.Service.Auth;

public interface IRecaptchaService
{
    Task<bool> VerifyAsync(string token);
}

public class RecaptchaService : IRecaptchaService
{
    private readonly ReCaptchaSettings _reCaptchaSettings;
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _environment;

    public RecaptchaService(
        IOptions<ReCaptchaSettings> reCaptchaSettings,
        HttpClient httpClient,
        IWebHostEnvironment environment)
    {
        _reCaptchaSettings = reCaptchaSettings.Value;
        _httpClient = httpClient;
        _environment = environment;
    }

    public async Task<bool> VerifyAsync(string token)
    {
        if (_environment.IsDevelopment() && (string.IsNullOrWhiteSpace(token) || token == "dev-bypass"))
            return true;

        var response = await _httpClient.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={_reCaptchaSettings.SecretKey}&response={token}",
            null);

        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<RecaptchaVerificationResult>(json);

        return result.success;
    }
}
public class RecaptchaVerificationResult
{
    public bool success { get; set; }
    public float score { get; set; }
    public string action { get; set; }
    public string challenge_ts { get; set; }
    public string hostname { get; set; }
    public string[] error_codes { get; set; }
}
public class ReCaptchaSettings
{
    public string SecretKey { get; set; }
    public string SiteKey { get; set; }
}

