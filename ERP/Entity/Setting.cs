using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity;

[Table(nameof(Setting))]
public class Setting : BaseEntity
{
    [Required, MaxLength(20)]
    public string TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string ProcessUser { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string SelectedAddress { get; set; } = string.Empty;
    public string ProductExpiryDays { get; set; } = string.Empty;
    public bool IsSupportReq { get; set; } = false;
}
