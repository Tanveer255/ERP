using ERP.Data;
using ERP.Data.DTO.Contact;
using ERP.Entity.Contact;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers.Parties
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ManufacturingDbContext _context;
        public ContactController(ManufacturingDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// create supplier with products and pricing information. This will create a new contact of type supplier and link it to the specified products with their prices and lead times.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("create-supplier")]
        public async Task<IActionResult> CreateSupplier([FromBody]CreateSupplierDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Supplier name is required.");

            var supplier = new Contact
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Email = dto.Email,
                Phone = dto.Phone,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                Country = dto.Country,
                ContactPerson = dto.ContactPerson,
                TaxNumber = dto.TaxNumber,
                DefaultLeadTimeInDays = dto.DefaultLeadTimeInDays,
                Currency = dto.Currency,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contact.Add(supplier);

            // Link products to supplier
            foreach (var p in dto.Products)
            {
                var productSupplier = new ProductSupplier
                {
                    Id = Guid.NewGuid(),
                    ProductId = p.ProductId,
                    SupplierId = supplier.Id,
                    Price = p.Price,
                    LeadTimeInDays = p.LeadTimeInDays,
                    IsPreferred = p.IsPreferred
                };
                _context.ProductSuppliers.Add(productSupplier);
            }

            await _context.SaveChangesAsync();
            var result = new CreateSupplierDto
            {
                Name = supplier.Name,
                Code = supplier.Code,
                Email = supplier.Email,
                Phone = supplier.Phone,
                AddressLine1 = supplier.AddressLine1,
                AddressLine2 = supplier.AddressLine2,
                City = supplier.City,
                Country = supplier.Country,
                ContactPerson = supplier.ContactPerson,
                TaxNumber = supplier.TaxNumber,
                DefaultLeadTimeInDays = supplier.DefaultLeadTimeInDays,
                Currency = supplier.Currency,
                Products = dto.Products.Select(p => new SupplierProductDto
                {
                    ProductId = p.ProductId,
                    Price = p.Price,
                    LeadTimeInDays = p.LeadTimeInDays,
                    IsPreferred = p.IsPreferred
                }).ToList()
            };

            return Ok(result);
        }
    }
}
