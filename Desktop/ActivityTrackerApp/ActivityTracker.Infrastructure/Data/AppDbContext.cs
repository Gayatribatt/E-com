using System.Text.Json;
using ActivityTracker.Application.Interfaces;
using ActivityTracker.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ActivityTracker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = BuildAuditEntries();
        ApplyEntityMetadata();
        var result = await base.SaveChangesAsync(cancellationToken);
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    private List<AuditLog> BuildAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var entries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries().Where(x =>
                     x.Entity is not AuditLog &&
                     x.Entity is not ActivityLog &&
                     x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            entries.Add(CreateAuditEntry(entry));
        }

        return entries;
    }

    private AuditLog CreateAuditEntry(EntityEntry entry)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues[property.Metadata.Name] = property.CurrentValue;
                    break;
                case EntityState.Deleted:
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                    break;
                case EntityState.Modified when property.IsModified:
                    oldValues[property.Metadata.Name] = property.OriginalValue;
                    newValues[property.Metadata.Name] = property.CurrentValue;
                    break;
            }
        }

        return new AuditLog
        {
            UserId = _currentUserService.UserId,
            Action = entry.State switch
            {
                EntityState.Added => "Create",
                EntityState.Modified => "Update",
                EntityState.Deleted => "Delete",
                _ => "Unknown"
            },
            EntityName = entry.Entity.GetType().Name,
            EntityId = entry.Properties.FirstOrDefault(x => x.Metadata.IsPrimaryKey())?.CurrentValue?.ToString() ?? "N/A",
            OldValuesJson = JsonSerializer.Serialize(oldValues),
            NewValuesJson = JsonSerializer.Serialize(newValues),
            IpAddress = _currentUserService.IpAddress,
            TimestampUtc = DateTime.UtcNow
        };
    }

    private void ApplyEntityMetadata()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<TaskItem>().HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AuditLog>().Property(x => x.OldValuesJson).HasColumnType("nvarchar(max)");
        modelBuilder.Entity<AuditLog>().Property(x => x.NewValuesJson).HasColumnType("nvarchar(max)");
        base.OnModelCreating(modelBuilder);
    }
}
