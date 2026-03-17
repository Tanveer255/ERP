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
}
