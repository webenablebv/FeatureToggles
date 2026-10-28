# FeatureToggles
Simple feature toggles implementation for ASP.NET Core.

## Caching

Feature evaluations and `GetAll` results are cached in memory for two minutes by default. Configure a different duration when registering feature toggles:

```csharp
services.AddFeatureToggles(options =>
{
    options.CacheDuration = TimeSpan.FromSeconds(30);
});
```

The duration must be greater than zero. User-specific evaluations use the normalized email address from `FeatureToggleUserContext` as part of the cache key.

## Beta groups by email

Keep a feature globally disabled and allow specific email addresses:

```json
{
  "Features": {
    "NewCheckout": {
      "Enabled": false,
      "AllowedEmails": ["alice@example.com", "bob@example.com"]
    }
  }
}
```

After authentication, populate the scoped `FeatureToggleUserContext` (registered
by `AddFeatureToggles`) with the authenticated user's email. Existing feature
checks automatically use that context:

```csharp
// Inject FeatureToggleUserContext userContext and IFeatureRouter featureRouter.
userContext.EmailAddress = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
var enabled = await featureRouter.IsEnabledAsync("NewCheckout");

// Clear the context when the user signs out.
userContext.EmailAddress = null;
```

Use the email claim supplied by your authentication provider; do not take the
email from an untrusted request parameter. Matching uses the entire address,
ignores casing and trims surrounding whitespace. Missing or blank emails do
not qualify for beta access. `Enabled: true` enables the feature for everyone.
An `AllowedEmails` list without `Enabled` also enables only the listed users.
Nested features use dotted names such as `Shop.NewCheckout`.

For ASP.NET Core requests, set the context after authentication on every request
(for example in middleware between authentication and endpoints), including null
for anonymous users. For a longer-lived scope such as a Blazor Server circuit,
update it when authentication changes. The context and router must be resolved
from the same scope. User context is never stored in singleton options.

`IsEnabledAsync` checks availability for the current user. With no email set,
it checks global availability. `GetAll` still reports global availability, so a
beta-only toggle stays false in that overview. Provider priority still applies: the first
provider with a configured value decides, including a false result for a user
outside the beta group. Custom providers retain their existing behavior and can
inject `FeatureToggleUserContext` to implement their own user-specific checks.

### Database toggles

Set `FeatureToggles.State` to `2` (`FeatureToggleState.Beta`) and store allowed
addresses in a separate `FeatureToggleEmails` table. State `0` remains disabled
for everyone; state `1` remains enabled for everyone.

Create this table in your application's migration before using database beta
groups. For example, on SQL Server:

```sql
CREATE TABLE FeatureToggleEmails (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FeatureToggleId int NOT NULL REFERENCES FeatureToggles(Id),
    EmailAddress nvarchar(320) NOT NULL
);
CREATE INDEX IX_FeatureToggleEmails_FeatureToggleId
    ON FeatureToggleEmails(FeatureToggleId);

-- Use the ID of your existing toggle here.
UPDATE FeatureToggles SET State = 2 WHERE Id = 42;
INSERT INTO FeatureToggleEmails (FeatureToggleId, EmailAddress)
VALUES (42, 'alice@example.com'), (42, 'bob@example.com');
```

The email table is queried only for a beta toggle evaluated with a nonblank
email. Existing database schemas continue to work for global toggle checks
and toggles in states `0` and `1`.

Run the tests with `dotnet test featuretoggles.sln`.
