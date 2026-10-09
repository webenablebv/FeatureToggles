namespace Webenable.FeatureToggles;

/// <summary>
/// The current authenticated user for feature evaluation. Registered as a scoped service.
/// Populate this context after authentication, and clear it when the user signs out.
/// </summary>
public sealed class FeatureToggleUserContext
{
    /// <summary>The authenticated user's email address, or null for an anonymous user.</summary>
    public string? EmailAddress { get; set; }
}
