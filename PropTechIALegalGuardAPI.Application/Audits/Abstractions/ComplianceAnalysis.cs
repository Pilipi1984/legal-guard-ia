using PropTechIALegalGuardAPI.Domain.Models;

namespace PropTechIALegalGuardAPI.Application.Audits.Abstractions
{
    public sealed record ComplianceAnalysis(string Summary, IReadOnlyList<Finding> Findings);
}
