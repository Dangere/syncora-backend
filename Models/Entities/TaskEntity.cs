using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementWebAPI.Models.Entities;


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

    // Using HashSet to avoid duplicate TaskEntity instances in memory.
    // Foreign key pointing to User
    [Required]
    public required int OwnerUserId { get; set; }
    public UserEntity OwnerUser { get; set; } = null!;

    public HashSet<UserEntity> SharedUsers { get; } = [];
}