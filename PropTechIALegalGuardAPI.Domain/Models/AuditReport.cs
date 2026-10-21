using PropTechIALegalGuardAPI.Domain.Enums;

namespace PropTechIALegalGuardAPI.Domain.Models;

public sealed class AuditReport
{
    private AuditReport() { } // EF Core

    public Guid Id { get; private set; }
    public Guid AuditRequestId { get; private set; }
    public int Score { get; private set; }                       // 0-100
    public string Summary { get; private set; } = default!;
    public List<Finding> Findings { get; private set; } = [];    // jsonb
    public string? PdfStorageKey { get; private set; }           // clave en blob storage (URL prefirmada bajo demanda)
    public DateTime GeneratedAtUtc { get; private set; }

    public static AuditReport Create(Guid auditId, string summary, IEnumerable<Finding> findings, TimeProvider? clock = null)
    {
        var list = findings.ToList();
        return new()
        {
            Id = Guid.CreateVersion7(),
            AuditRequestId = auditId,
            Summary = summary,
            Findings = list,
            Score = CalculateScore(list),
            GeneratedAtUtc = (clock ?? TimeProvider.System).GetUtcNow().UtcDateTime
        };
    }

    public void SetPdf(string storageKey) => PdfStorageKey = storageKey;

    private static int CalculateScore(IReadOnlyCollection<Finding> f)
    {
        var penalty = f.Sum(x => x.Severity switch
        {
            Severity.Critical => 20, Severity.High => 10, Severity.Medium => 5, Severity.Low => 2, _ => 0
        });
        return Math.Max(0, 100 - penalty);
    }
}

public sealed record Finding(
    LegalFramework Framework,
    string RuleCode,
    Severity Severity,
    string Description,
    string? Evidence,
    string Recommendation);
