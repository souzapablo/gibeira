using System.Text.RegularExpressions;

namespace Gibeira.Web.Tests.Styles;

public sealed partial class TokenTests
{
    // The token table of spec 002, value by value (UI-01).
    private static readonly string[] CanvasValues =
    [
        // Grounds
        "#15130f", "#0c0b09", "#1d1a15", "#25211b", "#2a261f", "#1a1813", "#2a2316", "#241f15",
        // Lines
        "#2c2820", "#3a342a", "#4a4336", "#26221c",
        // Text
        "#f3ede2", "#b9b0a1", "#958d7e", "#6f6758", "#1a1408", "#ef8f79",
        // Accent (entrada shares #E8B04B)
        "#E8B04B", "#f3c76f",
        // Kinds
        "#DF6A51", "#D65DB1", "#37A675", "#8C7CF0",
        // Fonts
        "Bricolage Grotesque", "Instrument Sans",
        // Radii
        "999px", "24px", "20px", "16px", "14px", "12px",
        // Shadows
        "0 30px 80px rgba(0,0,0,.6)", "0 12px 32px rgba(0,0,0,.45)",
        // Targets
        "44px",
    ];

    [Fact]
    public void TokensDefineEachCanvasValueOnce()
    {
        var path = Path.Combine(RepositoryRoot.Find(), "src", "Host", "Gibeira.Web", "wwwroot", "css", "tokens.css");
        var values = CustomProperty().Matches(File.ReadAllText(path))
            .Select(match => Normalize(match.Groups["value"].Value))
            .ToArray();

        var wrong = CanvasValues
            .Select(expected => (expected, count: values.Count(value => Holds(value, Normalize(expected)))))
            .Where(entry => entry.count != 1)
            .Select(entry => $"{entry.expected} is defined {entry.count} times")
            .ToArray();

        Assert.Empty(wrong);
    }

    // A font token holds its family first and fallbacks after it.
    private static bool Holds(string value, string expected) =>
        value == expected || value.StartsWith(expected + ",", StringComparison.Ordinal);

    private static string Normalize(string value) =>
        Whitespace().Replace(value.Replace("\"", "", StringComparison.Ordinal).Replace("'", "", StringComparison.Ordinal).Trim(), " ")
            .Replace(", ", ",", StringComparison.Ordinal)
            .ToLowerInvariant();

    [GeneratedRegex(@"--[\w-]+\s*:\s*(?<value>[^;]+);")]
    private static partial Regex CustomProperty();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}