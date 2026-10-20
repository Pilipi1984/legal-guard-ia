namespace PropTechIALegalGuardAPI.Domain.Models
{
    public class AuditIssue
    {
        public string Category { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public object SuggestedFixSnippet { get; set; }

    }
}
