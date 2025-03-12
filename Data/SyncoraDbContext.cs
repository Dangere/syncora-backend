using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Models.Entities;

namespace TaskManagementWebAPI.Data;

public class SyncoraDbContext(DbContextOptions<SyncoraDbContext> options) : DbContext(options)
{

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // One-to-Many: A Task has one Owner, while owner can own multiple tasks
        modelBuilder.Entity<TaskEntity>().HasOne(t => t.OwnerUser).WithMany(u => u.OwnedTasks).HasForeignKey(t => t.OwnerUserId);

        // Many-to-Many: A Task can be accessed by multiple users, while users can have access to multiple tasks
        modelBuilder.Entity<TaskEntity>().HasMany(t => t.SharedUsers).WithMany(u => u.AccessibleTasks);

        base.OnModelCreating(modelBuilder);
    }
}