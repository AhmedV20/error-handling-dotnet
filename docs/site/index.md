---
layout: home

hero:
  name: ErrorLens.ErrorHandling
  text: Structured Error Responses for ASP.NET Core
  tagline: Consistent, opinionated error handling for REST APIs — from .NET 6 to .NET 10
  image:
    src: /icon.png
    alt: ErrorLens
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: View on GitHub
      link: https://github.com/AhmedV20/error-handling-dotnet

features:
  - icon: ⚡
    title: Zero-Config Setup
    details: Add two lines of code and get structured error responses for all exceptions immediately.
  - icon: 🎯
    title: Smart Error Codes
    details: Automatic ALL_CAPS code generation from exception class names with full customization support.
  - icon: 🔧
    title: Highly Configurable
    details: JSON or YAML configuration for HTTP statuses, error codes, messages, and field names.
  - icon: 📋
    title: Validation Support
    details: Rich field-level, parameter-level, and global validation error responses with DataAnnotations.
  - icon: 🛡️
    title: Security Built-In
    details: Automatic 5xx message sanitization prevents information disclosure. Startup validation catches misconfigurations.
  - icon: 📊
    title: RFC 9457 Problem Details
    details: Opt-in support for standardized Problem Details format with application/problem+json content type.
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
