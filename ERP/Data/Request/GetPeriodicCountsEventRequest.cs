namespace ERP.Data.Request;

public class GetPeriodicCountsEventRequest
{
    public PeriodEventType PeriodEventType { get; set; }
    //[AllowedDuration]
    public int Duration { get; set; }
    public Guid UserId { get; set; }

}
public enum PeriodEventType
{
    Week,
    Month
}
public class GetPeriodicCountsEventResponse
{
    public string Period { get; set; } = string.Empty;
    public int Value { get; set; }
}

