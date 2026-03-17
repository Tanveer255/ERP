using ERP.Entity.Product;

namespace ERP.Service;

public class Helper
{
    public string GenerateCode()
    {
        string randomNo;
        Random random = new Random();
        randomNo = random.Next(100000000, 999999999).ToString();
        return randomNo;
    }
    public decimal GetFinalPrice(Price price)
    {
        decimal priceAfterDiscount = price.SalePrice;

        if (price.DiscountAmount.HasValue)
            priceAfterDiscount -= price.DiscountAmount.Value;

        if (price.DiscountPercentage.HasValue)
            priceAfterDiscount -= (price.SalePrice * price.DiscountPercentage.Value / 100);

        decimal tax = priceAfterDiscount * price.TaxPercentage / 100;

        return priceAfterDiscount + tax;
    }
}
