import { defineConfig } from 'vitepress'
import { withMermaid } from 'vitepress-plugin-mermaid'

export default withMermaid(defineConfig({
  title: 'ErrorLens.ErrorHandling',
  description: 'Consistent, structured error responses for ASP.NET Core REST APIs',
  base: '/error-handling-dotnet/current/',

  head: [
    ['link', { rel: 'icon', type: 'image/png', href: '/error-handling-dotnet/current/icon.png' }]
  ],

  themeConfig: {
    logo: '/icon.png',

    nav: [
      { text: 'Documentation', link: '/documentation' },
      { text: 'Changelog', link: '/changelog' },
      {
        text: 'v1.3.1',
        items: [
          { text: 'NuGet', link: 'https://www.nuget.org/packages/ErrorLens.ErrorHandling' }
        ]
      }
    ],

    sidebar: {
      '/documentation': [
        {
          text: 'Getting Started',
          items: [
            { text: 'Installation', link: '/documentation#installation' },
            { text: 'Quick Start', link: '/documentation#quick-start' },
            { text: 'How It Works', link: '/documentation#how-it-works' },
            { text: 'HTTP Status Mappings', link: '/documentation#default-http-status-mappings' },
            { text: 'Error Code Strategies', link: '/documentation#error-code-strategies' },
          ]
        },
        {
          text: 'Configuration',
          items: [
            { text: 'Configuration', link: '/documentation#configuration' },
            { text: 'JSON Field Names', link: '/documentation#custom-json-field-names' },
          ]
        },
        {
          text: 'Core Features',
          items: [
            { text: 'Exception Attributes', link: '/documentation#exception-attributes' },
            { text: 'Custom Handlers', link: '/documentation#custom-exception-handlers' },
            { text: 'Validation Errors', link: '/documentation#validation-errors' },
            { text: 'Response Customization', link: '/documentation#response-customization' },
            { text: 'Replaceable Mappers', link: '/documentation#replaceable-mappers' },
            { text: 'Problem Details', link: '/documentation#rfc-9457-problem-details' },
          ]
        },
        {
          text: 'Security & Logging',
          items: [
            { text: 'Security', link: '/documentation#security' },
            { text: 'Logging', link: '/documentation#logging' },
          ]
        },
        {
          text: 'Integrations',
          items: [
            { text: 'OpenTelemetry', link: '/documentation#opentelemetry-tracing' },
            { text: 'Localization', link: '/documentation#localization' },
            { text: 'OpenAPI & Swagger', link: '/documentation#openapi-swagger-integration' },
            { text: 'Rate Limiting', link: '/documentation#rate-limiting' },
          ]
        },
        {
          text: 'Reference',
          items: [
            { text: 'Error Code Constants', link: '/documentation#built-in-error-code-constants' },
            { text: 'API Reference', link: '/documentation#api-reference' },
            { text: 'Architecture', link: '/architecture' },
            { text: 'Config Template', link: '/documentation#configuration-template' },
            { text: 'Troubleshooting', link: '/documentation#troubleshooting' },
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
  },

  mermaid: {
    theme: 'base',
    themeVariables: {
      // Match ErrorLens brand colors
      primaryColor: '#5b6ee1',
      primaryTextColor: '#ffffff',
      primaryBorderColor: '#4a5bc7',
      secondaryColor: '#7c8af2',
      secondaryTextColor: '#ffffff',
      secondaryBorderColor: '#6b7ae0',
      tertiaryColor: '#f0f2ff',
      tertiaryTextColor: '#1e293b',
      tertiaryBorderColor: '#c4cbfa',
      lineColor: '#64748b',
      textColor: '#1e293b',
      mainBkg: '#5b6ee1',
      nodeBorder: '#4a5bc7',
      clusterBkg: '#f0f2ff',
      clusterBorder: '#c4cbfa',
      titleColor: '#1e293b',
      edgeLabelBackground: '#ffffff',
      nodeTextColor: '#ffffff',
    }
  },

  mermaidPlugin: {
    class: 'mermaid'
  }
}))
