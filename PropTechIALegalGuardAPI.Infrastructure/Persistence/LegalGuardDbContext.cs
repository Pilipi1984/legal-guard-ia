using Microsoft.EntityFrameworkCore;
using PropTechIALegalGuardAPI.Domain.Models;

namespace PropTechIALegalGuardAPI.Infrastructure.Persistence;

public sealed class LegalGuardDbContext(DbContextOptions<LegalGuardDbContext> options) : DbContext(options)
{
    public DbSet<AuditRequest> AuditRequests => Set<AuditRequest>();
    public DbSet<AuditReport> AuditReports => Set<AuditReport>();
    public DbSet<LegalRule> LegalRules => Set<LegalRule>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("vector");

        b.Entity<AuditRequest>(e =>
        {
            e.ToTable("audit_requests");
            e.HasKey(x => x.Id);
            e.Property(x => x.WebsiteUrl).HasMaxLength(2048).IsRequired();
            e.Property(x => x.Email).HasMaxLength(320).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.Status, x.CreatedAtUtc });
            e.HasOne(x => x.Report).WithOne().HasForeignKey<AuditReport>(r => r.AuditRequestId);
        });

        b.Entity<AuditReport>(e =>
        {
            e.ToTable("audit_reports");
            e.HasKey(x => x.Id);
            e.OwnsMany(x => x.Findings, o => o.ToJson());   // jsonb
        });

        b.Entity<LegalRule>(e =>
        {
            e.ToTable("legal_rules");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Code);
            e.Property(x => x.Framework).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Embedding).HasColumnType("vector(1536)");
            e.HasIndex(x => x.Embedding).HasMethod("hnsw").HasOperators("vector_cosine_ops");
        });

        b.Entity<UserSubscription>(e =>
        {
            e.ToTable("user_subscriptions");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.StripeCustomerId).IsUnique();
            e.HasIndex(x => x.Email);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
