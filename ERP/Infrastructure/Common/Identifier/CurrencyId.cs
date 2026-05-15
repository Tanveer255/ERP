namespace ERP.Infrastructure.Common.Identifier;

public class CurrencyId : Id
{
    public CurrencyId(String id)
        : base(id.ToUpper())
    {
    }

    public CurrencyId()
    {
    }
}