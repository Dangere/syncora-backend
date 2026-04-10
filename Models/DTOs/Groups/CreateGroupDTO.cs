using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Models.DTOs.Groups;

public record CreateGroupDTO([Required] string Title, string? Description) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Validators.ValidateTitle(Title) || (!string.IsNullOrEmpty(Description) && !Validators.ValidateDescription(Description!)))
            yield return new ValidationResult("Invalid Title or description");
    }
}