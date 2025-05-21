using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SyncoraBackend.Models.Entities;

[Table("deleted_records", Schema = "public"), Index(nameof(GroupId), nameof(DeletedAt))]
public class DeletedRecord()
{
    public required int Id { get; set; }
    [Required]
    public required int GroupId { get; set; }
    [Required]
    public required DateTime DeletedAt { get; set; }
    [Required]
    public required string TableName { get; set; }

    [Required]
    public required int EntityId { get; set; }
};