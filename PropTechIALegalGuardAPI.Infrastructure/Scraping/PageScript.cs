namespace PropTechIALegalGuardAPI.Infrastructure.Scraping
{
    /// <summary>JavaScript que se ejecuta dentro de la página. Devuelve objetos JSON-serializables.</summary>
    internal static class PageScript
    {
        public const string CompactDom = """
        () => {
          const c = document.documentElement.cloneNode(true);
          c.querySelectorAll('script,style,svg,noscript,link,template,iframe').forEach(n => n.remove());
          const w = document.createTreeWalker(c, NodeFilter.SHOW_COMMENT);
          const comments = []; while (w.nextNode()) comments.push(w.currentNode);
          comments.forEach(n => n.remove());
          return c.outerHTML.replace(/\s+/g, ' ');
        }
        """;

        public const string Signals = """
        () => {
          const q = s => Array.from(document.querySelectorAll(s));
          const text = e => (e.innerText || e.textContent || '').trim();
          const name = e => (e.getAttribute('aria-label') || e.getAttribute('aria-labelledby') || e.getAttribute('title')
              || text(e) || e.querySelector('img[alt]')?.getAttribute('alt') || '').trim();
          const findLink = re => { const a = q('a[href]').find(x => re.test(text(x)) || re.test(x.getAttribute('href') || '')); return a ? a.href : null; };
          const inputs = q('input:not([type=hidden]):not([type=submit]):not([type=button]):not([type=image]), select, textarea');
          const unlabeled = inputs.filter(i => !(i.labels && i.labels.length) && !i.getAttribute('aria-label')
              && !i.getAttribute('aria-labelledby') && !i.getAttribute('title'));
          const imgs = q('img');
          const bannerRe = /cookie|consent|gdpr|rgpd|onetrust|cookiebot|didomi|quantcast/i;
          const banner = q('[id],[class]').some(e => bannerRe.test(e.id || '') || bannerRe.test(String(e.className || '')));
          return {
            imagesTotal: imgs.length,
            imagesWithoutAlt: imgs.filter(i => !i.hasAttribute('alt')).length,
            linksWithoutName: q('a[href]').filter(a => !name(a)).length,
            buttonsWithoutName: q('button,[role=button]').filter(b => !name(b)).length,
            inputsWithoutLabel: unlabeled.length,
            formCount: q('form').length,
            hasViewportMeta: !!document.querySelector('meta[name=viewport]'),
            hasSkipLink: q('a[href^="#"]').slice(0, 5).some(a => /saltar|skip|contenido|content/i.test(text(a))),
            headingLevels: q('h1,h2,h3,h4,h5,h6').map(h => parseInt(h.tagName[1], 10)),
            privacyPolicyUrl: findLink(/privacidad|privacy/i),
            cookiePolicyUrl: findLink(/cookies/i),
            legalNoticeUrl: findLink(/aviso\s+legal|legal\s+notice|informaci[oó]n\s+legal/i),
            termsUrl: findLink(/t[eé]rminos|condiciones|terms/i),
            cookieBannerDetected: banner
          };
        }
        """;
    }
}
