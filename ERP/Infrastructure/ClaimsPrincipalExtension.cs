using System.Globalization;
using System.Security.Claims;

namespace ERP.Infrastructure;

public static class ClaimsPrincipalExtension
{
    public static string? GetTenantId(this ClaimsPrincipal user)
    {
        var tenantId = user.FindFirst(x => x.Type == "TenantId")?.Value;
        return tenantId;
    }
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
            return Guid.Empty;

        return Guid.TryParse(claim.Value, out var guid) ? guid : Guid.Empty;
    }

    public static Guid GetSettingId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirst(x => x.Type.Equals("SettingId", StringComparison.OrdinalIgnoreCase));
        if (claim == null)
            return Guid.Empty;

        return Guid.TryParse(claim.Value, out var guid) ? guid : Guid.Empty;
    }


    public static string? GetShopDomain(this ClaimsPrincipal user)
    {
        string shopDomain = string.Empty;
        var shopUrl = user.FindFirst(x => x.Type == "dest")?.Value;
        if (shopUrl is not null)
        {
            shopDomain = shopUrl.Replace("https://", "");
        }
        return shopDomain;
    }

    public static string? GetShopUrl(this ClaimsPrincipal user)
    {
        return user.FindFirst(x => x.Type == "dest")?.Value;
    }

    public static string? GetShopName(this ClaimsPrincipal user)
    {
        string shopName = string.Empty;
        var shopUrl = user.FindFirst(x => x.Type == "dest")?.Value;
        if (shopUrl is not null)
        {
            shopName = shopUrl.Replace("https://", "").Split(".")[0];
        }

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(shopName);
    }
}
