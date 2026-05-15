using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Entity.Auth;

/// <summary>
/// This is the Role class which holds all data related to Role and which inherits all properties of IdentityRole and has some properties given below.
/// </summary>
[Table("Role")]
public class Role : IdentityRole<Guid>
{
    [Column("ID")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override Guid Id { get; set; }

    [Column("NAME_NM")]
    public override string Name { get; set; }

    [Column("NORMALIZED_NM")]
    public override string NormalizedName { get; set; }

    [Column("CONCURRENCY_STAMP_DESC")]
    public override string ConcurrencyStamp { get; set; }

    public void Update(Role role)
    {
        Name = role.Name;
        NormalizedName = role.NormalizedName;
        ConcurrencyStamp = role.ConcurrencyStamp;
    }
}
