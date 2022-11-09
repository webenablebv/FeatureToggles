using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Webenable.FeatureToggles;

/// <summary>
/// Evaluates feature toggles against <see cref="IConfiguration"/> of the application.
/// </summary>
public class ConfigFeatureToggleConfiguration : FeatureToggleConfiguration
{
    private readonly IConfiguration _configuration;

    public ConfigFeatureToggleConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public override ValueTask<bool?> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        // Resolve the feature section from the configuration.
        var configKey = GetConfigKey(featureName);
        var section = _configuration.GetSection(configKey);
        if (section.Value != null)
        {
            // "Foo": true
            return new ValueTask<bool?>(section.Get<bool>());
        }

        if (section.GetSection("Enabled").Exists())
        {
            // Feature is explictly enabled or disabled.
            // "Foo": {
            //   "Enabled": true,
            //   "Bar": { }
            // }
            return new ValueTask<bool?>(section.GetValue<bool>("Enabled"));
        }

        if (section.GetChildren().Any())
        {
            // Enable the feature if it contains children and is not explicitly enabled or disabled.
            // "Foo": {
            //   "Bar": { }
            // }
            return new ValueTask<bool?>(true);
        }

        // The feature is not toggled in the configuration.
        return new ValueTask<bool?>((bool?)null);
    }

    protected virtual string GetConfigKey(string featureName) => "Features:" + featureName.Replace('.', ':');
}
