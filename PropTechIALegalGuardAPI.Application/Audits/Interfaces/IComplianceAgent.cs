using PropTechIALegalGuardAPI.Application.Audits.Abstractions;
using PropTechIALegalGuardAPI.Domain.Enums;

namespace PropTechIALegalGuardAPI.Application.Audits.Interfaces
{
    /// <summary>
    /// Phase 2. Agent (Semantic Kernel + plugins + RAG) that produces findings.
    /// </summary>
    public interface IComplianceAgent
    {
        Task<ComplianceAnalysis> AnalyzeAsync(ScrapedPage page, AuditType type, CancellationToken ct);
    }
}
