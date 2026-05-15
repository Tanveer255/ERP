using ERP.Infrastructure;

namespace ERP.Data.Request;

public class ResetPasswordRequest
{
    [ValidPassword]
    public string Password { get; set; }
    public string Token { get; set; }
    [ValidEmail]
    public string Email { get; set; }
}
