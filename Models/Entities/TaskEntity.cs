using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementWebAPI.Models.Entities;


// public record class TaskEntity(int Id, int UserId, string Title, string Description, bool Completed, DateTime CreatedAt, DateTime? UpdatedAt);

[Table("tasks", Schema = "public")]
public class TaskEntity
{

    public required int Id { get; set; }

    [Required]
    public required string Title { get; set; }

    public string? Description { get; set; }

    public bool Completed { get; set; } = false;

    [Required]
    public required DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? UpdatedDate { get; set; }

    // Foreign key pointing to User
    [Required]
    public int OwnerUserId { get; set; }
    public required UserEntity OwnerUser { get; set; }

    public HashSet<UserEntity> SharedUsers { get; } = [];
}