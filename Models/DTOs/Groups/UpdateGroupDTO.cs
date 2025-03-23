using System.ComponentModel.DataAnnotations;

namespace TaskManagementWebAPI.Models.DTOs.Groups;

public record UpdateGroupDTO(string? Title, string? Description) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Title == null && Description == null)
            yield return new ValidationResult("At least one field must be updated",
                [nameof(Title), nameof(Description)]);
    }
}
