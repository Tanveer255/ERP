using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

/// <summary>
/// edge_auth_db.user
/// </summary>
[Table(nameof(User))]
public class User : IdentityUser<Guid>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override Guid Id { get; set; }
    [Required, MaxLength(20)]
    public string TenantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Status { get; set; }
    public string UserType { get; set; }
    public bool IsTermsAgreed { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CountryCode { get; set; }
    public DateTime? LastActivity { get; set; }
    public User()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        LastActivity = DateTime.UtcNow;
    }
}