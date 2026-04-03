using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Models.DTOs.Users;

public record UserPreferencesDTO(bool? DarkMode, string? LanguageCode) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DarkMode == null && LanguageCode == null)
            yield return new ValidationResult("At least one field must be updated", [nameof(DarkMode), nameof(LanguageCode)]);

        if (!string.IsNullOrEmpty(LanguageCode))
        {
            if (!Validators.ValidateLanguageCode(LanguageCode))
            {
                yield return new ValidationResult("Language code is not in valid format.", [nameof(LanguageCode)]);
            }
        }

    }
}