using System.Security.Cryptography;
namespace SyncoraBackend.Utilities;

public static class Hashing
{

    public static string GenerateSalt()
    {
        byte[] salt = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);

        }
        return Convert.ToBase64String(salt);
    }

    public static string HashString(string str, string? salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt ?? string.Empty);
        using var Pbkdf2 = new Rfc2898DeriveBytes(str, saltBytes, 10000, HashAlgorithmName.SHA256);

        byte[] hashBytes = Pbkdf2.GetBytes(32);

        return Convert.ToBase64String(hashBytes);
    }

}