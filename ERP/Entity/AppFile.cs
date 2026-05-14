using System.ComponentModel.DataAnnotations;

namespace ERP.Entity;

public class AppFile : BaseEntity
{

    public string Data { get; set; }
    public string Type { get; set; } // MIME type like "image/jpeg"
    public string Name { get; set; } // Original file name
    public string AttachmentFileName { get; set; } // Saved file name
    public Guid? UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public bool IsLogoChanged { get; set; }
    public string AttachmentType { get; set; }
    [Required, MaxLength(20)]
    public string TenantId { get; set; } = string.Empty;
}
