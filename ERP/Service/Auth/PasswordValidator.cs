using ERP.Entity.Auth;
using ERP.Repository;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Security.Cryptography;

namespace ERP.Service.Auth;

public interface IPasswordValidator
{
    bool VerifyPassword(User user, string password);
    bool VerifyLegacyPassword(string inputPassword, string storedHash, string storedSalt);
}
public class PasswordValidator(
     IUnitOfWork unitOfWork,
         ILogger<PasswordValidator> logger
    ) : IPasswordValidator
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<PasswordValidator> _logger = logger;

    public bool VerifyPassword(User user, string password)
    {
        try
        {
            if (user == null) return false;

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success;
        }
        catch (Exception exception)
        {
            logger.LogError($"Error in {nameof(PasswordValidator)}.{nameof(VerifyPassword)}");
            _logger.LogError(exception, exception.Message, exception.InnerException, exception.InnerException?.Message ?? string.Empty);
            return false;
        }
    }


    public static string ComputeHash(string password, string salt)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hashBytes = sha1.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }

    public bool VerifyLegacyPassword(string inputPassword, string storedHash, string storedSalt)
    {
        string computedHash = ComputeHash(inputPassword, storedSalt);
        return computedHash.Equals(storedHash, StringComparison.CurrentCultureIgnoreCase);
    }

}
