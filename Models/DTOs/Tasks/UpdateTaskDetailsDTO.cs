using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Models.DTOs.Tasks;

public record class UpdateTaskDetailsDTO(string? Title, string? Description) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if ((!string.IsNullOrEmpty(Title) && !Validators.ValidateTitle(Title!)) || (!string.IsNullOrEmpty(Description) && !Validators.ValidateDescription(Description!)))
            yield return new ValidationResult("At least one field must be updated",
                [nameof(Title), nameof(Description)]);
    }
}
