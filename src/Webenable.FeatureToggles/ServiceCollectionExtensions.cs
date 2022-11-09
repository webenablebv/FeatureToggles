using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Webenable.FeatureToggles;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds feature toggle support.
    /// </summary>
    public static IServiceCollection AddFeatureToggles(this IServiceCollection services) => AddFeatureToggles(services, configureOptions: null);

    /// <summary>
    /// Adds feature toggle support.
    /// </summary>
    public static IServiceCollection AddFeatureToggles(this IServiceCollection services, Action<FeatureToggleOptions>? configureOptions)
    {
        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.AddFeatureToggleConfiguration<DatabaseFeatureToggleConfiguration>()
            .AddFeatureToggleConfiguration<ConfigFeatureToggleConfiguration>();

        services.TryAddScoped<IFeatureRouter, DefaultFeatureRouter>();
        return services;
    }

    /// <summary>
    /// Adds a <see cref="IFeatureToggleConfiguration"/> implementation.
    /// </summary>
    public static IServiceCollection AddFeatureToggleConfiguration<T>(this IServiceCollection services) where T : class, IFeatureToggleConfiguration
        => services.AddScoped<IFeatureToggleConfiguration, T>();

    /// <summary>
    /// Adds a service to configure <see cref="FeatureToggleOptions"/> dynamically.
    /// </summary>
    public static IServiceCollection AddFeatureToggleOptions<T>(this IServiceCollection services) where T : class, IConfigureOptions<FeatureToggleOptions>
        => services.AddSingleton<IConfigureOptions<FeatureToggleOptions>, T>();
}
