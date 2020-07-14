using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Dommel;
using Microsoft.Extensions.Options;

namespace Webenable.FeatureToggles
{
    /// <summary>
    /// Evaluates feature toggles against <see cref="FeatureToggleDto"/> records the database.
    /// </summary>
    public class DatabaseFeatureToggleConfiguration : FeatureToggleConfiguration
    {
        private readonly FeatureToggleOptions _options;

        public DatabaseFeatureToggleConfiguration(IOptions<FeatureToggleOptions> options)
        {
            _options = options.Value;
        }

        public override async ValueTask<bool?> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
        {
            var factory = _options.DbConnectionFactory;
            if (factory is object)
            {
                using var con = await factory(cancellationToken);
                var toggle = await con.FirstOrDefaultAsync<FeatureToggleDto>(x => x.Name == featureName);
                if (toggle is object)
                {
                    return toggle.State == FeatureToggleState.Enabled;
                }
            }

            // No database connection configured or feature toggle is not configured
            return null;
        }

        public override int Order => base.Order - 10;
    }

    /// <summary>
    /// A feature toggle record.
    /// </summary>
    [Table("FeatureToggles")]
    public class FeatureToggleDto
    {
        /// <summary>
        /// Gets or sets the ID of the feature toggle record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the feature.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the state of the feature toggle.
        /// </summary>
        public FeatureToggleState State { get; set; }
    }

    /// <summary>
    /// Specifies the state of the feature toggle.
    /// </summary>
    public enum FeatureToggleState
    {
        Disabled = 0,
        Enabled = 1,
    }
}
