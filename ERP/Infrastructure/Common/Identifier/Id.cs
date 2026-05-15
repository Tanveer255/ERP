namespace ERP.Infrastructure.Common.Identifier;

/// <summary>
/// This is the Id class which holds all data related to Id and inherits all properties of AggregateIdentifier has some properties given below.
/// </summary>
public class Id : AggregateIdentifier
{
    public Id(String id)
        : base(id)
    {
    }

    public Id()
    {
    }

    public static Id NewId()
    {
        return new Id(Guid.NewGuid().ToString());
    }
}
