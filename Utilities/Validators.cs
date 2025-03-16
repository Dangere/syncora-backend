using System.Text.RegularExpressions;
namespace LibraryManagementSystem.Utilities;

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

    public static bool ValidatePhone(string phone)
    { //phone must be between 10 and 16 characters
        if (phone.Length > 16 || phone.Length < 10)
            return false;

        Regex regex = new(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}");
        Match match = regex.Match(phone);
        return match.Success;
    }
    public static bool ValidateBookTitle(string title)
    {
        // Trim whitespace and validate length
        string trimmedTitle = title.Trim();
        if (trimmedTitle.Length < 1 || trimmedTitle.Length > 100)
            return false;

        // Match allowed characters (letters, spaces, apostrophes, hyphens, commas, periods)
        Regex regex = new Regex(@"^[A-Za-z\s.,'-]+$");
        return regex.IsMatch(trimmedTitle);
    }
    public static bool ValidateBookAuthor(string author)
    {
        // Trim whitespace and validate length
        string trimmedAuthor = author.Trim();
        if (trimmedAuthor.Length < 2 || trimmedAuthor.Length > 50)
            return false;

        // Split into parts (e.g., first name, last name)
        string[] nameParts = trimmedAuthor.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Require at least one valid name part (e.g., "F. Scott Fitzgerald" has 3 parts)
        if (nameParts.Length < 1)
            return false;

        // Match allowed characters for each part (no standalone invalid symbols)
        Regex regex = new Regex(@"^[A-Za-z.,'-]+$");
        foreach (string part in nameParts)
        {
            if (!regex.IsMatch(part))
                return false;
        }

        return true;
    }

}