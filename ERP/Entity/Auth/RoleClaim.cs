using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

[Table(nameof(RoleClaim))]
public class RoleClaim : IdentityRoleClaim<Guid>
{
    public override int Id { get; set; }
    public override string ClaimType { get; set; }
    public override string ClaimValue { get; set; }

    [Key]
    public override Guid RoleId { get; set; }
}
