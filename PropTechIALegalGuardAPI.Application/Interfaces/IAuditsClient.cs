using System.Threading;
using System.Threading.Tasks;

using PropTechIALegalGuardAPI.Domain.Models;

namespace PropTechIALegalGuardAPI.Application.Interfaces
{
	public interface IAuditsClient
	{
		/// <summary>
		/// Request free express audit
		/// </summary>
		Task<PropTechIALegalGuardAPI.Domain.Models.AuditTaskAcceptedResponse> RequestExpressAuditAsync(PropTechIALegalGuardAPI.Domain.Models.ExpressAuditRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Request comprehensive full audit
		/// </summary>
		Task<PropTechIALegalGuardAPI.Domain.Models.AuditTaskAcceptedResponse> RequestFullAuditAsync(PropTechIALegalGuardAPI.Domain.Models.FullAuditRequest request, CancellationToken cancellationToken = default);

		/// <summary>
		/// Get audit status or report result
		/// </summary>
		Task<PropTechIALegalGuardAPI.Domain.Models.AuditReportDetailResponse> GetAuditStatusAsync(Guid auditId, CancellationToken cancellationToken = default);

	}
}
