using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Models.DTOs.Tasks;

namespace SyncoraBackend.Models.Entities;


// public record class TaskEntity(int Id, int UserId, string Title, string Description, bool Completed, DateTime CreatedAt, DateTime? UpdatedAt);

[Table("tasks", Schema = "public"), Index(nameof(LastModifiedDate))]
public class TaskEntity
{
    public int Id { get; set; }
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int? CompletedById { get; set; } = null;
    public UserEntity? CompletedBy { get; set; } = null;
    [Required]
    public required DateTime CreationDate { get; set; }
    [Required]
    public required int GroupId { get; set; }
    // Navigation Properties
    public GroupEntity Group { get; set; } = null!;
    [Required]
    public required DateTime LastModifiedDate { get; set; }
    public HashSet<UserEntity> AssignedTo { get; set; } = [];

    public override bool Equals(object? obj)
    {
        return obj is TaskEntity other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

}