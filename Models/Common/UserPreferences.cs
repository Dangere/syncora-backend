using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncoraBackend.Models.Common;

public class UserPreferences
{
    // 1. Private backing fields (EF Core will interact with these)
    private bool? _isDarkMode;
    private dynamic? _language;

    // 2. Public properties with fallback logic
    public bool IsDarkMode
    {
        get => _isDarkMode ?? true; // Fallback to true if null
        set => _isDarkMode = value;
    }

    public Language Language
    {
        get => _language ?? Language.English; // Fallback to 0 if null
        set => _language = value;
    }

    // Factory method to create a new UserPreferences instance with pre set values
    // TODO: Request the user input from the frontend on registering
    public static UserPreferences WithUserInputs(bool isDarkMode, Language language) => new() { IsDarkMode = isDarkMode, Language = language };

}
public enum Language
{
    English = 0,
    Arabic = 1

}

