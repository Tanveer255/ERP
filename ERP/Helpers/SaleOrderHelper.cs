using ERP.Entity.Product;
using ERP.Service;

namespace ERP.Helpers;

public static class SaleOrderHelper
{
    public static string GenerateCode() => Helper.GenerateCode();

    public static decimal GetFinalPrice(Price price) => Helper.GetFinalPrice(price);

    public static Task<bool> ExecuteWithRetryAsync(Func<Task> action, int maxRetry = 3) =>
        Helper.ExecuteWithRetryAsync(action, maxRetry);
}
