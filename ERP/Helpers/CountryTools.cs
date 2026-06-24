namespace ERP.Helpers;

public static class CountryTools
{
    public static string GetCountryName(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return string.Empty;
        }

        return countryCode.Trim().ToUpperInvariant();
    }
}
