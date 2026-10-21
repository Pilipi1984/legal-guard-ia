using System.Threading.Channels;
using PropTechIALegalGuardAPI.Application.Audits;
using PropTechIALegalGuardAPI.Domain.Enums;
using PropTechIALegalGuardAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PropTechIALegalGuardAPI.Infrastructure.Workers;

public sealed class AuditProcessingWorker(
    ChannelReader<AuditTask> queue,
    IServiceScopeFactory scopes,
    ILogger<AuditProcessingWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        await RequeuePendingAsync(stop);   // recupera tareas si el proceso se reinició

        await foreach (var task in queue.ReadAllAsync(stop))
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LegalGuardDbContext>();
            var audit = await db.AuditRequests.FirstOrDefaultAsync(a => a.Id == task.AuditId, stop);
            if (audit is null) continue;

            try
            {
                audit.Advance(AuditStatus.Scraping);   await db.SaveChangesAsync(stop);
                // TODO Fase 1: IPageScraper (Playwright) -> Compressed DOM, JS console, metadata

                audit.Advance(AuditStatus.Analyzing);  await db.SaveChangesAsync(stop);
                // TODO Fase 2: IComplianceAgent (Semantic Kernel + plugins + RAG pgvector) -> IEnumerable<Finding>

                audit.Advance(AuditStatus.Rendering);  await db.SaveChangesAsync(stop);
                // TODO Fase 3: IReportRenderer (QuestPDF) -> bytes -> IBlobStorage

                // TODO Fase 4: INotifier (email) y AttachReport(...)
                audit.Advance(AuditStatus.Completed);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Auditoría {AuditId} fallida", task.AuditId);
                audit.Fail(ex.Message);
            }
            await db.SaveChangesAsync(stop);
        }
    }

    private async Task RequeuePendingAsync(CancellationToken ct)
    {
        // TODO: leer AuditRequests en estado Queued y reinyectarlas (necesita ChannelWriter) o migrar a Hangfire.
        await Task.CompletedTask;
    }
}
