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
    public sealed class AuditsClient : IAuditsClient
    {
        private readonly HttpClient _httpClient;

        public AuditsClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

		public async Task<PropTechIALegalGuardAPI.Domain.Models.AuditTaskAcceptedResponse> RequestExpressAuditAsync(PropTechIALegalGuardAPI.Domain.Models.ExpressAuditRequest request, CancellationToken cancellationToken = default)
		{
			var path = $"audits/express";
			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path);
			httpRequest.Content = JsonContent.Create(request);
			using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<PropTechIALegalGuardAPI.Domain.Models.AuditTaskAcceptedResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

			return result!;
		}

		public async Task<PropTechIALegalGuardAPI.Domain.Models.AuditTaskAcceptedResponse> RequestFullAuditAsync(PropTechIALegalGuardAPI.Domain.Models.FullAuditRequest request, CancellationToken cancellationToken = default)
		{
			var path = $"audits/full";
			using var httpRequest = new HttpRequestMessage(HttpMethod.Post, path);
			httpRequest.Content = JsonContent.Create(request);
			using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<PropTechIALegalGuardAPI.Domain.Models.AuditTaskAcceptedResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

			return result!;
		}

		public async Task<PropTechIALegalGuardAPI.Domain.Models.AuditReportDetailResponse> GetAuditStatusAsync(Guid auditId, CancellationToken cancellationToken = default)
		{
			var path = $"audits/{auditId}";
			using var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
			using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var result = await response.Content.ReadFromJsonAsync<PropTechIALegalGuardAPI.Domain.Models.AuditReportDetailResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

			return result!;
		}
    }
}
