using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Models.DTOs.Report;

public record SubmitReportDTO(
    [Required] string Message,
    [Required] string AppVersion,
    [Required] string Platform,
    [Required] string OsVersion,
    [Required] string DeviceModel,
    [Required] string Locale,
    [Required] Dictionary<string, object> UserSession,
    [Required] Dictionary<string, object> AppState,
    [Required] Dictionary<string, object>[] Breadcrumbs,
    [Required] DateTime CreationDate
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreationDate > DateTime.UtcNow.AddMinutes(5))
            yield return new ValidationResult(
                "CreationDate must not be in the future.",
                [nameof(CreationDate)]
            );



        var requiredStrings = new[]
        {
            (Value: Message,    Name: nameof(Message)),
            (Value: AppVersion, Name: nameof(AppVersion)),
            (Value: Platform,   Name: nameof(Platform)),
            (Value: OsVersion,  Name: nameof(OsVersion)),
            (Value: DeviceModel,Name: nameof(DeviceModel)),
            (Value: Locale,     Name: nameof(Locale)),

        };

        foreach (var (Value, Name) in requiredStrings)
        {
            if (string.IsNullOrWhiteSpace(Value))
                yield return new ValidationResult($"{Name} must not be empty.", [Name]);
        }

        if (Breadcrumbs.Length > 50)
            yield return new ValidationResult("Breadcrumbs must not exceed 50 entries.", [nameof(Breadcrumbs)]);



    }
}