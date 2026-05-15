using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

/// <summary>
/// edge_auth_db.user_role
/// </summary>
[Table(nameof(UserRole))]
public class UserRole : IdentityUserRole<Guid>
{
    public string UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
