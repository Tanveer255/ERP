using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;
using Microsoft.EntityFrameworkCore;

namespace ERP.Service;

public static class Helper
{
    public static string GenerateCode()
    {
        string randomNo;
        Random random = new Random();
        randomNo = random.Next(100000000, 999999999).ToString();
        return randomNo;
    }
    public static decimal GetFinalPrice(Price price)
    {
        decimal priceAfterDiscount = price.SalePrice;

        if (price.DiscountAmount.HasValue)
            priceAfterDiscount -= price.DiscountAmount.Value;

        if (price.DiscountPercentage.HasValue)
            priceAfterDiscount -= (price.SalePrice * price.DiscountPercentage.Value / 100);

        decimal tax = priceAfterDiscount * price.TaxPercentage / 100;

        return priceAfterDiscount + tax;
    }
    public static void UpdateReservationStatus(SalesOrder order)
    {
        if (order.Items.All(i => i.QuantityReserved == i.QuantityRequested))
            order.ReservationStatus = ReservationStatus.Full;

        else if (order.Items.Any(i => i.QuantityReserved > 0))
            order.ReservationStatus = ReservationStatus.Partial;

        else
            order.ReservationStatus = ReservationStatus.None;
    }
    public static async Task<bool> ExecuteWithRetryAsync(Func<Task> action, int maxRetry = 3)
    {
        int retry = maxRetry;

        while (retry > 0)
        {
            try
            {
                await action();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                retry--;
                if (retry == 0) return false;
            }
        }
        return false;
    }
}
