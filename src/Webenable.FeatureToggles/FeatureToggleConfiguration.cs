using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Webenable.FeatureToggles;

/// <summary>
/// Provides a configuration implementation for feature toggles.
/// </summary>
public interface IFeatureToggleConfiguration
{
    /// <summary>
    /// Evaluates the feature toggle against the configuration to determine whether it's enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to propagate cancellation.</param>
    /// <returns>
    /// <c>true</c> when the feature is enabled, <c>false</c> when the feature is disabled
    /// or <c>null</c> when the feature toggle is not configured in this implementation.
    /// </returns>
    ValueTask<bool?> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all feature toggles in the configuration.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to propagate cancellation.</param>
    /// <returns>A dictionary with configured feature toggles and whether they're enabled or not.</returns>
    ValueTask<Dictionary<string, bool>> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// The order which is used when evaluating feature toggles using a <see cref="IFeatureRouter"/>.
    /// </summary>
    int Order { get; }
}

/// <summary>
/// Provides a configuration implementation for feature toggles.
/// </summary>
public abstract class FeatureToggleConfiguration : IFeatureToggleConfiguration
{
    /// <inheritdoc/>
    public abstract ValueTask<bool?> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract ValueTask<Dictionary<string, bool>> GetAll(CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual int Order { get; } = 1000;
}
