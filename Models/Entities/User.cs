using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementWebAPI.Models.Entities;

[Table("users", Schema = "public")]
public class UserEntity
{
    public required int Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Hash { get; set; }
    public required string Salt { get; set; }

    // public List<TaskEntity> Tasks { get; set; }


}