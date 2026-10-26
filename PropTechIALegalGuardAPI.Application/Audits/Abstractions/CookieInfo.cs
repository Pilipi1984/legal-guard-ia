namespace PropTechIALegalGuardAPI.Application.Audits.Abstractions
{
    /// <summary>
    /// The information about a cookie that was set during the scraping process.
    /// </summary>
    /// <param name="Name">The name of the cookie.</param>
    /// <param name="Domain">The domain for which the cookie is valid.</param>
    /// <param name="Secure">Indicates whether the cookie should only be sent over HTTPS.</param>
    /// <param name="HttpOnly">Indicates whether the cookie should be accessible only through HTTP.</param>
    /// <param name="SameSite">The SameSite attribute of the cookie.</param>
    /// <param name="ExpiresUtc">The UTC expiration time of the cookie.</param>
    public sealed record CookieInfo(string Name, string Domain, bool Secure, bool HttpOnly, string SameSite, DateTime? ExpiresUtc);
}
