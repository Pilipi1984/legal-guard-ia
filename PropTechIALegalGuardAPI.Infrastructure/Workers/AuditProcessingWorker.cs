using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PropTechIALegalGuardAPI.Application.Audits;
using PropTechIALegalGuardAPI.Application.Audits.Abstractions;
using PropTechIALegalGuardAPI.Application.Audits.Interfaces;
using PropTechIALegalGuardAPI.Domain.Enums;
using PropTechIALegalGuardAPI.Infrastructure.Persistence;
using System.Threading.Channels;

namespace PropTechIALegalGuardAPI.Infrastructure.Workers
{

    public sealed class AuditProcessingWorker(
        ChannelReader<AuditTask> queue,
        IServiceScopeFactory scopes,
        ILogger<AuditProcessingWorker> log) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stop)
        {
            await RequeuePendingAsync(stop);   //  Resume tasks if the process has restarted.

            await foreach (var task in queue.ReadAllAsync(stop))
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LegalGuardDbContext>();
                var audit = await db.AuditRequests.FirstOrDefaultAsync(a => a.Id == task.AuditId, stop);
                if (audit is null) continue;

                try
                {
                    audit.Advance(AuditStatus.Scraping); await db.SaveChangesAsync(stop);
                    var scraper = scope.ServiceProvider.GetRequiredService<IPageScraper>();
                    var page = await scraper.ScrapeAsync(audit.WebsiteUrl, stop);   // Fase 1

                    audit.Advance(AuditStatus.Analyzing); await db.SaveChangesAsync(stop);
                    // TODO Phase 2: IComplianceAgent (Semantic Kernel + plugins + RAG pgvector) -> IEnumerable<Finding>

                    audit.Advance(AuditStatus.Rendering); await db.SaveChangesAsync(stop);
                    // TODO Phase 3: IReportRenderer (QuestPDF) -> bytes -> IBlobStorage

                    // TODO Phase 4: INotifier (email) and AttachReport(...)
                    audit.Advance(AuditStatus.Completed);
                }
                catch (ScrapeFailedException ex)
                {
                    log.LogWarning("Auditoría {AuditId}: {Reason}", task.AuditId, ex.Message);
                    audit.Fail(ex.Message);   // TODO: INotifier.SendAuditFailedAsync
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    log.LogError(ex, "Auditoría {AuditId} fallida", task.AuditId);
                    audit.Fail(ex.Message);
                }
                await db.SaveChangesAsync(stop);
            }
        }

        private async Task RequeuePendingAsync(CancellationToken ct) =>
            // TODO: Read AuditRequests in state Queued and requeue them (requires ChannelWriter) or migrate to Hangfire.
            await Task.CompletedTask;
    }
}