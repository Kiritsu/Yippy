using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Yippy.Caching;

public static class ServicesExtensions
{
    public static IServiceCollection AddYippyCaching(this IServiceCollection @this, IConfiguration configuration)
    {
        @this.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 50000;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(5),
                Expiration = TimeSpan.FromMinutes(30)
            };
        });

        @this.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Caching:Redis"];
        });

        return @this;
    }
}