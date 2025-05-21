using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Models.Entities;

namespace SyncoraBackend.Models.Entities;

[Table("group_members", Schema = "public"), Index(nameof(UserId), nameof(GroupId))]
public class GroupMemberEntity
{
    public int Id { get; set; }
    // Composite PK
    public required int GroupId { get; set; }
    public required int UserId { get; set; }

    // Any extra fields you might want:
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public required string RoleInGroup { get; set; }

    // Navigation
    public GroupEntity Group { get; set; } = null!;
    public UserEntity User { get; set; } = null!;

    public override bool Equals(object? obj)
    {
        return obj is TaskEntity other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}