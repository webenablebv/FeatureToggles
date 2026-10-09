using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Webenable.FeatureToggles;

/// <summary>
/// Evaluates feature toggles against <see cref="IConfiguration"/> of the application.
/// </summary>
public class ConfigFeatureToggleConfiguration(IConfiguration configuration, FeatureToggleUserContext? userContext = null) : FeatureToggleConfiguration
{
    public override ValueTask<Dictionary<string, bool>> GetAll(CancellationToken cancellationToken = default)
    {
        var featureConfig = configuration.GetSection("Features");
        if (featureConfig == null)
        {
            return new ValueTask<Dictionary<string, bool>>([]);
        }

        var toggles = featureConfig.GetChildren().ToDictionary(x => x.Key, x => Evaluate(x, null) ?? false);
        return new ValueTask<Dictionary<string, bool>>(toggles);
    }

    public override ValueTask<bool?> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Resolve the feature section from the configuration.
        var configKey = GetConfigKey(featureName);
        var section = configuration.GetSection(configKey);
        return new ValueTask<bool?>(Evaluate(section, userContext?.EmailAddress));
    }

    private static bool? Evaluate(IConfigurationSection section, string? emailAddress)
    {
        if (section.Value != null)
        {
            // "Foo": true
            return section.Get<bool>();
        }

        var allowedEmails = section.GetSection("AllowedEmails");
        var emailAllowed = FeatureToggleEmailMatcher.Contains(allowedEmails.GetChildren().Select(x => x.Value), emailAddress);

        if (section.GetSection("Enabled").Exists())
        {
            // Feature is explicitly enabled or disabled.
            // "Foo": {
            //   "Enabled": true,
            //   "Bar": { }
            // }
            return section.GetValue<bool>("Enabled") || emailAllowed;
        }

        // An email list alone must not implicitly enable a feature for everyone.
        if (allowedEmails.Exists())
        {
            return emailAllowed;
        }

        if (section.GetChildren().Any())
        {
            // Enable the feature if it contains children and is not explicitly enabled or disabled.
            // "Foo": {
            //   "Bar": { }
            // }
            return true;
        }

        // The feature is not toggled in the configuration.
        return null;
    }

    protected virtual string GetConfigKey(string featureName) => "Features:" + featureName.Replace('.', ':');
}
