using PropTechIALegalGuardAPI.Domain.Models;

namespace PropTechIALegalGuardAPI.Application.Audits.Interfaces
{
    /// <summary>
    /// Phase 3. Generates the PDF in memory.
    /// </summary>
    public interface IReportRenderer
    {
        /// <summary>
        /// Render the PDF
        /// </summary>
        /// <param name="audit">The audit request</param>
        /// <param name="report">The audit report</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>The generated PDF as a byte array</returns>
        Task<byte[]> RenderPdfAsync(AuditRequest audit, AuditReport report, CancellationToken ct);
    }
}
