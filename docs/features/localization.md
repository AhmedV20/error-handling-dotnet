# Error Message Localization

ErrorLens supports localizing error messages via the `IErrorMessageLocalizer` abstraction. This is opt-in — by default, messages pass through unchanged.

## Abstraction

The `IErrorMessageLocalizer` interface provides two methods:

```csharp
public interface IErrorMessageLocalizer
{
    string Localize(string errorCode, string defaultMessage);
    string LocalizeFieldError(string errorCode, string fieldName, string defaultMessage);
}
```

Error codes are used as resource keys for message lookup. When a resource is not found, the default message is returned as-is.

## Default Behavior

`NoOpErrorMessageLocalizer` is registered via `TryAddSingleton` and passes all messages through without modification. No setup is required for the default behavior.

## Enabling Localization

Call `AddErrorHandlingLocalization<TResource>()` to replace the no-op implementation with `StringLocalizerErrorMessageLocalizer<TResource>`, which bridges to `IStringLocalizer<T>`:

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddErrorHandling();
builder.Services.AddErrorHandlingLocalization<SharedErrorMessages>();
```

**Dependency**: `Microsoft.Extensions.Localization.Abstractions 8.0.0` (interfaces only).

## Resource Files

Create `.resx` files with error codes as keys:

**Resources/SharedErrorMessages.resx** (default / English):
```
USER_NOT_FOUND       = User not found.
VALIDATION_FAILED    = One or more validation errors occurred.
```

**Resources/SharedErrorMessages.fr.resx** (French):
```
USER_NOT_FOUND       = Utilisateur introuvable.
VALIDATION_FAILED    = Une ou plusieurs erreurs de validation se sont produites.
```

## Request Localization

Configure ASP.NET Core's `RequestLocalizationMiddleware` so that `CultureInfo.CurrentUICulture` is set from the incoming request:

```csharp
var supportedCultures = new[] { "en", "fr", "de", "es" };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("en");
    options.AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();
app.UseRequestLocalization();
```

The localizer uses `CultureInfo.CurrentUICulture` to resolve the correct resource file.

## What Gets Localized

Localization applies to all parts of the error response:

- Top-level `message`
- `fieldErrors[].message`
- `globalErrors[].message`
- `parameterErrors[].message`
