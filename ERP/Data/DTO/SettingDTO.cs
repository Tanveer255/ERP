using ERP.Entity;

namespace ERP.Data.DTO;

public record SettingDTO
{
    public Guid Id { get; set; }
    public string TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string ProductExpiryDays { get; set; } = string.Empty;
    public string ProcessUser { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string SelectedAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public static SettingDTO MapModelToDTO(Setting entity)
    {
        return new SettingDTO
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Email = entity.Email,
            Currency = entity.Currency,
            ProcessUser = entity.ProcessUser,
            Comment = entity.Comment,
            SelectedAddress = entity.SelectedAddress,
            ProductExpiryDays = entity.ProductExpiryDays,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    public static Setting MapDTOToModel(SettingDTO dto)
    {
        return new Setting
        {
            TenantId = dto.TenantId,
            Email = dto.Email,
            Currency = dto.Currency,
            ProcessUser = dto.ProcessUser,
            Comment = dto.Comment,
            SelectedAddress = dto.SelectedAddress,
            ProductExpiryDays = dto.ProductExpiryDays,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}
