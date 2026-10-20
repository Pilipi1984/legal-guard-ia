namespace PropTechIALegalGuardAPI.Domain.Models
{
    public class AuditTaskAcceptedResponse
    {
        public Guid AuditId { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public int EstimatedSeconds { get; set; }

    }
}
