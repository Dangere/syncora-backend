using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Models.Entities;

namespace SyncoraBackend.Data;

public class SyncoraDbContext(DbContextOptions<SyncoraDbContext> options) : DbContext(options)
{

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<GroupEntity> Groups { get; set; }
    public DbSet<GroupMemberEntity> GroupMembers { get; set; }

    public DbSet<DeletedRecord> DeletedRecords { get; set; }

    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // One-to-Many: A Task has one group, while the group can own multiple tasks
        modelBuilder.Entity<TaskEntity>().HasOne(t => t.Group).WithMany(tg => tg.Tasks).HasForeignKey(t => t.GroupId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskEntity>().HasMany(t => t.AssignedTo).WithMany(u => u.AssignedTasks);
        modelBuilder.Entity<TaskEntity>().HasOne(t => t.CompletedBy).WithMany(u => u.CompletedTasks).HasForeignKey(t => t.CompletedById).OnDelete(DeleteBehavior.NoAction);


        // modelBuilder.Entity<TaskEntity>(t =>
        //    {
        //        t.HasOne<UserEntity>()
        //         .WithMany()
        //         .HasForeignKey(t => t.CompletedById)
        //         .OnDelete(DeleteBehavior.NoAction);
        //    });



        // modelBuilder.Entity<TaskEntity>().HasOne(t => t.CompletedBy).WithMany().HasForeignKey(t => t.CompletedById).OnDelete(DeleteBehavior.Cascade);

        // One-to-Many: A User has many groups, while groups can be owned by only one user
        modelBuilder.Entity<UserEntity>().HasMany(u => u.OwnedGroups).WithOne(tg => tg.OwnerUser).HasForeignKey(tg => tg.OwnerUserId).OnDelete(DeleteBehavior.Cascade);

        // One-to-Many: A User has many refresh tokens, while refresh token can be owned by only one user
        modelBuilder.Entity<UserEntity>().HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rf => rf.UserId).OnDelete(DeleteBehavior.Cascade);

        // // Many-to-Many: A group can be accessed by multiple users, while users can have access to multiple group
        // modelBuilder.Entity<UserEntity>().HasMany(u => u.AccessibleGroups).WithMany(tg => tg.Members);

        // One-to-Many: A User has many groups, while groups can be owned by only one user
        modelBuilder.Entity<GroupMemberEntity>().HasOne(gm => gm.User).WithMany(u => u.GroupMemberships).HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GroupMemberEntity>().HasOne(gm => gm.Group).WithMany(g => g.GroupMembers).HasForeignKey(u => u.GroupId).OnDelete(DeleteBehavior.Cascade);




        modelBuilder.Entity<DeletedRecord>(b =>
           {
               b.HasOne<GroupEntity>()
                .WithMany()
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.NoAction);
           });

        base.OnModelCreating(modelBuilder);
    }
}