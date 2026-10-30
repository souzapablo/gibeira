namespace Gibeira.Web.Tests.Styles;

public sealed class ColorLiteralTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"gibeira-colors-{Guid.NewGuid():N}");

    // Colors come from tokens.css alone (UI-02).
    [Fact]
    public void NoColorLiteralOutsideTokens()
    {
        var project = Path.Combine(RepositoryRoot.Find(), "src", "Host", "Gibeira.Web");

        Assert.Empty(ColorLiterals.Scan(project));
    }

    // A literal fails the scan and is named by file and line (UI-02).
    [Fact]
    public void ColorLiteralIsReportedWithFileAndLine()
    {
        Write(Path.Combine("Components", "Card.razor"),
            "<div>",
            "    <span style=\"color: #fff\">a</span>",
            "    <span style=\"color: rgb(1, 2, 3)\">b</span>",
            "</div>");
        Write(Path.Combine("wwwroot", "app.css"),
            "body {",
            "    background: rgba(0, 0, 0, .5);",
            "    color: hsl(10, 50%, 50%);",
            "}");
        Write(Path.Combine("wwwroot", "css", "tokens.css"), ":root { --ground: #15130f; }");
        Write(Path.Combine("wwwroot", "lib", "vendor", "vendor.css"), "a { color: #000000; }");

        var found = ColorLiterals.Scan(_root);

        Assert.Equal(
        [
            $"{Path.Combine("Components", "Card.razor")}:2",
            $"{Path.Combine("Components", "Card.razor")}:3",
            $"{Path.Combine("wwwroot", "app.css")}:2",
            $"{Path.Combine("wwwroot", "app.css")}:3",
        ], found.Order(StringComparer.Ordinal));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private void Write(string relativePath, params string[] lines)
    {
        var path = Path.Combine(_root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllLines(path, lines);
    }
}