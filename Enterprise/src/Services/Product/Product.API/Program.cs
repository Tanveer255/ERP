using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Product.Infrastructure.Persistence;
using System.Security.Claims;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProductDb")));

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
    await scope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.EnsureCreatedAsync();

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController(ProductDbContext db) : ControllerBase
{
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id")!);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await db.Products.Where(x => x.TenantId == TenantId).AsNoTracking().ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product.Domain.Entities.Product
        {
            TenantId = TenantId,
            Sku = request.Sku,
            Name = request.Name,
            Category = request.Category,
            Unit = request.Unit,
            UnitCost = request.UnitCost,
            SalePrice = request.SalePrice,
            IsManufactured = request.IsManufactured,
            Tracking = request.Tracking
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.TenantId == TenantId, ct);
        return product is null ? NotFound() : Ok(product);
    }
}

public record CreateProductRequest(string Sku, string Name, string? Category, string Unit, decimal UnitCost, decimal SalePrice, bool IsManufactured, Product.Domain.Entities.TrackingType Tracking);