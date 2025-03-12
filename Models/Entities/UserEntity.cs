using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementWebAPI.Models.Entities;

[Table("users", Schema = "public")]
public class UserEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Hash { get; set; }
    public required string Salt { get; set; }

    // Using HashSet to avoid duplicate TaskEntity instances in memory.
    // One-to-Many: A user can own multiple tasks
    public HashSet<TaskEntity> OwnedTasks { get; set; } = [];

    // Many-to-Many: A user can access multiple tasks
    public HashSet<TaskEntity> AccessibleTasks { get; set; } = [];
}