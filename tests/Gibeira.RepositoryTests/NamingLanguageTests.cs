namespace Gibeira.RepositoryTests;

public sealed class NamingLanguageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"gibeira-naming-{Guid.NewGuid():N}");

    // Code is in English; Portuguese appears only in text users see on screens (MOD-09, AD-011).
    [Fact]
    public async Task CodeHasNoPortugueseIdentifiers()
    {
        var root = RepositoryRoot.Find();
        var files = await RepositoryRoot.ListFilesAsync(root, TestContext.Current.CancellationToken);

        var found = PortugueseIdentifiers.Scan(root, files);

        // Every line in the message: Assert.Empty would cut the list short.
        Assert.True(found.Length == 0, $"Portuguese identifiers at:{Environment.NewLine}{string.Join(Environment.NewLine, found)}");
    }

    // A Portuguese word of the spec's naming table used as an identifier fails the scan, named by file and line (MOD-09).
    // Screen text, string literals and comments are not identifiers.
    [Fact]
    public void PortugueseIdentifierIsReportedWithFileAndLine()
    {
        Write("src/A/Sample.cs",
            "namespace A;",
            "// saldo is a word in a comment",
            "public enum Kind { Income, Cartao }",
            "public static class Labels",
            "{",
            "    public const string Name = \"saldo\";",
            "    public static decimal SaldoTotal => 0;",
            "    /* entrada",
            "       vencimento */",
            "    public static string Due => @\"vencimento \"\"entrada\"\"\";",
            "    public static char C => '\\'';",
            "    public static string Text(int n) => $\"parcela {n}\";",
            "}");
        Write("src/A/Page.razor",
            "<th scope=\"col\">saldos</th>",
            "<td class=\"saldo-cell\">@row.Value</td>",
            "@* Kind.Cartao in a comment *@",
            "<p>entrada</p>",
            "<KindIcon Kind=\"Kind.Entrada\" />",
            "<td class=\"@(negative ? \"balance-cell\" : \"cell\")\"></td>",
            "@code {",
            "    // vencimento",
            "    private string label = \"economia\";",
            "    private int diarioCount;",
            "}");
        Write("tests/B/site.css",
            "/* saldo tints */",
            ".balance-cell { color: var(--kind-entrada); }",
            ".note::after { content: \"cartão\"; }",
            ".saldo-cell--positive { }",
            ":root { --lancamento-gap: 1px; }");
        Write("docs/Note.cs", "public enum Saldo { }");
        Write("src/A/readme.md", "Saldo");

        var found = PortugueseIdentifiers.Scan(_root, ["src/A/Sample.cs", "src/A/Page.razor", "tests/B/site.css", "docs/Note.cs", "src/A/readme.md"]);

        Assert.Equal(
        [
            "src/A/Page.razor:2",
            "src/A/Page.razor:5",
            "src/A/Page.razor:10",
            "src/A/Sample.cs:3",
            "src/A/Sample.cs:7",
            "tests/B/site.css:2",
            "tests/B/site.css:4",
            "tests/B/site.css:5",
        ], found);
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