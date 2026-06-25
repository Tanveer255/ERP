using System.Security.Claims;
using Manufacturing.Application;
using Manufacturing.Application.Bom.Commands;
using Manufacturing.Application.Bom.Queries;
using Manufacturing.Application.DTOs;
using Manufacturing.Application.Production.Commands;
using Manufacturing.Infrastructure;
using Manufacturing.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddManufacturingApplication();
builder.Services.AddManufacturingInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
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
    await scope.ServiceProvider.GetRequiredService<ManufacturingDbContext>().Database.EnsureCreatedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program;

[ApiController]
[Route("api/v1/manufacturing")]
[Authorize]
public class ManufacturingController(IMediator mediator) : ControllerBase
{
    private Guid TenantId => Guid.Parse(User.FindFirstValue("tenant_id") ?? User.FindFirstValue("TenantId") ?? throw new UnauthorizedAccessException());

    [HttpGet("boms")]
    public async Task<ActionResult<IReadOnlyList<BomDto>>> GetBoms(CancellationToken ct) =>
        Ok(await mediator.Send(new GetBomsQuery(TenantId), ct));

    [HttpPost("boms")]
    public async Task<ActionResult<BomDto>> CreateBom([FromBody] CreateBomRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new CreateBomCommand(TenantId, request.ProductId, request.Lines), ct));

    [HttpGet("production-orders")]
    public async Task<ActionResult<IReadOnlyList<ProductionOrderDto>>> GetProductionOrders(CancellationToken ct) =>
        Ok(await mediator.Send(new GetProductionOrdersQuery(TenantId), ct));

    [HttpPost("production-orders")]
    public async Task<ActionResult<ProductionOrderDto>> CreateProductionOrder([FromBody] CreateProductionOrderRequest request, CancellationToken ct) =>
        Ok(await mediator.Send(new CreateProductionOrderCommand(
            TenantId, request.ProductId, request.BomId, request.PlannedQuantity,
            request.PlannedStartDate, request.PlannedFinishDate), ct));

    [HttpPost("production-orders/{id:guid}/release")]
    public async Task<ActionResult<ProductionOrderDto>> Release(Guid id, CancellationToken ct) =>
        Ok(await mediator.Send(new ReleaseProductionOrderCommand(TenantId, id), ct));
}

public record CreateBomRequest(Guid ProductId, IReadOnlyList<BomLineDto> Lines);
public record CreateProductionOrderRequest(Guid ProductId, Guid BomId, decimal PlannedQuantity, DateTime PlannedStartDate, DateTime PlannedFinishDate);
