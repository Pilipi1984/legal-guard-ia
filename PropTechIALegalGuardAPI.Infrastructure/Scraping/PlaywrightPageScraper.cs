using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using PropTechIALegalGuardAPI.Application.Audits;
using PropTechIALegalGuardAPI.Application.Audits.Abstractions;
using PropTechIALegalGuardAPI.Application.Audits.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PropTechIALegalGuardAPI.Infrastructure.Scraping
{
    /// <summary>
    /// Singleton: one single shared Chromium browser; each audit uses an isolated and disposable BrowserContext.
    /// </summary>
    public sealed class PlaywrightPageScraper(IOptions<ScraperOptions> options, ILogger<PlaywrightPageScraper> log)
        : IPageScraper, IAsyncDisposable
    {
        private readonly ScraperOptions _opt = options.Value;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private IPlaywright? _playwright;
        private IBrowser? _browser;

        public async Task<ScrapedPage> ScrapeAsync(string url, CancellationToken ct)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_opt.TotalTimeoutSeconds));

            var browser = await GetBrowserAsync(cts.Token);
            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = _opt.UserAgent,
                Locale = "es-ES",
                TimezoneId = "Europe/Madrid",
                ViewportSize = new ViewportSize { Width = 1366, Height = 768 },
                ServiceWorkers = ServiceWorkerPolicy.Block,
                IgnoreHTTPSErrors = false
            });
            context.SetDefaultTimeout(_opt.NavigationTimeoutMs);

            var thirdParty = new ConcurrentDictionary<string, byte>();
            var hostVerdicts = new ConcurrentDictionary<string, bool>();
            var pageHost = new Uri(url).Host;

            // Anti-SSRF a nivel de navegador: cada petición (incluidas redirecciones) se valida.
            await context.RouteAsync("**/*", async route =>
            {
                var req = route.Request;
                if (!Uri.TryCreate(req.Url, UriKind.Absolute, out var u) || u.Scheme is not ("http" or "https"))
                {
                    await route.ContinueAsync();
                    return;
                }

                if (!hostVerdicts.TryGetValue(u.Host, out var ok))
                {
                    ok = await UrlSafety.IsPublicHttpUrlAsync(req.Url, cts.Token);
                    hostVerdicts[u.Host] = ok;
                }
                if (!ok) { log.LogWarning("Petición bloqueada (red no pública): {Host}", u.Host); await route.AbortAsync(); return; }

                if (!IsFirstParty(u.Host, pageHost)) thirdParty.TryAdd(u.Host, 0);

                // Para auditar DOM/accesibilidad no hace falta descargar binarios pesados.
                if (req.ResourceType is "image" or "media" or "font") await route.AbortAsync();
                else await route.ContinueAsync();
            });

            var page = await context.NewPageAsync();
            var console = new List<ConsoleEntry>();
            void Add(string level, string text)
            {
                lock (console)
                    if (console.Count < _opt.MaxConsoleMessages)
                        console.Add(new ConsoleEntry(level, text.Length > 300 ? text[..300] : text));
            }
            page.Console += (_, m) => { if (m.Type is "error" or "warning") Add(m.Type, m.Text); };
            page.PageError += (_, e) => Add("pageerror", e);

            try
            {
                var response = await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                if (response is null) throw new ScrapeFailedException("No se obtuvo respuesta del sitio.");
                if (response.Status >= 400) throw new ScrapeFailedException($"El sitio respondió con el código HTTP {response.Status}.");
                if (response.Headers.TryGetValue("content-type", out var ctype) && !ctype.Contains("html", StringComparison.OrdinalIgnoreCase))
                    throw new ScrapeFailedException("La URL no devuelve una página HTML.");

                // Mejor esfuerzo: esperar a que se calme la red, sin bloquear si hay polling continuo.
                try { await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = _opt.NetworkIdleWaitMs }); }
                catch (TimeoutException) { /* continuamos con lo que haya */ }

                var html = await page.EvaluateAsync<string>(PageScript.CompactDom);
                if (html.Length > _opt.MaxHtmlChars) html = html[.._opt.MaxHtmlChars];

                var signalsJson = await page.EvaluateAsync<JsonElement>(PageScript.Signals);
                var signals = signalsJson.Deserialize<PageSignals>(JsonSerializerOptions.Web)
                              ?? throw new ScrapeFailedException("No se pudo analizar la página.");

                string? aria = null;
                try
                {
                    aria = await page.Locator("body").AriaSnapshotAsync();
                    if (aria.Length > _opt.MaxAriaChars) aria = aria[.._opt.MaxAriaChars];
                }
                catch (PlaywrightException ex) { log.LogDebug(ex, "AriaSnapshot no disponible"); }

                var cookies = (await context.CookiesAsync()).Select(c => new CookieInfo(
                    c.Name, c.Domain, c.Secure, c.HttpOnly, c.SameSite.ToString(),
                    c.Expires > 0 ? DateTimeOffset.FromUnixTimeSeconds((long)c.Expires).UtcDateTime : null)).ToList();

                string? lang = await page.EvaluateAsync<string?>("() => document.documentElement.lang || null");
                ConsoleEntry[] consoleCopy; lock (console) consoleCopy = [.. console];

                return new ScrapedPage(
                    RequestedUrl: url, FinalUrl: page.Url, StatusCode: response.Status,
                    Title: await page.TitleAsync(), Lang: lang, CompactHtml: html, AriaSnapshot: aria,
                    Signals: signals, CookiesBeforeConsent: cookies,
                    ThirdPartyHosts: [.. thirdParty.Keys.Order()], ConsoleMessages: consoleCopy,
                    CapturedAtUtc: DateTime.UtcNow);
            }
            catch (TimeoutException ex) { throw new ScrapeFailedException("El sitio tardó demasiado en responder.", ex); }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            { throw new ScrapeFailedException("La auditoría superó el tiempo máximo permitido."); }
            catch (PlaywrightException ex)
            {
                var first = ex.Message.Split('\n')[0];
                throw new ScrapeFailedException(first.Contains("ERR_NAME_NOT_RESOLVED")
                    ? "No se pudo resolver el dominio." : "No se pudo cargar el sitio web.", ex);
            }
        }

        private async Task<IBrowser> GetBrowserAsync(CancellationToken ct)
        {
            if (_browser is { IsConnected: true }) return _browser;
            await _initLock.WaitAsync(ct);
            try
            {
                if (_browser is { IsConnected: true }) return _browser;   // doble comprobación
                if (_browser is not null) { log.LogWarning("Chromium desconectado; relanzando."); await _browser.DisposeAsync(); }
                _playwright ??= await Playwright.CreateAsync();
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = ["--disable-dev-shm-usage"]
                });
                return _browser;
            }
            finally { _initLock.Release(); }
        }

        private static bool IsFirstParty(string host, string pageHost)
        {
            static string Strip(string h) => h.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? h[4..] : h;
            var root = Strip(pageHost);
            var h = Strip(host);
            return h.Equals(root, StringComparison.OrdinalIgnoreCase) || h.EndsWith("." + root, StringComparison.OrdinalIgnoreCase);
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser is not null) await _browser.DisposeAsync();
            _playwright?.Dispose();
            _initLock.Dispose();
        }
    }

}
