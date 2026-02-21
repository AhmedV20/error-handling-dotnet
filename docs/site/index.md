---
layout: home

hero:
  name: ErrorLens.ErrorHandling
  text: Production-Ready Error Handling for ASP.NET Core
  tagline: Transform unhandled exceptions into clean, structured JSON responses with minimal setup — just two lines of code to get started.
  image:
    src: /icon.png
    alt: ErrorLens
  actions:
    - theme: brand
      text: Get Started
      link: /documentation
    - theme: alt
      text: View on GitHub
      link: https://github.com/AhmedV20/error-handling-dotnet

features:
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M13 2 3 14h9l-1 8 10-12h-9l1-8z"/></svg>
    title: Zero-Config Setup
    details: Add two lines of code and every unhandled exception returns a structured JSON response — no configuration required.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="16 18 22 12 16 6"/><polyline points="8 6 2 12 8 18"/></svg>
    title: Smart Error Codes
    details: Automatic ALL_CAPS code generation from exception class names with full customization support.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="21" x2="14" y1="4" y2="4"/><line x1="10" x2="3" y1="4" y2="4"/><line x1="21" x2="12" y1="12" y2="12"/><line x1="8" x2="3" y1="12" y2="12"/><line x1="21" x2="16" y1="20" y2="20"/><line x1="12" x2="3" y1="20" y2="20"/><line x1="14" x2="14" y1="2" y2="6"/><line x1="8" x2="8" y1="10" y2="14"/><line x1="16" x2="16" y1="18" y2="22"/></svg>
    title: Highly Configurable
    details: JSON or YAML configuration for HTTP statuses, error codes, messages, and custom JSON field names.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="8" height="4" x="8" y="2" rx="1" ry="1"/><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><path d="m9 14 2 2 4-4"/></svg>
    title: Validation Support
    details: Rich field-level, parameter-level, and global validation error responses with DataAnnotations integration.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z"/><path d="m9 12 2 2 4-4"/></svg>
    title: Security Built-In
    details: Automatic 5xx message sanitization prevents information disclosure. Startup validation catches misconfigurations early.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z"/><path d="M14 2v4a2 2 0 0 0 2 2h4"/><path d="M10 9H8"/><path d="M16 13H8"/><path d="M16 17H8"/></svg>
    title: RFC 9457 Problem Details
    details: Opt-in support for standardized application/problem+json format with automatic type URI generation.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="3" rx="2"/><path d="M7 7h3v3H7z"/><path d="M14 7h3v3h-3z"/><path d="M7 14h3v3H7z"/><path d="M14 14h3v3h-3z"/></svg>
    title: Custom Handlers
    details: Register IApiExceptionHandler implementations with priority ordering. Replace the built-in fallback handler for full control.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m12 3-1.9 12.3a1 1 0 0 0 1.1 1.2H18l-1.3-4.5"/><path d="M6.3 12.3 8.2 4a1 1 0 0 1 1-0.8H15"/><path d="m9 21 3-9"/><path d="M15 21 12 12"/></svg>
    title: Response Customization
    details: Add global properties like traceId, timestamp, or user context to all error responses via IApiErrorResponseCustomizer.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 12h-2.48a2 2 0 0 0-1.93 1.46l-2.35 8.36a.25.25 0 0 1-.48 0L9.24 2.18a.25.25 0 0 0-.48 0l-2.35 8.36A2 2 0 0 1 4.49 12H2"/></svg>
    title: OpenTelemetry Tracing
    details: Automatic Activity spans with error tags and OTel semantic conventions — zero new dependencies required.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 2a14.5 14.5 0 0 0 0 20 14.5 14.5 0 0 0 0-20"/><path d="M2 12h20"/></svg>
    title: Localization
    details: Multi-language error messages via IStringLocalizer bridge. Error codes are used as resource keys in .resx files.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 3h6a4 4 0 0 1 4 4v14a3 3 0 0 0-3-3H2z"/><path d="M22 3h-6a4 4 0 0 0-4 4v14a3 3 0 0 1 3-3h7z"/></svg>
    title: OpenAPI & Swagger
    details: Auto-generated error response schemas for .NET 9+ OpenAPI and .NET 6-8 Swashbuckle integration.
  - icon: <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m12 14 4-4"/><path d="M3.34 19a10 10 0 1 1 17.32 0"/></svg>
    title: Rate Limiting
    details: Structured 429 responses with Retry-After headers via IRateLimitResponseWriter (.NET 7+).
---

<div class="banner-section">
  <div class="banner-wrapper">
    <img src="/banner.png" alt="ErrorLens.ErrorHandling — Structured error handling for ASP.NET Core" />
  </div>
</div>

## Quick Start

```bash
dotnet add package ErrorLens.ErrorHandling
```

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddErrorHandling();

var app = builder.Build();
app.UseErrorHandling();
app.Run();
```

Any unhandled exception automatically becomes a structured JSON response:

```json
{
  "code": "USER_NOT_FOUND",
  "message": "Could not find user with id 123"
}
```

## Packages

| Package | Target | NuGet |
|---------|--------|-------|
| `ErrorLens.ErrorHandling` | .NET 6 - 10 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling)](https://www.nuget.org/packages/ErrorLens.ErrorHandling) |
| `ErrorLens.ErrorHandling.OpenApi` | .NET 9+ | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.OpenApi)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.OpenApi) |
| `ErrorLens.ErrorHandling.Swashbuckle` | .NET 6 - 8 | [![NuGet](https://img.shields.io/nuget/v/ErrorLens.ErrorHandling.Swashbuckle)](https://www.nuget.org/packages/ErrorLens.ErrorHandling.Swashbuckle) |
