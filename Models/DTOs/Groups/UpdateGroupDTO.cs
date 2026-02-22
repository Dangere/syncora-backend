using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Groups;

public record UpdateGroupDTO(string? Title, string? Description) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Description))
            yield return new ValidationResult("At least one field must be updated",
                [nameof(Title), nameof(Description)]);
    }
}
