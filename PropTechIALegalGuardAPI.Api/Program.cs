using System.Threading.Channels;
using System.Threading.RateLimiting;
using PropTechIALegalGuardAPI.Client.Features.Audits;
using PropTechIALegalGuardAPI.Api.Features.Webhooks;
using PropTechIALegalGuardAPI.Application.Audits;
using PropTechIALegalGuardAPI.Infrastructure.Persistence;
using PropTechIALegalGuardAPI.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Stripe;
using PropTechIALegalGuardAPI.Infrastructure.AI;

namespace PropTechIALegalGuardAPI.Api
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var cfg = builder.Configuration;

            // ---------- Persistencia: PostgreSQL + pgvector ----------
            builder.Services.AddDbContext<LegalGuardDbContext>(o =>
                o.UseNpgsql(cfg.GetConnectionString("Postgres")
                            ?? throw new InvalidOperationException("Falta ConnectionStrings:Postgres"),
                            npgsql => npgsql.UseVector()));

            // ---------- Cola interna (Channel) + worker ----------
            builder.Services.AddSingleton(Channel.CreateBounded<AuditTask>(
                new BoundedChannelOptions(capacity: 200) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true }));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<AuditTask>>().Reader);
            builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<AuditTask>>().Writer);
            builder.Services.AddHostedService<AuditProcessingWorker>();

            // ---------- IA: embeddings + Semantic Kernel (see AiServiceCollectionExtensions) ----------
            builder.Services.AddLegalGuardAi(cfg);

            // ---------- Auth (JWT) ----------
            builder.Services.AddAuthentication().AddJwtBearer(o =>
            {
                o.Authority = cfg["Auth:Authority"];
                o.Audience = cfg["Auth:Audience"];
            });
            builder.Services.AddAuthorization();

            // ---------- Rate limiting for the public endpoint ----------
            builder.Services.AddRateLimiter(o =>
            {
                o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                o.AddPolicy("express", ctx => RateLimitPartition.GetFixedWindowLimiter(
                    ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
                    _ => new FixedWindowRateLimiterOptions { PermitLimit = 3, Window = TimeSpan.FromHours(1) }));
            });

            // ---------- Stripe ----------
            StripeConfiguration.ApiKey = cfg["Stripe:SecretKey"];

            builder.Services.AddProblemDetails();
            builder.Services.AddHealthChecks().AddDbContextCheck<LegalGuardDbContext>();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseExceptionHandler();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment()) app.MapOpenApi();

            app.MapHealthChecks("/health");
            app.MapAuditEndpoints();
            app.MapStripeWebhook();

            app.Run();
        }
    }
}