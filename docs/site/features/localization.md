# Error Message Localization

ErrorLens supports localizing error messages via the `IErrorMessageLocalizer` abstraction. This is opt-in — by default, messages pass through unchanged.

## Default Behavior

`NoOpErrorMessageLocalizer` is registered via `TryAddSingleton` and passes all messages through without modification. No setup required.

## Enabling Localization

Call `AddErrorHandlingLocalization<TResource>()` to replace the no-op implementation:

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddErrorHandling();
builder.Services.AddErrorHandlingLocalization<SharedErrorMessages>();
```

This bridges to `IStringLocalizer<T>` and uses error codes as resource keys.

## Resource Files

Create `.resx` files with error codes as keys:

**Resources/SharedErrorMessages.resx** (default / English):

| Key | Value |
|-----|-------|
| `USER_NOT_FOUND` | User not found. |
| `VALIDATION_FAILED` | One or more validation errors occurred. |

**Resources/SharedErrorMessages.fr.resx** (French):

| Key | Value |
|-----|-------|
| `USER_NOT_FOUND` | Utilisateur introuvable. |
| `VALIDATION_FAILED` | Une ou plusieurs erreurs de validation se sont produites. |

## Request Localization

Configure ASP.NET Core's `RequestLocalizationMiddleware`:

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
