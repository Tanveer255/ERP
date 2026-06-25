using BuildingBlocks.EventBus;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Consumers;
using Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("InventoryDb")));

builder.Services.AddRabbitMqEventBus(
    builder.Configuration["RabbitMq:Host"] ?? "localhost",
    builder.Configuration["RabbitMq:Username"] ?? "erp",
    builder.Configuration["RabbitMq:Password"] ?? "erp_secret",
    InventoryConsumerRegistration.RegisterConsumers);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreatedAsync();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

[ApiController]
[Route("api/v1/inventory")]
[Authorize]
public class InventoryController(InventoryDbContext db) : ControllerBase
{
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock(CancellationToken ct) =>
        Ok(await db.StockBalances.Where(x => x.TenantId == TenantId).AsNoTracking().ToListAsync(ct));

    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var balance = await db.StockBalances.FirstOrDefaultAsync(x =>
            x.TenantId == TenantId && x.ProductId == request.ProductId && x.WarehouseId == request.WarehouseId, ct);

        if (balance is null)
        {
            balance = new StockBalance { TenantId = TenantId, ProductId = request.ProductId, WarehouseId = request.WarehouseId };
            db.StockBalances.Add(balance);
        }

        balance.QuantityOnHand += request.QuantityDelta;
        db.StockTransactions.Add(new StockTransaction
        {
            TenantId = TenantId,
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            TransactionType = request.TransactionType,
            Quantity = request.QuantityDelta
        });
        await db.SaveChangesAsync(ct);
        return Ok(balance);
    }
}

public record AdjustStockRequest(Guid ProductId, Guid WarehouseId, decimal QuantityDelta, string TransactionType);
