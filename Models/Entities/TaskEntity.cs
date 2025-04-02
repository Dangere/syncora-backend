using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SyncoraBackend.Models.Entities;


// public record class TaskEntity(int Id, int UserId, string Title, string Description, bool Completed, DateTime CreatedAt, DateTime? UpdatedAt);

[Table("tasks", Schema = "public")]
public class TaskEntity
{
    public int Id { get; set; }

    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    public bool Completed { get; set; } = false;

    [Required]
    public required DateTime CreationDate { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    [Required]
    public required int GroupId { get; set; }
    // Navigation Properties
    public GroupEntity Group { get; set; } = null!;

}