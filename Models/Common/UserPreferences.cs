using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncoraBackend.Models.Common;

public class UserPreferences(bool isDarkMode, string language)
{
    public bool IsDarkMode { get; set; } = isDarkMode;
    public string Language { get; set; } = language;

}
