using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Models.DTOs.Users;

public record UpdateUserProfileDTO(string? Username, string? FirstName, string? LastName, UserPreferencesDTO? Preferences) : IValidatableObject
{

    public bool IsUpdatingPreferencesOnly => string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) && Preferences != null;
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) && Preferences == null)
            yield return new ValidationResult("At least one field must be updated", [nameof(Username), nameof(FirstName), nameof(LastName), nameof(Preferences)]);


        if (FirstName != null && !Validators.ValidateName(FirstName))
        {
            yield return new ValidationResult("First name is not in valid format.", [nameof(FirstName)]);

        }
        if (LastName != null && !Validators.ValidateName(LastName))
        {
            yield return new ValidationResult("Last name is not in valid format.", [nameof(LastName)]);
        }

        if (Username != null)
        {
            if (!Validators.ValidateUsername(Username))
            {
                yield return new ValidationResult("Username is not in valid format.", [nameof(Username)]); ;
            }
        }
    }
};
