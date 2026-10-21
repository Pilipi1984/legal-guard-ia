using PropTechIALegalGuardAPI.Infrastructure.AI.Plugins;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using OpenAI;

namespace PropTechIALegalGuardAPI.Infrastructure.AI
{
    public static class AiServiceCollectionExtensions
    {
        public static IServiceCollection AddLegalGuardAi(this IServiceCollection services, IConfiguration cfg)
        {
            var apiKey = cfg["AI:ApiKey"] ?? throw new InvalidOperationException("Falta AI:ApiKey");
            var chatModel = cfg["AI:ChatModel"] ?? "gpt-4o-mini";
            var embeddingModel = cfg["AI:EmbeddingModel"] ?? "text-embedding-3-small"; // 1536 dimensions

            // Embeddings: Stable abstraction from Microsoft.Extensions.AI over the official OpenAI SDK.
            // Does not depend on Semantic Kernel's experimental extensions (SKEXP).
            services.AddSingleton(new OpenAIClient(apiKey));
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
                sp.GetRequiredService<OpenAIClient>().GetEmbeddingClient(embeddingModel).AsIEmbeddingGenerator());

            // Semantic Kernel: agent orchestration only (chat + native plugins).
            var kernel = services.AddKernel();
            kernel.AddOpenAIChatCompletion(chatModel, apiKey);
            kernel.Plugins.AddFromType<HeadingValidatorPlugin>();
            kernel.Plugins.AddFromType<LegalTextScannerPlugin>();

            return services;
        }
    }
}
