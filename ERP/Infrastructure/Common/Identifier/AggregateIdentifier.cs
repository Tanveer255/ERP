namespace ERP.Infrastructure.Common.Identifier;

/// <summary>
/// This is the AggregateIdentifier class which holds all data related to AggregateIdentifier and has some properties given below.
/// </summary>
public class AggregateIdentifier
{
    private String id;

    public AggregateIdentifier()
    {
    }

    public AggregateIdentifier(String id)
    {
        this.id = id;
    }

    public String AsString()
    {
        if (!String.IsNullOrEmpty(this.id))
        {
            return this.id;
        }

        return String.Empty;
    }

    public string ID
    {
        get { return this.id; }
        set { this.id = value; }
    }

    public override bool Equals(object obj)
    {
        if (obj != null && this.GetType().Name.Equals(obj.GetType().Name))
        {
            if (obj is AggregateIdentifier)
            {
                return this.AsString().ToLower().Equals(((AggregateIdentifier)obj).AsString().ToLower());
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.AsString().ToLower().GetHashCode();
    }

    /// <summary>
    /// Handles Null, Empty and whitespace Ids.
    /// </summary>
    /// <returns>True if Id is not null, emtpy or whitespace.</returns>
    public bool NotNullOrEmpty()
    {
        if (this != null &&
            !String.IsNullOrEmpty(this.AsString()) &&
            !String.IsNullOrWhiteSpace(this.AsString()))
        {
            return true;
        }

        return false;
    }
}
