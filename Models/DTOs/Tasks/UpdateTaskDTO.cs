using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Tasks;

public record class UpdateTaskDTO(string? Title, string? Description, bool? Completed) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Title == null && Description == null && Completed == null)
            yield return new ValidationResult("At least one field must be updated",
                [nameof(Title), nameof(Description)]);
    }


    public bool IsUpdatingCompletionOnly() => Completed != null && Title == null && Description == null;
}
