using System.Globalization;
using System.Text.RegularExpressions;
namespace SyncoraBackend.Utilities;

public static class Validators
{

    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
        if (email.Length > 30)
            return false;

        Regex regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        Match match = regex.Match(email);
        return match.Success;

    }
    public static bool ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;
        //password must be between 6 and 16 characters
        if (password.Length > 32 || password.Length < 6)
            return false;

        return true;
    }

    public static bool ValidateUsername(string username)
    {
        if (username.Length > 20 || username.Length < 2)
            return false;


        Regex regex = new(@"^[a-zA-Z0-9\s]*$");
        Match match = regex.Match(username);
        return match.Success;
    }

    public static bool ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
        return Regex.IsMatch(name, @"^[^';<>\s\\]{1,20}$");
    }

    public static bool ValidateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            return false;
        return Regex.IsMatch(title, @"^\S[^';<>\\]{1,108}\S$|^\S{3,110}$");
    }

    public static bool ValidateDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            return false;
        return Regex.IsMatch(description, @"^\S[^';<>\\]{4,253}\S$|^\S{6,255}$");
    }
    public static bool ValidateLanguageCode(string code)
    {
        try
        {
            // This will throw a CultureNotFoundException if the code is invalid
            var culture = CultureInfo.GetCultureInfo(code);
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

}