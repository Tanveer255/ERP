using ERP.Data;
using ERP.Entity;
using ERP.Entity.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers;
[ApiController]
[Route("api/[controller]")]
public class BOMController : ControllerBase
{
    private readonly ManufacturingDbContext _context;

    public BOMController(ManufacturingDbContext context)
    {
        _context = context;
    }

    [HttpPost("create-bom")]
    public async Task<IActionResult> CreateBOM(CreateBOMDto dto)
    {
        if (dto.ProductId == Guid.Empty)
            return BadRequest("ProductId is required.");

        if (dto.Components == null || !dto.Components.Any())
            return BadRequest("At least one component is required.");

        // Check finished product exists
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            return NotFound("Finished product not found.");

        // Check for duplicates in request
        var duplicateComponents = dto.Components
            .GroupBy(c => c.ComponentId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateComponents.Any())
            return BadRequest($"Duplicate components in request: {string.Join(", ", duplicateComponents)}");

        // Check existing BOMs to prevent duplicate component entries
        var existingComponentIds = await _context.BillOfMaterialItems
            .Where(b => b.BillOfMaterial.ProductId == dto.ProductId)
            .Select(b => b.ComponentId)
            .ToListAsync();

        foreach (var component in dto.Components)
        {
            if (component.ComponentId == dto.ProductId)
                return BadRequest("Product cannot be its own component.");

            if (component.Quantity <= 0)
                return BadRequest("Component quantity must be greater than zero.");

            if (existingComponentIds.Contains(component.ComponentId))
                return BadRequest($"Component {component.ComponentId} already exists in BOM.");

            // Check component exists
            var componentProduct = await _context.Products.FindAsync(component.ComponentId);
            if (componentProduct == null)
                return NotFound($"Component product {component.ComponentId} not found.");
        }

        // Begin transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create BOM (parent)
            var bom = new BillOfMaterial
            {
                ProductId = product.Id
            };
            _context.BillOfMaterials.Add(bom);
            await _context.SaveChangesAsync(); // Save to get BOM Id

            // Create BOM items (children)
            var bomItems = new List<BillOfMaterialItem>();
            foreach (var component in dto.Components)
            {
                var item = new BillOfMaterialItem
                {
                    BillOfMaterialId = bom.Id,
                    ComponentId = component.ComponentId,
                    Quantity = component.Quantity,
                    Unit = component.Unit
                };
                _context.BillOfMaterialItems.Add(item);
                bomItems.Add(item);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Return created BOM with items
            var result = new
            {
                BOM = new
                {
                    bom.Id,
                    bom.ProductId,
                    Components = bomItems.Select(i => new
                    {
                        i.Id,
                        i.ComponentId,
                        i.Quantity,
                        i.Unit
                    })
                }
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Error creating BOM: {ex.Message}");
        }
    }
}
