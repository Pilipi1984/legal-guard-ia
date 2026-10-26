namespace PropTechIALegalGuardAPI.Application.Audits.Abstractions
{
    /// <summary>
    /// Entry in the browser console, captured during the page scrape. The level is typically "log", "info", "warn", or "error".
    /// </summary>
    /// <param name="Level">The level of the console entry</param>
    /// <param name="Text">The text of the console entry</param>
    public sealed record ConsoleEntry(string Level, string Text);
}
