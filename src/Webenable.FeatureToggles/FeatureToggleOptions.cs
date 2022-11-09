using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Webenable.FeatureToggles;

/// <summary>
/// Defines options for feature toggles.
/// </summary>
public class FeatureToggleOptions
{
    /// <summary>
    /// Gets or sets a factory to create a <see cref="IDbConnection"/> to evaluate feature toggles using <see cref="DatabaseFeatureToggleConfiguration"/>.
    /// Leave null to disable feature toggles in the database.
    /// </summary>
    public Func<CancellationToken, Task<IDbConnection>>? DbConnectionFactory { get; set; }
}
