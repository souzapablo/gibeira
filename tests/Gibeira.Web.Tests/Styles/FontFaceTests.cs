using System.Text.RegularExpressions;

namespace Gibeira.Web.Tests.Styles;

public sealed partial class FontFaceTests
{
    private static readonly string[] Families = ["Bricolage Grotesque", "Instrument Sans"];

    // The fonts come from wwwroot and no stylesheet reaches another host (UI-04).
    [Fact]
    public void FontFacesPointToLocalFiles()
    {
        var wwwroot = Path.Combine(RepositoryRoot.Find(), "src", "Host", "Gibeira.Web", "wwwroot");
        var tokens = Path.Combine(wwwroot, "css", "tokens.css");
        var faces = FontFace().Matches(File.ReadAllText(tokens)).Select(match => match.Value).ToArray();

        var missingFamilies = Families
            .Where(family => !faces.Any(face => face.Contains($"\"{family}\"", StringComparison.Ordinal)))
            .ToArray();
        Assert.Empty(missingFamilies);

        var wrongSources = faces
            .SelectMany(face => Url().Matches(face).Select(match => match.Groups["url"].Value))
            .Where(url => !IsRelative(url) || !File.Exists(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(tokens)!, url))))
            .ToArray();
        Assert.Empty(wrongSources);
        Assert.All(faces, face => Assert.Matches(Url(), face));

        var lib = Path.Combine(wwwroot, "lib") + Path.DirectorySeparatorChar;
        var remoteReferences = Directory.EnumerateFiles(wwwroot, "*.css", SearchOption.AllDirectories)
            .Where(path => !path.StartsWith(lib, StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => (path, line, number: index + 1)))
            .Where(entry => entry.line.Contains("@import", StringComparison.OrdinalIgnoreCase)
                || Url().Matches(entry.line).Any(match => IsRemote(match.Groups["url"].Value)))
            .Select(entry => $"{Path.GetRelativePath(wwwroot, entry.path)}:{entry.number}")
            .ToArray();
        Assert.Empty(remoteReferences);
    }

    private static bool IsRelative(string url) =>
        !url.StartsWith('/') && !url.Contains(':', StringComparison.Ordinal);

    private static bool IsRemote(string url) =>
        url.StartsWith("//", StringComparison.Ordinal)
        || url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https:", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"@font-face\s*\{[^}]*\}")]
    private static partial Regex FontFace();

    [GeneratedRegex(@"url\(\s*[""']?(?<url>[^""')]+)[""']?\s*\)")]
    private static partial Regex Url();
}