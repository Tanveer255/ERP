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

    // POST: api/bom/create-bom
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

        var createdBoms = new List<BillOfMaterial>();

        foreach (var component in dto.Components)
        {
            if (component.ComponentId == dto.ProductId)
                return BadRequest("Product cannot be its own component.");

            if (component.Quantity <= 0)
                return BadRequest("Component quantity must be greater than zero.");

            // Check component exists
            var componentProduct = await _context.Products.FindAsync(component.ComponentId);
            if (componentProduct == null)
                return NotFound($"Component product {component.ComponentId} not found.");

            // Prevent duplicate component in same BOM
            var exists = await _context.BillOfMaterials.AnyAsync(b =>
                b.ProductId == dto.ProductId &&
                b.ComponentId == component.ComponentId);

            if (exists)
                return BadRequest($"Component {component.ComponentId} already exists in BOM.");

            var bom = new BillOfMaterial
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                ComponentId = component.ComponentId,
                Quantity = component.Quantity
            };

            createdBoms.Add(bom);
            _context.BillOfMaterials.Add(bom);
        }

        await _context.SaveChangesAsync();

        return Ok(createdBoms);
    }
}
