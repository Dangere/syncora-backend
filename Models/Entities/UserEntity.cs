using System.ComponentModel.DataAnnotations.Schema;
using SyncoraBackend.Enums;

namespace SyncoraBackend.Models.Entities;

[Table("users", Schema = "public")]
public class UserEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string Hash { get; set; }
    public required string Salt { get; set; }

    public required UserRole Role { get; set; } = UserRole.User;

    public required DateTime CreationDate { get; set; } = DateTime.UtcNow;

    public string? ProfilePictureURL { get; set; } = null;

    // Using HashSet to avoid duplicate TaskEntity instances in memory.
    // One-to-Many: A user can own multiple groups, while groups can be owned by only one user
    public HashSet<GroupEntity> OwnedGroups { get; set; } = [];

    // Many-to-Many: A user can access multiple groups, while groups can be accessed by multiple users
    public HashSet<GroupEntity> AccessibleGroups { get; set; } = [];
}