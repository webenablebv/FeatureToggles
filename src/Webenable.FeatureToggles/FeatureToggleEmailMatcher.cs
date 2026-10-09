using System;
using System.Collections.Generic;
using System.Linq;

namespace Webenable.FeatureToggles;

internal static class FeatureToggleEmailMatcher
{
    public static bool Contains(IEnumerable<string?> allowedEmails, string? emailAddress)
        => !string.IsNullOrWhiteSpace(emailAddress)
            && allowedEmails.Any(email => !string.IsNullOrWhiteSpace(email)
                && string.Equals(email.Trim(), emailAddress.Trim(), StringComparison.OrdinalIgnoreCase));
}
