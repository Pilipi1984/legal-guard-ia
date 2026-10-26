namespace PropTechIALegalGuardAPI.Application.Audits.Interfaces
{
    /// <summary>
    /// Phase 4. Sends the result to the user.
    /// </summary>
    public interface INotifier
    {
        /// <summary>
        /// Sends an email to the user notifying them that their audit report is ready for download.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="auditId">The ID of the audit.</param>
        /// <param name="websiteUrl">The URL of the website that was audited.</param>
        /// <param name="downloadUrl">The URL where the audit report can be downloaded.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendReportReadyAsync(string email, Guid auditId, string websiteUrl, string downloadUrl, CancellationToken ct);
        /// <summary>
        /// Sends an email to the user notifying them that their audit report is ready for download.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="websiteUrl">The URL of the website that was audited.</param>
        /// <param name="userMessage">The message for the user.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAuditFailedAsync(string email, string websiteUrl, string userMessage, CancellationToken ct);
    }
}
