
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
        services.AddScoped<IVectorDb>(provider =>
        {            
            var dd = provider.GetRequiredService<IOptions<VectorDbOptions>>();

            return new VectorDb(provider.GetRequiredService<ILogger<VectorDb>>(),
                        provider.GetRequiredService<IOptions<VectorDbOptions>>());
        });

        services.AddScoped<LanguageModel<VectorEmbeddings>>(provider =>
        {
            return new EmbeddingsLanguageModel(
                provider.GetRequiredService<ILogger<EmbeddingsLanguageModel>>(),
                provider.GetRequiredService<IOptions<OllamaOptions>>());
        });

        services.AddScoped<LanguageModel<string>>(provider =>
        {
            return new ResponseLanguageModel(
                provider.GetRequiredService<ILogger<ResponseLanguageModel>>(),
              provider.GetRequiredService<IOptions<OllamaOptions>>());
        });

        return services;
    }
}
