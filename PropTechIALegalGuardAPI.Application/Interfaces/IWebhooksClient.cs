using System.Threading;
using System.Threading.Tasks;

using PropTechIALegalGuardAPI.Domain.Models;

namespace PropTechIALegalGuardAPI.Application.Interfaces
{
	public interface IWebhooksClient
	{
		/// <summary>
		/// Stripe Billing Integration Webhook
		/// </summary>
		Task<object> HandleStripeWebhookAsync(string stripeSignature, object request, CancellationToken cancellationToken = default);

	}
}
