import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'ErrorLens.ErrorHandling',
  description: 'Consistent, structured error responses for ASP.NET Core REST APIs',
  base: '/error-handling-dotnet/current/',

  head: [
    ['link', { rel: 'icon', type: 'image/png', href: '/error-handling-dotnet/current/icon.png' }]
  ],

  themeConfig: {
    logo: '/error-handling-dotnet/current/icon.png',

    nav: [
      { text: 'Guide', link: '/guide/getting-started' },
      { text: 'Features', link: '/features/attributes' },
      { text: 'Reference', link: '/reference/api' },
      {
        text: 'v1.3.0',
        items: [
          { text: 'Changelog', link: '/changelog' },
          { text: 'NuGet', link: 'https://www.nuget.org/packages/ErrorLens.ErrorHandling' }
        ]
      }
    ],

    sidebar: {
      '/guide/': [
        {
          text: 'Introduction',
          items: [
            { text: 'Getting Started', link: '/guide/getting-started' },
            { text: 'Configuration', link: '/guide/configuration' },
            { text: 'Logging', link: '/guide/logging' }
          ]
        }
      ],
      '/features/': [
        {
          text: 'Core Features',
          items: [
            { text: 'Exception Attributes', link: '/features/attributes' },
            { text: 'Custom Handlers', link: '/features/custom-handlers' },
            { text: 'JSON Field Names', link: '/features/json-field-names' },
            { text: 'Problem Details (RFC 9457)', link: '/features/problem-details' }
          ]
        },
        {
          text: 'Integration Features',
          collapsed: false,
          items: [
            { text: 'OpenTelemetry Tracing', link: '/features/telemetry' },
            { text: 'Localization', link: '/features/localization' },
            { text: 'OpenAPI (.NET 9+)', link: '/features/openapi' },
            { text: 'Swashbuckle (.NET 6-8)', link: '/features/swashbuckle' },
            { text: 'Rate Limiting (.NET 7+)', link: '/features/rate-limiting' }
          ]
        }
      ],
      '/reference/': [
        {
          text: 'Reference',
          items: [
            { text: 'API Reference', link: '/reference/api' },
            { text: 'Configuration Template', link: '/reference/template' }
          ]
        }
      ]
    },

    socialLinks: [
      { icon: 'github', link: 'https://github.com/AhmedV20/error-handling-dotnet' }
    ],

    editLink: {
      pattern: 'https://github.com/AhmedV20/error-handling-dotnet/edit/main/docs/site/:path',
      text: 'Edit this page on GitHub'
    },

    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © 2026 ErrorLens'
    },

    search: {
      provider: 'local'
    },

    outline: {
      level: [2, 3]
    }
  }
})
