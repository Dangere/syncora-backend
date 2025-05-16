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

    public required DateTime CreationDate { get; set; }

    [Required]
    public required int OwnerUserId { get; set; }
    public UserEntity OwnerUser { get; set; } = null!;

    public HashSet<UserEntity> Members { get; } = [];


    public HashSet<TaskEntity> Tasks { get; } = [];

    [Required]
    public required DateTime LastModifiedDate { get; set; }


    public override bool Equals(object? obj)
    {
        return obj is GroupEntity other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}