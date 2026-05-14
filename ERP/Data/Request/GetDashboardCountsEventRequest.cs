namespace ERP.Data.Request;

public class GetDashboardCountsEventRequest
{
    public Guid UserId { get; set; }
}
public class GetDashboardCountsEventResponse
{
    public int TotalCounts { get; set; }
    public int CurrentMonthCounts { get; set; }
    public double PercentageIncrease { get; set; }
}