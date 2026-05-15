using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

[Table(nameof(UserClaim))]
public class UserClaim : IdentityUserClaim<Guid>
{
    public override int Id { get; set; }
    public override string ClaimType { get; set; }
    public override string ClaimValue { get; set; }

    [Key]
    public override Guid UserId { get; set; }
}
