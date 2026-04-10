using ERP.Data;
using ERP.Entity.Product;

namespace ERP.Service.Product;

public class StockTransactionService
{
    private readonly ManufacturingDbContext _context;
    public StockTransactionService( ManufacturingDbContext context)
    {
        _context = context;   
    }
    public async Task AddReceiveTransactionAsync(string type, Guid productId, decimal quantity, Guid referenceId, string referenceNumber, string note, CancellationToken cancellationToken = default)
    {
        if (productId == Guid.Empty) throw new ArgumentException("productId is required", nameof(productId));
        if (quantity <= 0) throw new ArgumentException("quantity must be greater than zero", nameof(quantity));

        var tx = new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            Type = type,
            ReferenceId = referenceId,
            Date = DateTime.UtcNow,
            PerformedBy = "System",
            Notes = note
        };

        await _context.StockTransactions.AddAsync(tx, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
