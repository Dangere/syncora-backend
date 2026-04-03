using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SyncoraBackend.Models.DTOs.Users;

namespace SyncoraBackend.Models.Entities;

public class UserPreferences
{
    // 1. Private backing fields (EF Core will interact with these)
    private bool? _darkMode;
    private string? _languageCode;

    // 2. Public properties with fallback logic
    public bool DarkMode
    {
        get => _darkMode ?? false; // Fallback to false if null
        set => _darkMode = value;
    }

    public string LanguageCode
    {
        get => _languageCode ?? "en"; // Fallback to 0 if null
        set => _languageCode = value;
    }

    // Factory method to create a new UserPreferences instance with pre set values
    // TODO: Request the user input from the frontend on registering
    public static UserPreferences WithUserInputs(bool darkMode, string languageCode) => new() { DarkMode = darkMode, LanguageCode = languageCode };

    /// <summary>
    /// Takes in a UserPreferencesDTO and updates the UserPreferences instance
    /// The UserPreferences instance can have a set values or the defaults
    /// </summary>
    /// <param name="userPreferencesDTO"></param>
    /// <returns></returns>
    public UserPreferences UpdateFromDTO(UserPreferencesDTO? userPreferencesDTO)
    {
        if (userPreferencesDTO == null) return this;

        return new UserPreferences
        {
            DarkMode = userPreferencesDTO.DarkMode ?? DarkMode,
            LanguageCode = userPreferencesDTO.LanguageCode ?? LanguageCode
        };

    }
}
