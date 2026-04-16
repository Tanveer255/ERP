using ERP.Data;
using ERP.Data.Request;
using ERP.Entity.Product;

namespace ERP.Service.Product;

public class StockTransactionService
{
    private readonly ManufacturingDbContext _context;
    public StockTransactionService( ManufacturingDbContext context)
    {
        _context = context;   
    }
    public async Task AddReceiveTransactionAsync(
    ReceiveTransactionRequest request,
    CancellationToken cancellationToken = default)
    {
        if (request.ProductId == Guid.Empty)
            throw new ArgumentException("productId is required", nameof(request.ProductId));

        if (request.Quantity <= 0)
            throw new ArgumentException("quantity must be greater than zero", nameof(request.Quantity));

        var tx = new StockTransaction
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Type = nameof(request.Type),
            ReferenceId = request.ReferenceId,
            Date = DateTime.UtcNow,
            PerformedBy = request.PerformedBy ?? "System",
            Notes = request.Note
        };

        await _context.StockTransactions.AddAsync(tx, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
