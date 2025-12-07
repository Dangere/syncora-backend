using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncoraBackend.Models.Common;

public class UserPreferences
{
    // 1. Private backing fields (EF Core will interact with these)
    private bool? _isDarkMode;
    private string? _language;

    // 2. Public properties with fallback logic
    public bool IsDarkMode
    {
        get => _isDarkMode ?? true; // Fallback to true if null
        set => _isDarkMode = value;
    }

    public string Language
    {
        get => _language ?? "en"; // Fallback to "en" if null
        set => _language = value;
    }

    // Factory method to create a new UserPreferences instance with pre set values
    // TODO: Request the user input from the frontend on registering
    public static UserPreferences WithUserInputs(bool isDarkMode, string language) => new() { IsDarkMode = isDarkMode, Language = language };

}
