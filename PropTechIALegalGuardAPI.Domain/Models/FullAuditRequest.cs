namespace PropTechIALegalGuardAPI.Domain.Models
{
    public class FullAuditRequest
    {
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Includes deep WCAG 2.1 / EN 301 549 scanning.
        /// </summary>
        public bool DeepScanAccessibility { get; set; }

        /// <summary>
        /// Verifies cookie consent banners, legal notices, and privacy policies.
        /// </summary>
        public bool CheckRgpdAndCookies { get; set; }

    }
}
