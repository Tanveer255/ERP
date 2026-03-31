using ERP.Entity.Document;
using ERP.Entity.Product;
using ERP.Enum;

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
        if (order.Items.All(i => i.ReservedQuantity == i.RequestedQuantity))
            order.ReservationStatus = ReservationStatus.Full;

        else if (order.Items.Any(i => i.ReservedQuantity > 0))
            order.ReservationStatus = ReservationStatus.Partial;

        else
            order.ReservationStatus = ReservationStatus.None;
    }
}
