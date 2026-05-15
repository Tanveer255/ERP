using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

/// <summary>
/// edge_auth_db.tenant
/// </summary>
[Table(nameof(Tenant))]
public class Tenant : BaseEntity
{
    [Required, MaxLength(20)]
    public string TenantId { get; set; }
    public string TenantName { get; set; }
    public string TenantType { get; set; }
    public string TenantStatus { get; set; }
}
