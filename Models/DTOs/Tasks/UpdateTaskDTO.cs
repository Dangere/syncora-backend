using System.ComponentModel.DataAnnotations;

namespace TaskManagementWebAPI.Models.DTOs.Tasks;

public record class UpdateTaskDTO(string? NewTitle, string? NewDescription, bool? Completed) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewTitle == null && NewDescription == null && Completed is null)
            yield return new ValidationResult("At least one field must be updated",
                [nameof(NewTitle), nameof(NewDescription), nameof(Completed)]);
    }
}
