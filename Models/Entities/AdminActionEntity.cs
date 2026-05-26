using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SyncoraBackend.Models.Entities;

[Table("admin_actions", Schema = "public"), Index(nameof(AdminId))]

public class AdminActionEntity
{
    public int Id { get; set; }

    [Required]
    public required int AdminId { get; set; }
    public UserEntity Admin { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public required string Action { get; set; }
}
