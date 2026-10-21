using System.Net.Mail;
using System.Threading.Channels;
using PropTechIALegalGuardAPI.Application.Audits;
using PropTechIALegalGuardAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using PropTechIALegalGuardAPI.Domain.Models;
using PropTechIALegalGuardAPI.Domain.Enums;

namespace PropTechIALegalGuardAPI.Client.Features.Audits
{
    public static class AuditEndpoints
    {
        public sealed record ExpressAuditRequest(string WebsiteUrl, string Email);

        public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
        {
            var g = app.MapGroup("/api/v1/audits").WithTags("Audits");

            g.MapPost("/express", async (ExpressAuditRequest req, LegalGuardDbContext db,
                    ChannelWriter<AuditTask> queue, CancellationToken ct) =>
            {
                if (!MailAddress.TryCreate(req.Email, out _))
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["email"] = ["Email no válido."] });
                if (!await UrlSafety.IsPublicHttpUrlAsync(req.WebsiteUrl, ct))
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["websiteUrl"] = ["URL no válida o no pública."] });

                var audit = AuditRequest.Create(req.WebsiteUrl, req.Email, AuditType.Express);
                db.AuditRequests.Add(audit);
                await db.SaveChangesAsync(ct);

                if (!queue.TryWrite(new AuditTask(audit.Id, audit.WebsiteUrl, audit.Type)))
                    return Results.Problem("Cola saturada, inténtalo de nuevo en unos minutos.", statusCode: 503);

                return Results.Accepted($"/api/v1/audits/{audit.Id}", new { auditId = audit.Id, status = audit.Status.ToString() });
            })
            .RequireRateLimiting("express")
            .AllowAnonymous();

            g.MapPost("/full", () => Results.StatusCode(StatusCodes.Status501NotImplemented))
                .RequireAuthorization();   // TODO: validar suscripción activa + TryConsumeAudit

            g.MapGet("/{auditId:guid}", async (Guid auditId, LegalGuardDbContext db, CancellationToken ct) =>
            {
                var a = await db.AuditRequests.AsNoTracking().Include(x => x.Report)
                    .FirstOrDefaultAsync(x => x.Id == auditId, ct);
                return a is null ? Results.NotFound() : Results.Ok(new
                {
                    auditId = a.Id,
                    status = a.Status.ToString(),
                    a.FailureReason,
                    report = a.Report is null ? null : new { a.Report.Score, a.Report.Summary, a.Report.Findings }
                    // TODO: pdfUrl = URL prefirmada generada desde a.Report.PdfStorageKey
                });
            }).AllowAnonymous();   // El Guid v7 no es un secreto fuerte: valorar token de acceso en el email.

            return app;
        }
    }
}
