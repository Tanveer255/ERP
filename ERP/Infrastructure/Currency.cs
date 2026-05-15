using ERP.Infrastructure.Common.Identifier;

namespace ERP.Infrastructure;

/// <summary>
/// This is the Currency sealed class which holds all data related to Currency and has some properties given below.
/// </summary>
public sealed class Currency
{
    // Western Union Currency
    public static readonly Currency AED = Currency.CreateFrom("AED", "United Arab Emirates Dirham", "د.إ.‏");
    public static readonly Currency ALL = Currency.CreateFrom("ALL", "Albanian Lek", "Lekë");
    public static readonly Currency AMD = Currency.CreateFrom("AMD", "Armenian Dram", "֏");
    public static readonly Currency AOA = Currency.CreateFrom("AOA", "Angolan Kwanza", "Kz");
    public static readonly Currency ARS = Currency.CreateFrom("ARS", "Argentine Peso", "$");
    public static readonly Currency AUD = Currency.CreateFrom("AUD", "Australian Dollar", "$");
    public static readonly Currency AZN = Currency.CreateFrom("AZN", "Azerbaijani Manat", "₼");
    public static readonly Currency BAM = Currency.CreateFrom("BAM", "Bosnia And Herzegovina Convertible Mark", "КМ");
    public static readonly Currency BBD = Currency.CreateFrom("BBD", "Barbados Dollar", "$");
    public static readonly Currency BDT = Currency.CreateFrom("BDT", "Bangladeshi Taka", "৳");
    public static readonly Currency BGN = Currency.CreateFrom("BGN", "Bulgarian Lev", "лв.");
    public static readonly Currency BHD = Currency.CreateFrom("BHD", "Bahraini Dinar", "د.ب.‏");
    public static readonly Currency BIF = Currency.CreateFrom("BIF", "Burundian Franc", "FBu");
    public static readonly Currency BMD = Currency.CreateFrom("BMD", "Bermudian Dollar", "$");
    public static readonly Currency BND = Currency.CreateFrom("BND", "Brunei Dollar", "$");
    public static readonly Currency BOB = Currency.CreateFrom("BOB", "Boliviano", "Bs");
    public static readonly Currency BRL = Currency.CreateFrom("BRL", "Brazilian Real", "R$");
    public static readonly Currency BTN = Currency.CreateFrom("BTN", "Bhutanese Ngultrum", "Nu.");
    public static readonly Currency BWP = Currency.CreateFrom("BWP", "Botswana Pula", "P");
    public static readonly Currency BZD = Currency.CreateFrom("BZD", "Belize Dollar", "$");
    public static readonly Currency CAD = Currency.CreateFrom("CAD", "Canadian Dollar", "$");
    public static readonly Currency CHF = Currency.CreateFrom("CHF", "Swiss Franc", "CHF");
    public static readonly Currency CLP = Currency.CreateFrom("CLP", "Chilean Peso", "$");
    public static readonly Currency CNH = Currency.CreateFrom("CNH", "Chinese Yuan Renminbi", "");
    public static readonly Currency CNY = Currency.CreateFrom("CNY", "Renminbi Chinese Yuan", "¥");
    public static readonly Currency COP = Currency.CreateFrom("COP", "Colombian Peso", "$");
    public static readonly Currency CRC = Currency.CreateFrom("CRC", "Costa Rican Colon", "₡");
    public static readonly Currency CVE = Currency.CreateFrom("CVE", "Cape Verdean Escudo", "");
    public static readonly Currency CZK = Currency.CreateFrom("CZK", "Czech Koruna", "Kč");
    public static readonly Currency DJF = Currency.CreateFrom("DJF", "Djiboutian Franc", "Fdj");
    public static readonly Currency DKK = Currency.CreateFrom("DKK", "Danish Krone", "kr.");
    public static readonly Currency DOP = Currency.CreateFrom("DOP", "Dominican Peso", "$");
    public static readonly Currency DZD = Currency.CreateFrom("DZD", "Algerian Dinar", "د.ج.‏");
    public static readonly Currency EGP = Currency.CreateFrom("EGP", "Egyptian Pound", "ج.م.‏");
    public static readonly Currency ERN = Currency.CreateFrom("ERN", "Eritrean Nakfa", "Nfk");
    public static readonly Currency ETB = Currency.CreateFrom("ETB", "Ethiopian Birr", "Br");
    public static readonly Currency EUR = Currency.CreateFrom("EUR", "Euro", "€");
    public static readonly Currency FJD = Currency.CreateFrom("FJD", "Fiji Dollar", "$");
    public static readonly Currency GBP = Currency.CreateFrom("GBP", "Pound Sterling", "£");
    public static readonly Currency GEL = Currency.CreateFrom("GEL", "Georgian Lari", "₾");
    public static readonly Currency GHS = Currency.CreateFrom("GHS", "Ghanaian Cedi", "GH₵");
    public static readonly Currency GMD = Currency.CreateFrom("GMD", "Gambian Dalasi", "D");
    public static readonly Currency GNF = Currency.CreateFrom("GNF", "Guinean Franc", "FG");
    public static readonly Currency GTQ = Currency.CreateFrom("GTQ", "Guatemalan Quetzal", "Q");
    public static readonly Currency GYD = Currency.CreateFrom("GYD", "Guyanese Dollar", "$");
    public static readonly Currency HKD = Currency.CreateFrom("HKD", "Hong Kong Dollar", "$");
    public static readonly Currency HNL = Currency.CreateFrom("HNL", "Honduran Lempira", "L");
    public static readonly Currency HRK = Currency.CreateFrom("HRK", "Croatian Kuna", "kn");
    public static readonly Currency HTG = Currency.CreateFrom("HTG", "Haitian Gourde", "G");
    public static readonly Currency HUF = Currency.CreateFrom("HUF", "Hungarian Forint", "Ft");
    public static readonly Currency IDR = Currency.CreateFrom("IDR", "Indonesian Rupiah", "Rp");
    public static readonly Currency ILS = Currency.CreateFrom("ILS", "Israeli New Shekel", "₪");
    public static readonly Currency INR = Currency.CreateFrom("INR", "Indian Rupee", "₹");
    public static readonly Currency ISK = Currency.CreateFrom("ISK", "Icelandic Króna", "kr");
    public static readonly Currency JMD = Currency.CreateFrom("JMD", "Jamaican Dollar", "$");
    public static readonly Currency JOD = Currency.CreateFrom("JOD", "Jordanian Dinar", "د.ا.‏");
    public static readonly Currency JPY = Currency.CreateFrom("JPY", "Japanese Yen", "¥");
    public static readonly Currency KES = Currency.CreateFrom("KES", "Kenyan Shilling", "Ksh");
    public static readonly Currency KGS = Currency.CreateFrom("KGS", "Kyrgyzstani Som", "сом");
    public static readonly Currency KHR = Currency.CreateFrom("KHR", "Cambodian Riel", "៛");
    public static readonly Currency KMF = Currency.CreateFrom("KMF", "Comoro Franc", "CF");
    public static readonly Currency KRW = Currency.CreateFrom("KRW", "South Korean Won", "₩");
    public static readonly Currency KWD = Currency.CreateFrom("KWD", "Kuwaiti Dinar", "د.ك.‏");
    public static readonly Currency KYD = Currency.CreateFrom("KYD", "Cayman Islands Dollar", "$");
    public static readonly Currency KZT = Currency.CreateFrom("KZT", "Kazakhstani Tenge", "₸");
    public static readonly Currency LAK = Currency.CreateFrom("LAK", "Lao Kip", "₭");
    public static readonly Currency LBP = Currency.CreateFrom("LBP", "Lebanese Pound", "ل.ل.‏");
    public static readonly Currency LKR = Currency.CreateFrom("LKR", "Sri Lankan Rupee", "රු.");
    public static readonly Currency LSL = Currency.CreateFrom("LSL", "Lesotho Loti", "");
    public static readonly Currency MAD = Currency.CreateFrom("MAD", "Moroccan Dirham", "د.م.‏");
    public static readonly Currency MGA = Currency.CreateFrom("MGA", "Malagasy Ariary", "Ar");
    public static readonly Currency MKD = Currency.CreateFrom("MKD", "Macedonian Denar", "ден");
    public static readonly Currency MOP = Currency.CreateFrom("MOP", "Macanese Pataca", "MOP$");
    public static readonly Currency MUR = Currency.CreateFrom("MUR", "Mauritian Rupee", "Rs");
    public static readonly Currency MVR = Currency.CreateFrom("MVR", "Maldivian Rufiyaa", "ރ.");
    public static readonly Currency MWK = Currency.CreateFrom("MWK", "Malawian Kwacha", "MK");
    public static readonly Currency MXN = Currency.CreateFrom("MXN", "Mexican Peso", "$");
    public static readonly Currency MYR = Currency.CreateFrom("MYR", "Malaysian Ringgit", "RM");
    public static readonly Currency MZN = Currency.CreateFrom("MZN", "Mozambican Metical", "MTn");
    public static readonly Currency NAD = Currency.CreateFrom("NAD", "Namibian Dollar", "$");
    public static readonly Currency NGN = Currency.CreateFrom("NGN", "Nigerian Naira", "₦");
    public static readonly Currency NIO = Currency.CreateFrom("NIO", "Nicaraguan Córdoba", "C$");
    public static readonly Currency NOK = Currency.CreateFrom("NOK", "Norwegian Krone", "kr");
    public static readonly Currency NPR = Currency.CreateFrom("NPR", "Nepalese Rupee", "रु");
    public static readonly Currency NZD = Currency.CreateFrom("NZD", "New Zealand Dollar", "$");
    public static readonly Currency OMR = Currency.CreateFrom("OMR", "Omani Rial", "ر.ع.‏");
    public static readonly Currency PAB = Currency.CreateFrom("PAB", "Panamanian Balboa", "B/.");
    public static readonly Currency PEN = Currency.CreateFrom("PEN", "Peruvian Sol", "S/");
    public static readonly Currency PGK = Currency.CreateFrom("PGK", "Papua New Guinean Kina", "K");
    public static readonly Currency PHP = Currency.CreateFrom("PHP", "Philippine Peso", "₱");
    public static readonly Currency PKR = Currency.CreateFrom("PKR", "Pakistani Rupee", "Rs");
    public static readonly Currency PLN = Currency.CreateFrom("PLN", "Polish Złoty", "zł");
    public static readonly Currency PYG = Currency.CreateFrom("PYG", "Paraguayan Guaraní", "₲");
    public static readonly Currency QAR = Currency.CreateFrom("QAR", "Qatari Riyal", "ر.ق.‏");
    public static readonly Currency RON = Currency.CreateFrom("RON", "Romanian Leu", "lei");
    public static readonly Currency RSD = Currency.CreateFrom("RSD", "Serbian Dinar", "дин.");
    public static readonly Currency RUB = Currency.CreateFrom("RUB", "Russian Ruble", "₽");
    public static readonly Currency RWF = Currency.CreateFrom("RWF", "Rwandan Franc", "RF");
    public static readonly Currency SAR = Currency.CreateFrom("SAR", "Saudi Riyal", "ر.س.‏");
    public static readonly Currency SBD = Currency.CreateFrom("SBD", "Solomon Islands Dollar", "$");
    public static readonly Currency SCR = Currency.CreateFrom("SCR", "Seychelles Rupee", "SR");
    public static readonly Currency SEK = Currency.CreateFrom("SEK", "Swedish Krona/kronor", "kr");
    public static readonly Currency SGD = Currency.CreateFrom("SGD", "Singapore Dollar", "$");
    public static readonly Currency SLL = Currency.CreateFrom("SLL", "Sierra Leonean Leone", "Le");
    public static readonly Currency SRD = Currency.CreateFrom("SRD", "Surinamese Dollar", "$");
    public static readonly Currency SZL = Currency.CreateFrom("SZL", "Swazi Lilangeni", "E");
    public static readonly Currency THB = Currency.CreateFrom("THB", "Thai Baht", "฿");
    public static readonly Currency TND = Currency.CreateFrom("TND", "Tunisian Dinar", "د.ت.‏");
    public static readonly Currency TOP = Currency.CreateFrom("TOP", "Tongan Paʻanga", "T$");
    public static readonly Currency TRY = Currency.CreateFrom("TRY", "Turkish Lira", "₺");
    public static readonly Currency TTD = Currency.CreateFrom("TTD", "Trinidad And Tobago Dollar", "$");
    public static readonly Currency TWD = Currency.CreateFrom("TWD", "New Taiwan Dollar", "NT$");
    public static readonly Currency TZS = Currency.CreateFrom("TZS", "Tanzanian Shilling", "TSh");
    public static readonly Currency UGX = Currency.CreateFrom("UGX", "Ugandan Shilling", "USh");
    public static readonly Currency USD = Currency.CreateFrom("USD", "United States Dollar", "$");
    public static readonly Currency UYU = Currency.CreateFrom("UYU", "Uruguayan Peso", "$");
    public static readonly Currency UZS = Currency.CreateFrom("UZS", "Uzbekistan Som", "сўм");
    public static readonly Currency VND = Currency.CreateFrom("VND", "Vietnamese Đồng", "₫");
    public static readonly Currency VUV = Currency.CreateFrom("VUV", "Vanuatu Vatu", "VT");
    public static readonly Currency WST = Currency.CreateFrom("WST", "Samoan Tala", "WS$");
    public static readonly Currency XAF = Currency.CreateFrom("XAF", "Cfa Franc Beac", "FCFA");
    public static readonly Currency XCD = Currency.CreateFrom("XCD", "East Caribbean Dollar", "EC$");
    public static readonly Currency XOF = Currency.CreateFrom("XOF", "Cfa Franc Bceao", "CFA");
    public static readonly Currency XPF = Currency.CreateFrom("XPF", "Cfp Franc (Franc Pacifique)", "FCFP");
    public static readonly Currency YER = Currency.CreateFrom("YER", "Yemeni Rial", "ر.ي.‏");
    public static readonly Currency ZAR = Currency.CreateFrom("ZAR", "South African Rand", "R");
    public static readonly Currency ZMW = Currency.CreateFrom("ZMW", "Zambian Kwacha", "K");
    public static readonly Currency EMPTY = Currency.CreateFrom(string.Empty, string.Empty, string.Empty);
    public static readonly Currency UNKNOWN = Currency.CreateFrom("UNKNOWN", "Unknown", "Unknown");
    private static Dictionary<CurrencyId, Currency> currencyDictionary = new Dictionary<CurrencyId, Currency>();
    private CurrencyId currencyId;
    private String currency;
    private String symbol;

    public Currency()
    {
    }

    /// <summary>
    /// Gets returns an enumeration of all currencies.
    /// </summary>
    /// commented because we do not need it now and it was generating error
    public static IEnumerable<Currency> Values
    {
        get
        {
            return currencyDictionary.Values;
        }
    }

    public Currency(CurrencyId currencyId, String currency, String symbol)
    {
        this.currencyId = new CurrencyId(currencyId.AsString().ToUpper());
        this.currency = currency;
        this.symbol = symbol;

        // no need for this, it initiates infinite loop.
        if (currencyDictionary == null)
        {
            currencyDictionary = new Dictionary<CurrencyId, Currency>();
        }

        if (!currencyId.AsString().Equals(string.Empty) && !currencyId.AsString().Equals("UNKNOWN") && !currencyDictionary.ContainsKey(currencyId))
        {
            currencyDictionary.Add(this.currencyId, this);
        }
    }

    public String IsoCode
    {
        get { return this.currencyId.AsString(); }
    }

    /// <summary>
    /// Gets or sets the official ISO currency name.
    /// </summary>
    public String CurrencyName
    {
        get { return this.currency; }
        set { this.currency = value; }
    }

    /// <summary>
    /// Gets or sets the official ISO 4217 currency codes.
    /// </summary>
    public CurrencyId CurrencyId
    {
        get { return this.currencyId; }
        set { this.currencyId = value; }
    }


    public string Symbol
    {
        get { return this.symbol; }
        set { this.symbol = value; }
    }

    /// <summary>
    /// Converts a 3 character ISO Code string into a Currency.
    /// This method will return null if no match is found.
    /// </summary>
    /// <param name="isoCode">3 charcater ISO currency code.</param>
    /// <returns>the equivalent Currency object.</returns>
    public static Currency ValueOf(String isoCode)
    {
        try
        {
            if (isoCode == null)
            {
                return null;
            }

            return currencyDictionary[new CurrencyId(isoCode.ToUpper())];
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Currency ValueOf(CurrencyId currencyId)
    {
        try
        {
            if (currencyId == null)
            {
                return Currency.UNKNOWN;
            }

            return currencyDictionary[currencyId];
        }
        catch (Exception)
        {
            return Currency.UNKNOWN;
        }
    }

    public static Currency GetCountry(String currencyName)
    {
        foreach (KeyValuePair<CurrencyId, Currency> pair in currencyDictionary)
        {
            if (pair.Value.CurrencyName.ToLower().Trim().Equals(currencyName.ToLower().Trim()))
            {
                return pair.Value;
            }
        }

        return Currency.UNKNOWN;
    }

    public static Currency CreateFrom(String isoCode, String name, string symbol)
    {
        return new Currency(new CurrencyId(isoCode), name, symbol);
    }
}