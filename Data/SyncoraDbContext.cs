using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public DbSet<VerificationTokenEntity> VerificationTokens { get; set; }
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens { get; set; }
    public DbSet<ReportEntity> Reports { get; set; }
    public DbSet<AdminActionEntity> AdminActions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // One-to-Many: A Task has one group, while the group can own multiple tasks
        modelBuilder.Entity<TaskEntity>().HasOne(t => t.Group).WithMany(tg => tg.Tasks).HasForeignKey(t => t.GroupId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskEntity>().HasMany(t => t.AssignedTo).WithMany(u => u.AssignedTasks);
        modelBuilder.Entity<TaskEntity>().HasOne(t => t.CompletedBy).WithMany(u => u.CompletedTasks).HasForeignKey(t => t.CompletedById).OnDelete(DeleteBehavior.SetNull);

        // One-to-Many: A User has many groups, while groups can be owned by only one user
        modelBuilder.Entity<UserEntity>().HasMany(u => u.OwnedGroups).WithOne(tg => tg.OwnerUser).HasForeignKey(tg => tg.OwnerUserId).OnDelete(DeleteBehavior.Cascade);

        // One-to-Many: A User has many verification tokens (only one can be active), while verification token can be owned by only one user
        modelBuilder.Entity<UserEntity>().HasMany(u => u.VerificationTokens).WithOne(rt => rt.User).HasForeignKey(vt => vt.UserId).OnDelete(DeleteBehavior.Cascade);

        // One-to-Many: A User has many reset password tokens (only one can be active), while reset password token can be owned by only one user
        modelBuilder.Entity<UserEntity>().HasMany(u => u.PasswordResetTokens).WithOne(rt => rt.User).HasForeignKey(rp => rp.UserId).OnDelete(DeleteBehavior.Cascade);

        // One-to-Many: A User has many refresh tokens, while refresh token can be owned by only one user
        modelBuilder.Entity<UserEntity>().HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rf => rf.UserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReportEntity>().HasOne(r => r.User).WithMany(u => u.Reports).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);


        // The report entity will never be updated once inserted, so the value comparer is set to no-op
        var JsonSerializerOptions = (JsonSerializerOptions?)null;
        modelBuilder.Entity<ReportEntity>()
            .Property(e => e.Breadcrumbs)
            .HasConversion(b => JsonSerializer.Serialize(b, JsonSerializerOptions), b => JsonSerializer.Deserialize<Dictionary<string, object>[]>(b, JsonSerializerOptions) ?? new Dictionary<string, object>[0]).Metadata.SetValueComparer(Comparers.arrayDictComparerNoOp);

        modelBuilder.Entity<ReportEntity>()
            .Property(e => e.UserSession)
            .HasConversion(us => JsonSerializer.Serialize(us, JsonSerializerOptions), us => JsonSerializer.Deserialize<Dictionary<string, object>>(us, JsonSerializerOptions) ?? new Dictionary<string, object>()).Metadata.SetValueComparer(Comparers.dictComparerNoOp);

        modelBuilder.Entity<ReportEntity>()
            .Property(e => e.AppState)
            .HasConversion(appState => JsonSerializer.Serialize(appState, JsonSerializerOptions), appState => JsonSerializer.Deserialize<Dictionary<string, object>>(appState, JsonSerializerOptions) ?? new Dictionary<string, object>()).Metadata.SetValueComparer(Comparers.dictComparerNoOp);


        modelBuilder.Entity<UserEntity>()
        .OwnsOne(u => u.Preferences, builder =>
        {
            builder.ToJson();
        });

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