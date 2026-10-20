namespace PropTechIALegalGuardAPI.Domain.Models
{
    public class AuditReportDetailResponse
    {
        public Guid AuditId { get; set; }

        public string WebsiteUrl { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Overall legal compliance score (0-100).
        /// </summary>
        public int OverallScore { get; set; }

        public string Summary { get; set; } = string.Empty;

        public object IssuesFound { get; set; }

        public object PdfDownloadUrl { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public object CompletedAt { get; set; }

    }
}
