using System.Text.RegularExpressions;

namespace Gibeira.Web.Tests.Styles;

// Finds color literals in a web project's .razor and .css files (UI-02).
public static partial class ColorLiterals
{
    private static readonly string[] Extensions = [".razor", ".css"];

    // Every literal outside wwwroot/css/tokens.css, wwwroot/lib/ and the build output, as file:line.
    public static string[] Scan(string projectRoot)
    {
        var tokens = Path.Combine(projectRoot, "wwwroot", "css", "tokens.css");
        string[] skipped =
        [
            Path.Combine(projectRoot, "wwwroot", "lib") + Path.DirectorySeparatorChar,
            Path.Combine(projectRoot, "bin") + Path.DirectorySeparatorChar,
            Path.Combine(projectRoot, "obj") + Path.DirectorySeparatorChar,
        ];

        return Directory.EnumerateFiles(projectRoot, "*", SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Where(path => !string.Equals(path, tokens, StringComparison.OrdinalIgnoreCase))
            .Where(path => !skipped.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .Order(StringComparer.Ordinal)
            .SelectMany(path => File.ReadLines(path).Select((line, index) => (path, line, number: index + 1)))
            .Where(entry => Literal().IsMatch(entry.line))
            .Select(entry => $"{Path.GetRelativePath(projectRoot, entry.path)}:{entry.number}")
            .ToArray();
    }

    // #rgb, #rrggbb, #rrggbbaa, rgb(, rgba(, hsl(, hsla(; an id such as #add-button or an entity such as &#123; is not one.
    [GeneratedRegex(@"(?<![\w&])#(?:[0-9a-f]{8}|[0-9a-f]{6}|[0-9a-f]{3})(?![\w-])|\b(?:rgba?|hsla?)\(", RegexOptions.IgnoreCase)]
    private static partial Regex Literal();
}