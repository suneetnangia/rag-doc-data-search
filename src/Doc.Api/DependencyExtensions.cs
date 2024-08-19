
namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Common;
using Microsoft.Extensions.Options;

public static class DependencyExtensions
{
    public static IServiceCollection AddConfig(
             this IServiceCollection services, IConfiguration config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        services.Configure<VectorDbOptions>(
             config.GetSection(VectorDbOptions.VectorDb));

        services.Configure<OllamaOptions>(
                   config.GetSection(OllamaOptions.VectorDb));

        return services;
    }

    public static IServiceCollection AddDependencies(
         this IServiceCollection services)
    {
        /// Singleton service for vector database, we do not want to create an instance per request.
        services.AddSingleton<IVectorDb>(provider =>
        {
            var vector_db =  new QdrantVectorDb(provider.GetRequiredService<ILogger<QdrantVectorDb>>(),
                        provider.GetRequiredService<IOptions<VectorDbOptions>>());

            vector_db.Init().Wait();
            return vector_db;
        });

        /// Singleton service for embeddings language model, we do not want to create an instance per request.        
        services.AddSingleton<LanguageModel<VectorEmbeddings>>(provider =>
        {
            return new EmbeddingsLanguageModel(
                provider.GetRequiredService<ILogger<EmbeddingsLanguageModel>>(),
                provider.GetRequiredService<IOptions<OllamaOptions>>());
        });

        /// Singleton service for response language model, we do not want to create an instance per request.
        services.AddSingleton<LanguageModel<VectorDocument>>(provider =>
        {
            return new ResponseLanguageModel(
                provider.GetRequiredService<ILogger<ResponseLanguageModel>>(),
              provider.GetRequiredService<IOptions<OllamaOptions>>());
        });

        return services;
    }
}
