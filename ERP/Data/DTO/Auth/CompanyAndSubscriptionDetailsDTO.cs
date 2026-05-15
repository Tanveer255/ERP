namespace ERP.Data.DTO.Auth;
public class SettlementSubscriptionReportDTO
{
    public string TenantId { get; set; }
    public string Email { get; set; }
    public string CompanyName { get; set; }
    public DateTime? DateTime { get; set; }
    public DateTime? PaymentTimeStamp { get; set; }
    public string ServiceType { get; set; }
    public decimal PayableAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; }
}

public class SettlementTransactionsAndSummaryReportDTO
{
    public string TenantId { get; set; }
    public string Email { get; set; }
    public string CompanyName { get; set; }
    public DateTime? DateTime { get; set; }
    public DateTime? PaymentTimeStamp { get; set; }
    public string ServiceType { get; set; }
    public decimal ServicePrice { get; set; }

    public int CDTBroughtForwardNumberOfTransactions { get; set; }
    public decimal CDTBroughtForwardUsd { get; set; }
    public int CDTPayableNumberOfTransactions { get; set; }
    public decimal CDTPayableUsd { get; set; }
    public int CDTPaidNumberOfTransactions { get; set; }
    public decimal CDTPaidInPeriodUsd { get; set; }
    public int CDTUnpaidCarriedForwardNumberOfTransactions { get; set; }
    public decimal CDTUnpaidCarriedForwardUsd { get; set; }

    public int DPSBroughtForwardNumberOfTransactions { get; set; }
    public decimal DPSBroughtForwardUsd { get; set; }
    public int DPSPayableNumberOfTransactions { get; set; }
    public decimal DPSPayableUsd { get; set; }
    public int DPSPaidNumberOfTransactions { get; set; }
    public decimal DPSPaidInPeriodUsd { get; set; }
    public int DPSUnpaidCarriedForwardNumberOfTransactions { get; set; }
    public decimal DPSUnpaidCarriedForwardUsd { get; set; }

    public int CPCLeadBroughtForwardNumberOfTransactions { get; set; }
    public decimal CPCLeadBroughtForwardUsd { get; set; }
    public int CPCLeadPayableNumberOfTransactions { get; set; }
    public decimal CPCLeadPayableUsd { get; set; }
    public int CPCLeadPaidNumberOfTransactions { get; set; }
    public decimal CPCLeadPaidInPeriodUsd { get; set; }
    public int CPCLeadUnpaidCarriedForwardNumberOfTransactions { get; set; }
    public decimal CPCLeadUnpaidCarriedForwardUsd { get; set; }

    public int CPCSubsequentBroughtForwardNumberOfTransactions { get; set; }
    public decimal CPCSubsequentBroughtForwardUsd { get; set; }
    public int CPCSubsequentPayableNumberOfTransactions { get; set; }
    public decimal CPCSubsequentPayableUsd { get; set; }
    public int CPCSubsequentPaidNumberOfTransactions { get; set; }
    public decimal CPCSubsequentPaidInPeriodUsd { get; set; }
    public int CPCSubsequentUnpaidCarriedForwardNumberOfTransactions { get; set; }
    public decimal CPCSubsequentUnpaidCarriedForwardUsd { get; set; }

    public int CPCUploadLeadBroughtForwardNumberOfTransactions { get; set; }
    public decimal CPCUploadLeadBroughtForwardUsd { get; set; }
    public int CPCUploadLeadPayableNumberOfTransactions { get; set; }
    public decimal CPCUploadLeadPayableUsd { get; set; }
    public int CPCUploadLeadPaidNumberOfTransactions { get; set; }
    public decimal CPCUploadLeadPaidInPeriodUsd { get; set; }
    public int CPCUploadLeadUnpaidCarriedForwardNumberOfTransactions { get; set; }
    public decimal CPCUploadLeadUnpaidCarriedForwardUsd { get; set; }

    public int CPCUploadSubsequentBroughtForwardNumberOfTransactions { get; set; }
    public decimal CPCUploadSubsequentBroughtForwardUsd { get; set; }
    public int CPCUploadSubsequentPayableNumberOfTransactions { get; set; }
    public decimal CPCUploadSubsequentPayableUsd { get; set; }
    public int CPCUploadSubsequentPaidNumberOfTransactions { get; set; }
    public decimal CPCUploadSubsequentPaidInPeriodUsd { get; set; }
    public int CPCUploadSubsequentUnpaidCarriedForwardNumberOfTransactions { get; set; }
    public decimal CPCUploadSubsequentUnpaidCarriedForwardUsd { get; set; }

    public int SubscriptionBroughtForwardNumberOfTransactions { get; set; }
    public decimal SubscriptionBroughtForwardUsd { get; set; }
    public int SubscriptionPayableNumberOfTransactions { get; set; }
    public decimal SubscriptionPayableUsd { get; set; }
    public int SubscriptionPaidNumberOfTransactions { get; set; }
    public decimal SubscriptionPaidInPeriodUsd { get; set; }
    public int SubscriptionUnpaidCarriedForwardNumberOfTransactions { get; set; }
    public decimal SubscriptionUnpaidCarriedForwardUsd { get; set; }
}

public class ChargeableReportDTO
{
    public List<ChargeableReportContent> List { get; set; }

    public int TotalSubscriptionCount { get; set; }
    public decimal TotalSubscriptionChargeable { get; set; }
    public int TotalCPCLeadTotalCountChargeable { get; set; }
    public decimal TotalCPCLeadTotalChargeable { get; set; }
    public int TotalCPCSubTotalCountChargeable { get; set; }
    public decimal TotalCPCSubTotalChargeable { get; set; }
    public int TotalCDTTotalCountChargeable { get; set; }
    public decimal TotalCDTTotalChargeable { get; set; }
    public int TotalDPSTotalCountChargeable { get; set; }
    public decimal TotalDPSTotalChargeable { get; set; }
    public decimal NetChargeableAmount { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal TotalBalance { get; set; }
}

public class ChargeableReportContent
{
    public string TenantId { get; set; }
    public string Company { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string TownLocality { get; set; }
    public string CityRegion { get; set; }
    public string State { get; set; }
    public string Country { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string CustomerStatus { get; set; }

    public int SubscriptionTotalCount { get; set; }
    public decimal SubscriptionChargeable { get; set; }

    public int CPCLeadTotalCountChargeable { get; set; }
    public decimal CPCLeadTotalChargeable { get; set; }

    public int CPCSubTotalCountChargeable { get; set; }
    public decimal CPCSubTotalChargeable { get; set; }

    public int CDTTotalCountChargeable { get; set; }
    public decimal CDTTotalChargeable { get; set; }

    public int DPSTotalCountChargeable { get; set; }
    public decimal DPSTotalChargeable { get; set; }

    public decimal TotalChargeable { get; set; }
    public decimal Commission { get; set; }
    public decimal Balance { get; set; }
}

public class CompanyAndSubscriptionDetailsDTO
{
    public string TenantId { get; set; }
    public string Email { get; set; }
    public string CompanyName { get; set; }
}

public class AddressAndSubscriptionDetailsDTO
{
    public string TenantId { get; set; }
    public string CompanyName { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string TownLocality { get; set; }
    public string CityRegion { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
}

