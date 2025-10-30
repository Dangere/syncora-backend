using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SyncoraBackend.Models.Entities;


[Table("groups", Schema = "public"), Index(nameof(OwnerUserId), nameof(LastModifiedDate))]
public class GroupEntity
{
    public int Id { get; set; }
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }
    [Required]
    public required DateTime CreationDate { get; set; }

    [Required]
    public required int OwnerUserId { get; set; }
    public UserEntity OwnerUser { get; set; } = null!;

    // public HashSet<UserEntity> Members { get; } = [];
    public HashSet<GroupMemberEntity> GroupMembers { get; } = [];

    public HashSet<TaskEntity> Tasks { get; } = [];

    [Required]
    public required DateTime LastModifiedDate { get; set; }

    public DateTime? DeletedAt { get; set; } = null;

    public override bool Equals(object? obj)
    {
        return obj is GroupEntity other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    // This method assumes users are loaded into memory.
    public GroupAccess GetGroupAccess(int userId)
    {
        bool isOwner = OwnerUserId == userId;
        bool isShared = GroupMembers.Any(m => m.UserId == userId);
        if (!isOwner && isShared)
        {
            return GroupAccess.Shared;
        }
        else if (!isOwner && !isShared)
            return GroupAccess.Denied;
        return GroupAccess.Owner;
    }

}
public enum GroupAccess
{
    Owner,
    Shared,
    Denied
}