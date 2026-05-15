using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

[Table(nameof(UserLogin))]
public class UserLogin : IdentityUserLogin<Guid>
{
    public override string LoginProvider { get; set; }
    public override string ProviderKey { get; set; }
    public override string ProviderDisplayName { get; set; }

    [Key]
    public override Guid UserId { get; set; }
}
