using System.Globalization;
using System.Text.RegularExpressions;
namespace SyncoraBackend.Utilities;

public static class Validators
{
    /// <summary>
    ///     Checks if the email is valid
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Checks if the username is valid
    ///     Username must be between 3 and 20 characters
    ///     Username must be alphanumeric
    ///     Username must not contain spaces
    ///     Username must not contain special characters
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static bool ValidateUsername(string username)
    {
        if (username.Length > 20 || username.Length < 3)
            return false;


        Regex regex = new(@"^[a-zA-Z0-9\s]*$");
        Match match = regex.Match(username);
        return match.Success;
    }

    /// <summary>
    ///     Checks if the name is valid
    ///     Name must be between 3 and 20 characters
    ///     Name must not contain special characters
    ///     Name must not contain spaces
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
        return Regex.IsMatch(name, @"^[^';<>\s\\]{3,20}$");
    }

    /// <summary>
    ///     Checks if the title is valid
    ///     Title must be between 3 and 110 characters
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    public static bool ValidateTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            return false;
        return Regex.IsMatch(title, @"^\S[^';<>\\]{1,108}\S$|^\S{3,110}$");
    }

    /// <summary>
    ///     Checks if the description is valid
    ///     Description must be between 4 and 255 characters
    /// </summary>
    /// <param name="description"></param>
    /// <returns></returns>
    public static bool ValidateDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            return false;
        return Regex.IsMatch(description, @"^\S[^';<>\\]{4,253}\S$|^\S{6,255}$");
    }

    /// <summary>
    ///     Checks if the language code is valid
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
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