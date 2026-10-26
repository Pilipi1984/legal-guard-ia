namespace PropTechIALegalGuardAPI.Application.Audits.Abstractions
{
    /// <summary>
    /// The result of scraping a page. This is the output of the <see cref="IPageScraper"/> interface.
    /// </summary>
    /// <param name="RequestedUrl">The URL that was requested</param>
    /// <param name="FinalUrl">The final URL after any redirects</param>
    /// <param name="StatusCode">The HTTP status code of the response</param>
    /// <param name="Title">The title of the page</param>
    /// <param name="Lang">The language of the page</param>
    /// <param name="CompactHtml">The renderized DOM without script/style/svg</param>
    /// <param name="AriaSnapshot">The accessibility tree (YAML)</param>
    /// <param name="Signals">The page signals</param>
    /// <param name="CookiesBeforeConsent">The cookies present without having interacted</param>
    /// <param name="ThirdPartyHosts">The third-party hosts contacted during loading</param>
    /// <param name="ConsoleMessages">The console messages</param>
    /// <param name="CapturedAtUtc">The timestamp when the page was captured</param>
    public sealed record ScrapedPage(
    string RequestedUrl,
    string FinalUrl,
    int StatusCode,
    string? Title,
    string? Lang,
    string CompactHtml,                              // Renderized DOM without script/style/svg
    string? AriaSnapshot,                            // Accessibility tree (YAML)
    PageSignals Signals,                             // determinist metrics calculated in the browser
    IReadOnlyList<CookieInfo> CookiesBeforeConsent,  // cookies present without having interacted
    IReadOnlyList<string> ThirdPartyHosts,           // third-party hosts contacted during loading
    IReadOnlyList<ConsoleEntry> ConsoleMessages,
    DateTime CapturedAtUtc);
}
