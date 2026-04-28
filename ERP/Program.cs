using ERP.Data;
using ERP.Repository;
using ERP.Repository.Contact;
using ERP.Repository.Product;
using ERP.Service;
using ERP.Service.Common;
using ERP.Service.Contact;
using ERP.Service.Document;
using ERP.Service.Product;
using ERP.Service.Production;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ManufacturingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddTransient(typeof(ICrudService<>), typeof(CrudService<>));
builder.Services.AddTransient<IProductService,ProductService>();
builder.Services.AddScoped<BillOfMaterialService>();
builder.Services.AddScoped<MrpService>();
builder.Services.AddScoped<SalesOrderService>();
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<StockTransactionService>();
builder.Services.AddScoped<ProductStockService>();
builder.Services.AddScoped<ProductionOrderService>();
builder.Services.AddScoped<ProductionOperationService>();
builder.Services.AddTransient<IProductRepository,ProductRepository>();
builder.Services.AddTransient<IContactService, ContactService>();
builder.Services.AddTransient<IContactRepository, ContactRepository>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Auto apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ManufacturingDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
