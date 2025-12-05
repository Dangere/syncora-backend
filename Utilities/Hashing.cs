using System.Security.Cryptography;
namespace SyncoraBackend.Utilities;

public static class Hashing
{
    // OWASP recommended iterations for PBKDF2-SHA256
    private const int Iterations = 600000;
    public static string GenerateSalt()
    {
        byte[] salt = new byte[16];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);

        }
        return Convert.ToBase64String(salt);
    }

    public static string HashPassword(string str, string salt)
    {

        byte[] saltBytes = Convert.FromBase64String(salt);

        using var Pbkdf2 = new Rfc2898DeriveBytes(str, saltBytes, Iterations, HashAlgorithmName.SHA256);

        byte[] hashBytes = Pbkdf2.GetBytes(32);

        return Convert.ToBase64String(hashBytes);
    }

    public static string HashToken(string str, string? salt)
    {

        byte[] saltBytes;

        if (salt != null)
            saltBytes = Convert.FromBase64String(salt);
        else
            saltBytes = new byte[16];

        using var Pbkdf2 = new Rfc2898DeriveBytes(str, saltBytes, Iterations / 30, HashAlgorithmName.SHA256);

        byte[] hashBytes = Pbkdf2.GetBytes(32);

        return Convert.ToBase64String(hashBytes);
    }

}