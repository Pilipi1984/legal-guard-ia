namespace PropTechIALegalGuardAPI.Application.Audits.Abstractions
{
    /// <summary>
    /// The PageSignals record represents various signals and metrics extracted from a web page during scraping. 
    /// It includes information about images, links, buttons, inputs, forms, meta tags, headings, and legal-related URLs. 
    /// This data can be used for accessibility audits, SEO analysis, and compliance checks.
    /// </summary>
    /// <param name="ImagesTotal">The total number of images on the page.</param>
    /// <param name="ImagesWithoutAlt">The number of images without alt text.</param>
    /// <param name="LinksWithoutName">The number of links without descriptive names.</param>
    /// <param name="ButtonsWithoutName">The number of buttons without descriptive names.</param>
    /// <param name="InputsWithoutLabel">The number of input fields without associated labels.</param>
    /// <param name="FormCount">The total number of forms on the page.</param>
    /// <param name="HasViewportMeta">Indicates whether the page has a viewport meta tag.</param>
    /// <param name="HasSkipLink">Indicates whether the page has a skip link.</param>
    /// <param name="HeadingLevels">The levels of headings used on the page.</param>
    /// <param name="PrivacyPolicyUrl">The URL of the privacy policy.</param>
    /// <param name="CookiePolicyUrl">The URL of the cookie policy.</param>
    /// <param name="LegalNoticeUrl">The URL of the legal notice.</param>
    /// <param name="TermsUrl">The URL of the terms of service.</param>
    /// <param name="CookieBannerDetected">Indicates whether a cookie banner was detected.</param>
    public sealed record PageSignals(
        int ImagesTotal,
        int ImagesWithoutAlt,
        int LinksWithoutName,
        int ButtonsWithoutName,
        int InputsWithoutLabel,
        int FormCount,
        bool HasViewportMeta,
        bool HasSkipLink,
        IReadOnlyList<int> HeadingLevels,
        string? PrivacyPolicyUrl,
        string? CookiePolicyUrl,
        string? LegalNoticeUrl,
        string? TermsUrl,
        bool CookieBannerDetected);
}
