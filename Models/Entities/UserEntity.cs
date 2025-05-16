using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Enums;

namespace SyncoraBackend.Models.Entities;

[Table("users", Schema = "public"), Index(nameof(Email)), Index(nameof(Username), IsUnique = true), Index(nameof(LastModifiedDate))]
public class UserEntity
{
    public int Id { get; set; }
    public required string Email { get; set; }
    [Required]
    public required string Username { get; set; }
    [Required]
    public required string Hash { get; set; }
    [Required]
    public required string Salt { get; set; }
    [Required]
    public required UserRole Role { get; set; } = UserRole.User;

    [Required]
    public required DateTime CreationDate { get; set; } = DateTime.UtcNow;

    public string? ProfilePictureURL { get; set; } = null;

    // Using HashSet to avoid duplicate TaskEntity instances in memory.
    // One-to-Many: A user can own multiple groups, while groups can be owned by only one user
    public HashSet<GroupEntity> OwnedGroups { get; set; } = [];

    // Many-to-Many: A user can access multiple groups, while groups can be accessed by multiple users
    public HashSet<GroupEntity> AccessibleGroups { get; set; } = [];

    public required DateTime LastModifiedDate { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is UserEntity other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}