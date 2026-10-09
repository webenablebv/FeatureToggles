using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dommel;
using Microsoft.Extensions.Options;

namespace Webenable.FeatureToggles;

/// <summary>
/// Evaluates feature toggles against <see cref="FeatureToggleDto"/> records the database.
/// </summary>
public class DatabaseFeatureToggleConfiguration(IOptions<FeatureToggleOptions> options, FeatureToggleUserContext? userContext = null) : FeatureToggleConfiguration
{
    public override async ValueTask<bool?> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var emailAddress = userContext?.EmailAddress;
        var factory = options.Value.DbConnectionFactory;
        if (factory is not null)
        {
            await using var con = await factory(cancellationToken);
            var toggle = await con.FirstOrDefaultAsync<FeatureToggleDto>(x => x.Name == featureName, cancellationToken: cancellationToken);
            if (toggle is not null)
            {
                if (toggle.State == FeatureToggleState.Beta && !string.IsNullOrWhiteSpace(emailAddress))
                {
                    var emails = await con.SelectAsync<FeatureToggleEmailDto>(x => x.FeatureToggleId == toggle.Id, cancellationToken: cancellationToken);
                    return FeatureToggleEmailMatcher.Contains(emails.Select(x => x.EmailAddress), emailAddress);
                }
                return toggle.IsEnabled;
            }
        }

        // No database connection configured or feature toggle is not configured
        return null;
    }

    public override async ValueTask<Dictionary<string, bool>> GetAll(CancellationToken cancellationToken = default)
    {
        var factory = options.Value.DbConnectionFactory;
        if (factory is not null)
        {
            await using var con = await factory(cancellationToken);
            var toggles = await con.GetAllAsync<FeatureToggleDto>(cancellationToken: cancellationToken);
            return toggles.ToDictionary(x => x.Name!, x => x.IsEnabled);
        }

        return [];
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

    [NotMapped]
    public bool IsEnabled => State == FeatureToggleState.Enabled;
}

/// <summary>
/// Specifies the state of the feature toggle.
/// </summary>
public enum FeatureToggleState
{
    Disabled = 0,
    Enabled = 1,
    /// <summary>Enabled only for addresses in the FeatureToggleEmails table.</summary>
    Beta = 2,
}

/// <summary>An email address allowed to use a beta feature.</summary>
[Table("FeatureToggleEmails")]
public class FeatureToggleEmailDto
{
    public int Id { get; set; }
    public int FeatureToggleId { get; set; }
    public string? EmailAddress { get; set; }
}
