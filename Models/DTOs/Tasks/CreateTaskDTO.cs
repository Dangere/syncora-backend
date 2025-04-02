using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Tasks;

public record class CreateTaskDTO(
    [Required] string Title,
    string Description
// [Required, Range(1, int.MaxValue, ErrorMessage = "OwnerId must be greater than 0.")] int OwnerId
//Using the `Range` attribute with ints to make sure if the value is missing and automatically set to the default value of 0 for ints
//it will throw an error thanks to [ApiController] on our controllers
);