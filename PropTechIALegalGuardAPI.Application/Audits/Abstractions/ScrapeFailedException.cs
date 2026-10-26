namespace PropTechIALegalGuardAPI.Application.Audits.Abstractions
{
    /// <summary>
    /// Expected error (site down, timeout, not HTML): the message can be displayed to the user
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="inner">The inner exception</param>
    public sealed class ScrapeFailedException(string message, Exception? inner = null) : Exception(message, inner);
}
