using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;

namespace PropTechIALegalGuardAPI.Infrastructure.AI.Plugins;

/// <summary>Plugins nativos deterministas que el agente puede invocar (reduce alucinaciones).</summary>
public sealed partial class HeadingValidatorPlugin
{
    [KernelFunction, Description("Valida la jerarquía de encabezados H1-H6 de un HTML. Devuelve incidencias.")]
    public string ValidateHeadings([Description("HTML del DOM")] string html)
    {
        var levels = HeadingRegex().Matches(html).Select(m => int.Parse(m.Groups[1].Value)).ToList();
        var issues = new List<string>();
        if (levels.Count(l => l == 1) != 1) issues.Add($"Se esperaba un único H1; hay {levels.Count(l => l == 1)}.");
        for (var i = 1; i < levels.Count; i++)
            if (levels[i] - levels[i - 1] > 1) issues.Add($"Salto de nivel H{levels[i - 1]} -> H{levels[i]}.");
        return issues.Count == 0 ? "OK" : string.Join(" | ", issues);
    }

    [GeneratedRegex(@"<h([1-6])\b", RegexOptions.IgnoreCase)]
    private static partial Regex HeadingRegex();
}

public sealed partial class LegalTextScannerPlugin
{
    [KernelFunction, Description("Detecta en el HTML la presencia de aviso legal, política de privacidad, cookies y datos LSSI (NIF, domicilio).")]
    public string ScanLegalPresence([Description("HTML del DOM")] string html)
    {
        bool Has(string pattern) => Regex.IsMatch(html, pattern, RegexOptions.IgnoreCase);
        return $"avisoLegal={Has("aviso\\s+legal")}; privacidad={Has("pol[ií]tica\\s+de\\s+privacidad")}; " +
               $"cookies={Has("pol[ií]tica\\s+de\\s+cookies")}; nif={Has("\\b[ABCDEFGHJNPQRSUVW]\\d{7}[0-9A-J]\\b|\\b\\d{8}[A-Z]\\b")}";
    }
}
