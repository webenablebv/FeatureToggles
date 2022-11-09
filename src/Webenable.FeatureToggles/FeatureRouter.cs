using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Webenable.FeatureToggles;

/// <summary>
/// Evaluates whether features are enabled.
/// </summary>
public interface IFeatureRouter
{
    /// <summary>
    /// Evaluates whether a feature toggle with the specified <paramref name="featureName"/> is enabled.
    /// </summary>
    /// <param name="featureName">
    /// The name of the feature. Nested feature toggles can be specified
    /// using dots (e.g. <c>Webshop.ShoppingCart.ShareShoppingCart</c>.)</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to propagate cancellation.</param>
    /// <returns><c>true</c> when the feature is enabled; otherwise, <c>false</c>.</returns>
    ValueTask<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default <see cref="IFeatureRouter"/> implementation which evaluates <see cref="IFeatureToggleConfiguration"/>'s.
///
/// <para>
/// All the <see cref="IFeatureToggleConfiguration"/> implementations are fetched and evaluated in their specified
/// <see cref="IFeatureToggleConfiguration.Order"/>. If a <see cref="IFeatureToggleConfiguration"/> returns either
/// <c>true</c> or <c>false</c> the evaluation is stopped and the value is returned. When a <see cref="IFeatureToggleConfiguration"/>
/// implementation does not returns a value, the next configuration is evaluated. In case no configuration configures
/// a certain feature toggle specified, <c>false</c> is returned.
/// </para>
/// </summary>
public class DefaultFeatureRouter : IFeatureRouter
{
    private readonly IEnumerable<IFeatureToggleConfiguration> _featureToggleConfiguration;

    public DefaultFeatureRouter(IEnumerable<IFeatureToggleConfiguration> featureToggleConfiguration)
    {
        _featureToggleConfiguration = featureToggleConfiguration;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(featureName))
        {
            throw new ArgumentNullException(nameof(featureName));
        }

        // The feature toggle providers are invoked in the specified order
        foreach (var featureToggleConfiguration in _featureToggleConfiguration.OrderBy(f => f.Order))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // If the provider returned a value use it, otherwise keep recursing
            var enabled = await featureToggleConfiguration.IsEnabledAsync(featureName, cancellationToken);
            if (enabled.HasValue)
            {
                return enabled.Value;
            }
        }

        // No provider configured this feature
        return false;
    }
}
