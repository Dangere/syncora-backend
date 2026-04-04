using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Models.DTOs.Auth;

public record RegisterWithGoogleRequestDTO([Required] string IdToken, [Required] string Password, [Required] string Username, [Required] string FirstName, [Required] string LastName, UserPreferencesDTO? UserPreferences) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            yield return new ValidationResult("All fields must be filled", [nameof(Password), nameof(Username), nameof(FirstName), nameof(LastName)]);

        if (!Validators.ValidatePassword(Password))
        {
            yield return new ValidationResult("Password is not in valid format.", [nameof(Password)]);
        }

        if (!Validators.ValidateUsername(Username))
        {
            yield return new ValidationResult("Username is not in valid format.", [nameof(Username)]);
        }

        if (!Validators.ValidateName(FirstName) || !Validators.ValidateName(LastName))
        {
            yield return new ValidationResult("First name or last name is not in valid format.", [nameof(FirstName), nameof(LastName)]);
        }
    }
}