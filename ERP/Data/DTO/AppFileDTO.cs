using ERP.Entity;
using System.IO.Compression;

namespace ERP.Data.DTO;

public record AppFileDTO
{
    public Guid Id { get; set; }
    public string Data { get; set; }
    public string Type { get; set; } // MIME type like "image/jpeg"
    public string Name { get; set; } // Original file name
    public string AttachmentFileName { get; set; } // Saved file name
    public Guid? UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public bool IsLogoChanged { get; set; }
    public string AttachmentType { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static AppFile MapDtoToModel(AppFileDTO dto)
    {
        return new AppFile
        {
            Id = dto.Id,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            IsDeleted = dto.IsDeleted,
            Data = dto.Data,
            Type = dto.Type,
            Name = dto.Name,
            AttachmentFileName = dto.AttachmentFileName,
            UserId = dto.UserId,
            CompanyId = dto.CompanyId,
            IsLogoChanged = dto.IsLogoChanged,
            AttachmentType = dto.AttachmentType
        };
    }
    public static AppFileDTO MapModelToDto(AppFile model)
    {
        return new AppFileDTO
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            Data = model.Data,
            Type = model.Type,
            Name = model.Name,
            AttachmentFileName = model.AttachmentFileName,
            UserId = model.UserId,
            CompanyId = model.CompanyId,
            IsLogoChanged = model.IsLogoChanged,
            AttachmentType = model.AttachmentType
        };
    }
    public static List<AppFileDTO> MapListModelToDto(List<AppFile> models)
    {
        return models.Select(model => new AppFileDTO
        {
            Id = model.Id,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted,
            Data = model.Data,
            Type = model.Type,
            Name = model.Name,
            AttachmentFileName = model.AttachmentFileName,
            UserId = model.UserId,
            CompanyId = model.CompanyId,
            IsLogoChanged = model.IsLogoChanged,
            AttachmentType = model.AttachmentType
        }).ToList();
    }
    public static List<AppFile> MapListDtoToModel(List<AppFileDTO> dtos)
    {
        return dtos.Select(dto => new AppFile
        {
            Id = dto.Id,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            IsDeleted = dto.IsDeleted,
            Data = dto.Data,
            Type = dto.Type,
            Name = dto.Name,
            AttachmentFileName = dto.AttachmentFileName,
            UserId = dto.UserId,
            CompanyId = dto.CompanyId,
            IsLogoChanged = dto.IsLogoChanged,
            AttachmentType = dto.AttachmentType
        }).ToList();
    }
}
