using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

/// <summary>
///  edge_auth_db.reset_password_token
/// </summary>
[Table(nameof(UserToken))]
public class UserToken : IdentityUserToken<Guid>
{
    [Key]
    public override Guid UserId { get; set; }
    public override string Value { get; set; }
    public override string Name { get; set; }
    public override string LoginProvider { get; set; }
    public DateTime ExpiryDate { get; set; }
}
