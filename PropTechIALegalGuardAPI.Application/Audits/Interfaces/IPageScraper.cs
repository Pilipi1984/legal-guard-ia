using PropTechIALegalGuardAPI.Application.Audits.Abstractions;

namespace PropTechIALegalGuardAPI.Application.Audits.Interfaces
{
    /// <summary>
    /// Phase 1. Throws <see cref="ScrapeFailedException"/> in expected scenarios.
    /// </summary>
    public interface IPageScraper
    {
        /// <summary>
        /// Scrapes the page at the given URL and returns a <see cref="ScrapedPage"/> object containing the scraped data.
        /// </summary>
        /// <param name="url">The URL of the page to scrape.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The scraped page data.</returns>
        Task<ScrapedPage> ScrapeAsync(string url, CancellationToken ct);
    }
}
