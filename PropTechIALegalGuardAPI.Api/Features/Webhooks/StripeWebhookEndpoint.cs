using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace PropTechIALegalGuardAPI.Api.Features.Webhooks;

public static class StripeWebhookEndpoint
{
    public static IEndpointRouteBuilder MapStripeWebhook(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/webhooks/stripe", async (HttpRequest http, IConfiguration cfg, ILogger<Program> log) =>
        {
            var json = await new StreamReader(http.Body).ReadToEndAsync();
            Event ev;
            try
            {
                ev = EventUtility.ConstructEvent(json, http.Headers["Stripe-Signature"], cfg["Stripe:WebhookSecret"]);
            }
            catch (StripeException) { return Results.BadRequest(); }

            switch (ev.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                case EventTypes.CustomerSubscriptionUpdated:
                case EventTypes.CustomerSubscriptionDeleted:
                    // TODO: handler idempotente (guardar ev.Id) que actualice UserSubscription
                    log.LogInformation("Stripe {Type} {Id}", ev.Type, ev.Id);
                    break;
            }
            return Results.Ok();
        }).AllowAnonymous();
        return app;
    }
}
