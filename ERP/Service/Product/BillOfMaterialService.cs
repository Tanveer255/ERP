using ERP.Data;
using ERP.Data.DTO;
using ERP.Entity.BOM;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service.Product;

public class BillOfMaterialService
{
    private readonly ManufacturingDbContext _context;
    public BillOfMaterialService( ManufacturingDbContext manufacturingDbContext)
    {
          _context = manufacturingDbContext;
    }
    public async Task<ResultDTO<BillOfMaterial>> GetBillOfMaterialByProductId(Guid productId)
    {
        var billOfMaterial = await _context.BillOfMaterials
            .Include(b => b.Items)
            .Where(b => b.ProductId == productId)
            .FirstOrDefaultAsync();
        if (billOfMaterial ==null)
            return ResultDTO<BillOfMaterial>.Failure($"No BOM found for Product {productId}.");
        return ResultDTO<BillOfMaterial>.Success(billOfMaterial);
    }
}
