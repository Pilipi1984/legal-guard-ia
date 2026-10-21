using PropTechIALegalGuardAPI.Domain.Enums;

namespace PropTechIALegalGuardAPI.Application.Audits;

/// <summary>
/// Message that travels through the Channel to the worker.
/// </summary>
public sealed record AuditTask(Guid AuditId, string WebsiteUrl, AuditType Type);
