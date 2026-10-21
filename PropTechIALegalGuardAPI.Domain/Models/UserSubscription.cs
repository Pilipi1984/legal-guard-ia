using PropTechIALegalGuardAPI.Domain.Enums;

namespace PropTechIALegalGuardAPI.Domain.Models;

public sealed class UserSubscription
{
    private UserSubscription() { } // EF Core

    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string StripeCustomerId { get; private set; } = default!;
    public string? StripeSubscriptionId { get; private set; }
    public string Plan { get; private set; } = "free";
    public SubscriptionStatus Status { get; private set; }
    public DateTime? CurrentPeriodEndUtc { get; private set; }
    public int MonthlyAuditQuota { get; private set; }
    public int AuditsUsedThisPeriod { get; private set; }
    public DateTime LastStripeEventUtc { get; private set; }      // idempotencia / orden de webhooks

    public static UserSubscription Create(string email, string stripeCustomerId) => new()
    {
        Id = Guid.CreateVersion7(), Email = email.Trim().ToLowerInvariant(),
        StripeCustomerId = stripeCustomerId, Status = SubscriptionStatus.Incomplete
    };

    public void Activate(string subscriptionId, string plan, int quota, DateTime periodEndUtc, DateTime eventUtc)
    {
        StripeSubscriptionId = subscriptionId; Plan = plan; MonthlyAuditQuota = quota;
        CurrentPeriodEndUtc = periodEndUtc; Status = SubscriptionStatus.Active;
        AuditsUsedThisPeriod = 0; LastStripeEventUtc = eventUtc;
    }

    public void Cancel(DateTime eventUtc) { Status = SubscriptionStatus.Canceled; LastStripeEventUtc = eventUtc; }

    public bool IsActive(DateTime nowUtc) =>
        Status == SubscriptionStatus.Active && CurrentPeriodEndUtc is { } end && end > nowUtc;

    public bool TryConsumeAudit(DateTime nowUtc)
    {
        if (!IsActive(nowUtc) || AuditsUsedThisPeriod >= MonthlyAuditQuota) return false;
        AuditsUsedThisPeriod++;
        return true;
    }
}
