using System;
using Microsoft.Extensions.DependencyInjection;
using PropTechIALegalGuardAPI.Application.Interfaces;
using PropTechIALegalGuardAPI.Infrastructure.Clients;

namespace PropTechIALegalGuardAPI.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPropTechIALegalGuardAPIClients(this IServiceCollection services, Uri baseAddress)
        {
            services.AddHttpClient<IAuditsClient, AuditsClient>(client =>
            {
                client.BaseAddress = baseAddress;
            });
            services.AddHttpClient<IWebhooksClient, WebhooksClient>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            return services;
        }
    }
}
