using System.Security.Cryptography;
namespace TaskManagementWebAPI.Utilities;
public static class Hashing
{

    public static byte[] GenerateSalt()
    {
        byte[] salt = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);

        }
        return salt;
    }

    public static string HashPassword(string password, byte[] salt)
    {
        using var Pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);

        byte[] hashBytes = Pbkdf2.GetBytes(32);

        return Convert.ToBase64String(hashBytes);
    }

}