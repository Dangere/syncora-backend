using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Models.Entities;

namespace TaskManagementWebAPI.Data;

public class SyncoraDbContext(DbContextOptions<SyncoraDbContext> options) : DbContext(options)
{

    public DbSet<UserEntity> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}