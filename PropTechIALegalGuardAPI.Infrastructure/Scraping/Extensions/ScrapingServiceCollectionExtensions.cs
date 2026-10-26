using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PropTechIALegalGuardAPI.Application.Audits.Interfaces;

namespace PropTechIALegalGuardAPI.Infrastructure.Scraping.Extensions
{
    public static class ScrapingServiceCollectionExtensions
    {
        public static IServiceCollection AddLegalGuardScraping(this IServiceCollection services, IConfiguration cfg)
        {
            services.Configure<ScraperOptions>(cfg.GetSection(ScraperOptions.Section));
            services.AddSingleton<IPageScraper, PlaywrightPageScraper>();
            return services;
        }
    }
}
