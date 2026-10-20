using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using PropTechIALegalGuardAPI.Domain.Models;
using PropTechIALegalGuardAPI.Application.Interfaces;

namespace PropTechIALegalGuardAPI.Infrastructure.Clients
{
    public sealed class WebhooksClient : IWebhooksClient
    {
        private readonly HttpClient _httpClient;

        public WebhooksClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

		public async Task<object> HandleStripeWebhookAsync(string stripeSignature, object request, CancellationToken cancellationToken = default)
		{
			var path = $"webhooks/stripe";
			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path);
			if (stripeSignature is not null)
			{
				httpRequest.Headers.TryAddWithoutValidation("Stripe-Signature", stripeSignature.ToString());
			}

			httpRequest.Content = JsonContent.Create(request);
			using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<object>(cancellationToken: cancellationToken).ConfigureAwait(false);

			return result!;
		}
    }
}
