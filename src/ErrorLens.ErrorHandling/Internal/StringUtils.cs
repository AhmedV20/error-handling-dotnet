namespace ErrorLens.ErrorHandling.Internal;

/// <summary>
/// Shared string utilities for internal use.
/// </summary>
internal static class StringUtils
{
    /// <summary>
    /// Converts a PascalCase name to camelCase with dotted-path support.
    /// Each segment separated by '.' is independently converted.
    /// Example: "Address.ZipCode" → "address.zipCode"
    /// </summary>
    internal static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Handle dotted paths like "Address.ZipCode"
        if (name.Contains('.'))
        {
            var parts = name.Split('.');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                    parts[i] = char.ToLowerInvariant(parts[i][0]) + parts[i][1..];
            }

            return string.Join(".", parts);
        }

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
