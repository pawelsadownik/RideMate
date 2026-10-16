using Microsoft.EntityFrameworkCore;
using RideMate.Domain.Common;
using RideMate.Domain.Entities;
using System.Linq.Expressions;

namespace RideMate.Infrastructure.Persistence;

public sealed class RideMateDbContext : DbContext
{
    public RideMateDbContext(DbContextOptions<RideMateDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<ProfilePhoto> ProfilePhotos { get; set; }
    public DbSet<RidingPreferences> RidingPreferences { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Block> Blocks { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<SubscriptionEntitlement> SubscriptionEntitlements { get; set; }
    public DbSet<PushToken> PushTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Indeksy i unikalności
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.IsDeleted);
            e.HasIndex(u => u.LastActiveAt);
            e.HasIndex(u => u.EmailConfirmationToken);
            e.HasIndex(u => u.PasswordResetToken);
        });

        modelBuilder.Entity<Profile>(e =>
        {
            e.HasIndex(p => p.UserId).IsUnique();
            e.HasIndex(p => p.IsActive);
            e.HasIndex(p => p.City);
            e.HasIndex(p => new { p.Latitude, p.Longitude }); // wyszukiwanie lokalizacji
            e.HasIndex(p => p.ActivityType);
            e.HasIndex(p => p.LookingFor);
            e.HasOne(p => p.User).WithOne(u => u.Profile).HasForeignKey<Profile>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfilePhoto>(e =>
        {
            e.HasIndex(p => p.ProfileId);
            e.HasIndex(p => new { p.ProfileId, p.IsPrimary });
            e.HasOne(p => p.Profile).WithMany(p => p.Photos).HasForeignKey(p => p.ProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RidingPreferences>(e =>
        {
            e.HasIndex(p => p.ProfileId).IsUnique();
            e.HasOne(p => p.Profile).WithOne(p => p.RidingPreferences).HasForeignKey<RidingPreferences>(p => p.ProfileId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(e =>
        {
            e.HasIndex(l => new { l.FromUserId, l.ToUserId }).IsUnique(); // brak duplikatów
            e.HasIndex(l => l.ToUserId);
            e.HasIndex(l => l.Type);
            e.HasOne(l => l.FromUser).WithMany(u => u.SentLikes).HasForeignKey(l => l.FromUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(l => l.ToUser).WithMany(u => u.ReceivedLikes).HasForeignKey(l => l.ToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Match>(e =>
        {
            e.HasIndex(m => new { m.User1Id, m.User2Id }).IsUnique(); // jedna para = jedno dopasowanie
            e.HasIndex(m => m.IsActive);
            e.HasOne(m => m.User1).WithMany().HasForeignKey(m => m.User1Id).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.User2).WithMany().HasForeignKey(m => m.User2Id).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Conversation).WithOne(c => c.Match).HasForeignKey<Conversation>(c => c.MatchId);
        });

        modelBuilder.Entity<Conversation>(e =>
        {
            e.HasOne(c => c.Match).WithOne(m => m.Conversation).HasForeignKey<Conversation>(c => c.MatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(c => c.MatchId);
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.HasIndex(m => m.ConversationId);
            e.HasIndex(m => m.SenderUserId);
            e.HasIndex(m => m.ReadAt);
            e.HasOne(m => m.Conversation).WithMany(c => c.Messages).HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender).WithMany(u => u.Messages).HasForeignKey(m => m.SenderUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Block>(e =>
        {
            e.HasIndex(b => new { b.BlockerUserId, b.BlockedUserId }).IsUnique();
            e.HasIndex(b => b.BlockedUserId);
            e.HasOne(b => b.Blocker).WithMany(u => u.BlocksAsBlocker).HasForeignKey(b => b.BlockerUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Blocked).WithMany(u => u.BlocksAsBlocked).HasForeignKey(b => b.BlockedUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Report>(e =>
        {
            e.HasIndex(r => r.Status);
            e.HasIndex(r => r.ReportedUserId);
            e.HasIndex(r => r.ReporterUserId);
            e.HasIndex(r => r.MessageId);
            e.HasOne(r => r.Reporter).WithMany(u => u.ReportsAsReporter).HasForeignKey(r => r.ReporterUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Reported).WithMany(u => u.ReportsAsReported).HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Message).WithMany().HasForeignKey(r => r.MessageId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SubscriptionEntitlement>(e =>
        {
            e.HasIndex(s => s.UserId).IsUnique();
            e.HasIndex(s => s.IsPremium);
            e.HasOne(s => s.User).WithOne(u => u.Subscription).HasForeignKey<SubscriptionEntitlement>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PushToken>(e =>
        {
            e.HasIndex(p => p.Token).IsUnique();
            e.HasIndex(p => p.UserId);
            e.HasOne(p => p.User).WithMany(u => u.PushTokens).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // Miękkie usuwanie — domyślny filtr na IsDeleted
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                entityType.SetQueryFilter(ExpressionHelper.IsDeletedFalse(entityType.ClrType));
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(ct);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    entry.Entity.Touch();
                    break;
            }
        }
    }
}

internal static class ExpressionHelper
{
    public static LambdaExpression IsDeletedFalse(Type type)
    {
        var param = Expression.Parameter(type, "x");
        var isDeleted = Expression.PropertyOrField(param, nameof(BaseEntity.IsDeleted));
        var falseExpr = Expression.Constant(false);
        var body = Expression.Equal(isDeleted, falseExpr);
        return Expression.Lambda(body, param);
    }
}
