using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.Common;

namespace SyncoraBackend.Models.DTOs.Users;

public record UpdateUserProfileDTO(string? Username, string? FirstName, string? LastName, UserPreferences? Preferences) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) && Preferences == null)
            yield return new ValidationResult("At least one field must be updated", [nameof(Username), nameof(FirstName), nameof(LastName), nameof(Preferences)]);
    }
};
