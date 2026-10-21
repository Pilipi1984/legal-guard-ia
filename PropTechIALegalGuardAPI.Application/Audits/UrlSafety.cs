using System.Net;
using System.Net.Sockets;

namespace PropTechIALegalGuardAPI.Application.Audits;

/// <summary>Protección SSRF: Playwright nunca debe navegar a redes internas.</summary>
public static class UrlSafety
{
    public static async Task<bool> IsPublicHttpUrlAsync(string raw, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme is not ("http" or "https")) return false;
        try
        {
            var addrs = await Dns.GetHostAddressesAsync(uri.DnsSafeHost, ct);
            return addrs.Length > 0 && addrs.All(IsPublic);
        }
        catch (SocketException) { return false; }
    }

    private static bool IsPublic(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip) || ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal) return false;
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = ip.GetAddressBytes();
            return !(b[0] == 10 || b[0] == 127 || b[0] == 0
                  || (b[0] == 172 && b[1] is >= 16 and <= 31)
                  || (b[0] == 192 && b[1] == 168)
                  || (b[0] == 169 && b[1] == 254));
        }
        return true;
    }
}
