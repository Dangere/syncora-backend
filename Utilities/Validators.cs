using System.Text.RegularExpressions;
namespace SyncoraBackend.Utilities;

public static class Validators
{

    public static bool ValidateEmail(string email)
    {
        if (email.Length > 30)
            return false;

        Regex regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        Match match = regex.Match(email);
        return match.Success;

    }
    public static bool ValidatePassword(string password)
    {
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
        if (!(name.Length <= 10 && name.Length >= 2))
            return false;


        string[] namesArray = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (namesArray.Length != 1)
        {
            return false;
        }

        Regex regex = new(@"[A-Za-z., '-]+");
        Match match = regex.Match(name);

        return match.Success;
    }

    public static bool ValidateTitle(string title)
    {
        if (title.Length > 50 || title.Length < 2)
            return false;

        Regex regex = new(@"^[a-zA-Z0-9\s]*$");
        Match match = regex.Match(title);
        return match.Success;
    }

    public static bool ValidateDescription(string description)
    {
        if (description.Length > 200 || description.Length < 2)
            return false;

        Regex regex = new(@"^[a-zA-Z0-9\s]*$");
        Match match = regex.Match(description);
        return match.Success;
    }

}