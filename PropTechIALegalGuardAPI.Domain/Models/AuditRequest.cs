using PropTechIALegalGuardAPI.Domain.Enums;

namespace PropTechIALegalGuardAPI.Domain.Models;

public sealed class AuditRequest
{
    private AuditRequest() { } // EF Core

    public Guid Id { get; private set; }
    public string WebsiteUrl { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public AuditType Type { get; private set; }
    public AuditStatus Status { get; private set; }
    public Guid? SubscriptionId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string? FailureReason { get; private set; }
    public AuditReport? Report { get; private set; }

    public static AuditRequest Create(string websiteUrl, string email, AuditType type,
        Guid? subscriptionId = null, TimeProvider? clock = null) => new()
    {
        Id = Guid.CreateVersion7(),
        WebsiteUrl = websiteUrl,
        Email = email.Trim().ToLowerInvariant(),
        Type = type,
        SubscriptionId = subscriptionId,
        Status = AuditStatus.Queued,
        CreatedAtUtc = (clock ?? TimeProvider.System).GetUtcNow().UtcDateTime
    };

    public void Advance(AuditStatus next, TimeProvider? clock = null)
    {
        if (Status is AuditStatus.Completed or AuditStatus.Failed)
            throw new InvalidOperationException($"La auditoría ya está en estado final ({Status}).");
        var now = (clock ?? TimeProvider.System).GetUtcNow().UtcDateTime;
        if (Status == AuditStatus.Queued) StartedAtUtc = now;
        Status = next;
        if (next == AuditStatus.Completed) CompletedAtUtc = now;
    }

    public void Fail(string reason, TimeProvider? clock = null)
    {
        Status = AuditStatus.Failed;
        FailureReason = reason.Length > 500 ? reason[..500] : reason;
        CompletedAtUtc = (clock ?? TimeProvider.System).GetUtcNow().UtcDateTime;
    }

    public void AttachReport(AuditReport report) => Report = report;
}
