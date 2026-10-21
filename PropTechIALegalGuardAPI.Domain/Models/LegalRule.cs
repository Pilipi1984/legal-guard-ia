using Pgvector;
using PropTechIALegalGuardAPI.Domain.Enums;

namespace PropTechIALegalGuardAPI.Domain.Models;

/// <summary>Fragmento normativo vigente indexado para RAG.</summary>
public sealed class LegalRule
{
    private LegalRule() { } // EF Core

    public Guid Id { get; private set; }
    public LegalFramework Framework { get; private set; }
    public string Code { get; private set; } = default!;          // p. ej. "RGPD-art13", "EN301549-9.1.1.1"
    public string Title { get; private set; } = default!;
    public string Text { get; private set; } = default!;
    public string SourceUrl { get; private set; } = default!;     // BOE / EUR-Lex
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public Vector Embedding { get; private set; } = default!;     // vector(1536)

    public static LegalRule Create(LegalFramework fw, string code, string title, string text,
        string sourceUrl, DateOnly from, float[] embedding) => new()
    {
        Id = Guid.CreateVersion7(), Framework = fw, Code = code, Title = title, Text = text,
        SourceUrl = sourceUrl, EffectiveFrom = from, Embedding = new Vector(embedding)
    };

    public void Repeal(DateOnly on) => EffectiveTo = on;
    public bool IsActiveOn(DateOnly d) => d >= EffectiveFrom && (EffectiveTo is null || d < EffectiveTo);
}
