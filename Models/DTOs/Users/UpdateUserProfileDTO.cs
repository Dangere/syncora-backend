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


        if (!string.IsNullOrEmpty(FirstName) && !Validators.ValidateName(FirstName))
        {
            yield return new ValidationResult("First name is not in valid format.", [nameof(FirstName)]);

        }
        if (!string.IsNullOrEmpty(LastName) && !Validators.ValidateName(LastName))
        {
            yield return new ValidationResult("Last name is not in valid format.", [nameof(LastName)]);
        }

        if (!string.IsNullOrEmpty(Username) && !Validators.ValidateUsername(Username))
        {

            yield return new ValidationResult("Username is not in valid format.", [nameof(Username)]); ;

        }
    }
};
