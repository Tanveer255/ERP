using ERP.Data.DTO.Auth;
using ERP.Entity;
using ERP.Entity.Auth;
using ERP.Repository;
using ERP.Repository.Auth;
using ERP.Repository.Contact;
using ERP.Repository.Product;
using ERP.Service;
using ERP.Service.Auth;
using ERP.Service.Common;
using ERP.Service.Contact;
using ERP.Service.Document;
using ERP.Service.Product;
using ERP.Service.Production;
using Microsoft.AspNetCore.Identity;

namespace ERP.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddErpServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<ReCaptchaSettings>(configuration.GetSection("ReCaptchaSettings"));
        services.Configure<SendGridSettings>(configuration.GetSection("SendGridSettings"));
        services.Configure<ApplicationSettings>(configuration.GetSection("ApplicationSettings"));
        services.Configure<AppSetting>(configuration);

        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddHttpClient<IRecaptchaService, RecaptchaService>();

        services.AddIdentity<User, Role>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<Data.ManufacturingDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(ICrudService<>), typeof(CrudService<>));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<ISettingRepository, SettingRepository>();
        services.AddScoped<IAddressTypeRepository, AddressTypeRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IAppFileRepository, AppFileRepository>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<BillOfMaterialService>();
        services.AddScoped<MrpService>();
        services.AddScoped<SalesOrderService>();
        services.AddScoped<PurchaseOrderService>();
        services.AddScoped<StockTransactionService>();
        services.AddScoped<ProductStockService>();
        services.AddScoped<ProductionOrderService>();
        services.AddScoped<ProductionOperationService>();

        services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IAddressTypeService, AddressTypeService>();
        services.AddScoped<IAppFileService, AppFileService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IPasswordValidator, PasswordValidator>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<ISettingsService, SettingService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:61104";

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(frontendUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
