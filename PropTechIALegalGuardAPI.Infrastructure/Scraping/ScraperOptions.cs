namespace PropTechIALegalGuardAPI.Infrastructure.Scraping
{
    public sealed class ScraperOptions
    {
        public const string Section = "Scraping";
        public int NavigationTimeoutMs { get; set; } = 30_000;
        public int NetworkIdleWaitMs { get; set; } = 8_000;
        public int TotalTimeoutSeconds { get; set; } = 60;
        public int MaxHtmlChars { get; set; } = 200_000;
        public int MaxAriaChars { get; set; } = 30_000;
        public int MaxConsoleMessages { get; set; } = 50;
        public string UserAgent { get; set; } =
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0 Safari/537.36 LegalGuardBot/1.0 (+https://legalguard.example/bot)";
    }
}
