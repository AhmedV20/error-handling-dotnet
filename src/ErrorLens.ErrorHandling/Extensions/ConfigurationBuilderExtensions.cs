using Microsoft.Extensions.Configuration;
using NetEscapades.Configuration.Yaml;

namespace ErrorLens.ErrorHandling.Extensions;

/// <summary>
/// Extension methods for adding YAML configuration support for error handling.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds a YAML configuration file for error handling settings.
    /// </summary>
    /// <param name="builder">The configuration builder.</param>
    /// <param name="path">Path to the YAML file. Default: "errorhandling.yml"</param>
    /// <param name="optional">Whether the file is optional. Default: true</param>
    /// <param name="reloadOnChange">Whether to reload on file changes. Default: true</param>
    /// <returns>The configuration builder for chaining.</returns>
    public static IConfigurationBuilder AddYamlErrorHandling(
        this IConfigurationBuilder builder,
        string path = "errorhandling.yml",
        bool optional = true,
        bool reloadOnChange = true)
    {
        return builder.AddYamlFile(path, optional, reloadOnChange);
    }
}
