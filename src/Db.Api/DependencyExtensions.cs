
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

        services.Configure<InfluxDbOptions>(
                    config.GetSection(InfluxDbOptions.InfluxDb));

        services.AddHttpClient();

        return services;
    }

    public static IServiceCollection AddDependencies(
         this IServiceCollection services)
    {
        /// Singleton service for vector database, we do not want to create an instance per request.
        services.AddSingleton<IVectorDb>(provider =>
        {
            var vector_db = new QdrantVectorDb(provider.GetRequiredService<ILogger<QdrantVectorDb>>(),
                        provider.GetRequiredService<IOptions<VectorDbOptions>>());

            // TODO: Cancellation token should be passed here.
            vector_db.Init().Wait();
            return vector_db;
        });

        services.AddSingleton<InfluxDbRepository>(provider =>
        {
            var influx_db = new InfluxDbRepository(provider.GetRequiredService<ILogger<InfluxDbRepository>>(),
                        provider.GetRequiredService<IOptions<InfluxDbOptions>>());

            return influx_db;
        });

        /// Singleton service for embeddings language model, we do not want to create an instance per request.        
        services.AddSingleton<LanguageModel<VectorEmbeddings>>(provider =>
        {
            var http_client = provider.GetRequiredService<HttpClient>();
            var ollama_options = provider.GetRequiredService<IOptions<OllamaOptions>>();

            http_client.Timeout = TimeSpan.FromSeconds(ollama_options.Value.HttpTimeoutInSeconds);
            http_client.BaseAddress = new Uri(ollama_options.Value.OllamaApiBaseUrl);

            return new EmbeddingsLanguageModel(
                provider.GetRequiredService<ILogger<EmbeddingsLanguageModel>>(),
                http_client,
                provider.GetRequiredService<IOptions<OllamaOptions>>());
        });

        /// Singleton service for response language model, we do not want to create an instance per request.
        services.AddSingleton<LanguageModel<LanguageResponse>>(provider =>
        {
            var http_client = provider.GetRequiredService<HttpClient>();
            var ollama_options = provider.GetRequiredService<IOptions<OllamaOptions>>();

            http_client.Timeout = TimeSpan.FromSeconds(ollama_options.Value.HttpTimeoutInSeconds);
            http_client.BaseAddress = new Uri(ollama_options.Value.OllamaApiBaseUrl);

            return new ResponseLanguageModel(
                provider.GetRequiredService<ILogger<ResponseLanguageModel>>(),
                http_client,
                provider.GetRequiredService<IOptions<OllamaOptions>>());
        });

        return services;
    }
}
