using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementWebAPI.Models.Entities;

[Table("task_groups", Schema = "public")]
public class TaskGroupEntity
{
    public int Id { get; set; }
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }

    public required DateTime CreationDate { get; set; }

    [Required]
    public required int OwnerUserId { get; set; }
    public UserEntity OwnerUser { get; set; } = null!;

    public HashSet<UserEntity> SharedUsers { get; } = [];


    public HashSet<TaskEntity> Tasks { get; } = [];


}