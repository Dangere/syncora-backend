using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Models.DTOs.Auth;

public record LoginRequestDTO([Required, EmailAddress] string Email, [Required] string Password) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            yield return new ValidationResult("All fields must be filled", [nameof(Email), nameof(Password)]);

        if (!Validators.ValidateEmail(Email))
        {
            yield return new ValidationResult("Email is not in valid format.", [nameof(Password)]);
        }

        if (!Validators.ValidatePassword(Password))
        {
            yield return new ValidationResult("Password is not in valid format.", [nameof(Password)]);
        }

    }
}