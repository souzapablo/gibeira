using System.Text;
using System.Text.RegularExpressions;

namespace Gibeira.RepositoryTests;

// Finds Portuguese words of spec 003's naming table used as identifiers in .cs, .razor and .css files under src/ and tests/ (MOD-09, AD-011).
// Screen text, string literals and comments may hold them; code names may not.
public static partial class PortugueseIdentifiers
{
    private static readonly string[] Words =
    [
        "entrada", "saida", "saída", "diario", "diário", "economia", "cartao", "cartão",
        "saldo", "parcela", "vencimento", "lancamento", "lançamento",
    ];

    private static readonly string[] Folders = ["src/", "tests/"];
    private static readonly string[] Extensions = [".cs", ".razor", ".css"];

    // Every offending line as `path:line`, ordered by path then line; paths are `/`-separated and relative to root.
    public static string[] Scan(string root, IEnumerable<string> relativePaths) =>
        [.. relativePaths
            .Where(path => Folders.Any(folder => path.StartsWith(folder, StringComparison.Ordinal)))
            .Where(path => Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .SelectMany(path => OffendingLines(path, File.ReadAllText(Path.Combine(root, path))).Select(line => $"{path}:{line}"))];

    private static IEnumerable<int> OffendingLines(string path, string text)
    {
        var lines = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".cs" => CodeLines(Mask(text, lineComments: true)),
            ".css" => Lines(Mask(text, lineComments: false)).Select(line => Tokens(CssToken(), line).Any(HasWord)).ToArray(),
            _ => RazorLines(text),
        };

        return lines.Select((offends, index) => (offends, number: index + 1)).Where(line => line.offends).Select(line => line.number);
    }

    // C# with comments and literals blanked: any identifier holding a word.
    private static bool[] CodeLines(string masked) =>
        [.. Lines(masked).Select(line => Tokens(Identifier(), line).Any(HasWord))];

    // Markup up to `@code`, C# from there. In markup, screen text is lowercase words, so only class values,
    // custom properties and mixed-case code names count.
    private static bool[] RazorLines(string text)
    {
        var uncommented = Blank(text, RazorComment());
        var code = CodeStart().Match(uncommented);
        var markupEnd = code.Success ? code.Index : uncommented.Length;

        var markup = Lines(uncommented[..markupEnd]).Select(line =>
            ClassValue().Matches(line).Any(m => HasWord(m.Groups[1].Value))
            || Tokens(CustomProperty(), line).Any(HasWord)
            || Tokens(Identifier(), line).Any(token => token.Any(char.IsUpper) && HasWord(token)));

        // `@code` starts a line, so the markup ends with a line break and its last, empty split is the `@code` line.
        var codeLines = code.Success ? CodeLines(Mask(uncommented[markupEnd..], lineComments: true)) : [];
        return [.. markup.SkipLast(code.Success ? 1 : 0), .. codeLines];
    }

    private static bool HasWord(string token) =>
        Words.Any(word => token.Contains(word, StringComparison.OrdinalIgnoreCase));

    private static string[] Lines(string text) => text.Split('\n');

    private static IEnumerable<string> Tokens(Regex pattern, string line) =>
        pattern.Matches(line).Select(m => m.Value);

    private static string Blank(string text, Regex pattern)
    {
        var result = new StringBuilder(text);
        foreach (Match match in pattern.Matches(text))
        {
            BlankRange(result, match.Index, match.Index + match.Length);
        }

        return result.ToString();
    }

    // Comments and string or char literals become spaces; line breaks stay, so line numbers hold.
    private static string Mask(string text, bool lineComments)
    {
        var result = new StringBuilder(text);
        var i = 0;
        while (i < text.Length)
        {
            var end = SkippedEnd(text, i, lineComments);
            if (end > i)
            {
                BlankRange(result, i, end);
                i = end;
            }
            else
            {
                i++;
            }
        }

        return result.ToString();
    }

    // Where the comment or literal starting at i ends, or i when none starts there.
    private static int SkippedEnd(string text, int i, bool lineComments)
    {
        if (lineComments && At(text, i, "//"))
        {
            return EndOf(text, "\n", i, includeMarker: false);
        }

        if (At(text, i, "/*"))
        {
            return EndOf(text, "*/", i + 2, includeMarker: true);
        }

        if (text[i] == '\'')
        {
            return QuotedEnd(text, i + 1, '\'', verbatim: false);
        }

        // "...", $"...", @"...", $@"...", @$"..." and raw """...""".
        var quote = i;
        while (quote < text.Length && text[quote] is '$' or '@')
        {
            quote++;
        }

        if (quote >= text.Length || text[quote] != '"')
        {
            return i;
        }

        if (At(text, quote, "\"\"\""))
        {
            return EndOf(text, "\"\"\"", quote + 3, includeMarker: true);
        }

        return QuotedEnd(text, quote + 1, '"', verbatim: text.AsSpan(i, quote - i).Contains('@'));
    }

    private static int QuotedEnd(string text, int from, char quote, bool verbatim)
    {
        for (var k = from; k < text.Length; k++)
        {
            if (!verbatim && text[k] == '\\')
            {
                k++;
            }
            else if (text[k] == quote)
            {
                if (verbatim && k + 1 < text.Length && text[k + 1] == quote)
                {
                    k++;
                    continue;
                }

                return k + 1;
            }
        }

        return text.Length;
    }

    private static int EndOf(string text, string marker, int from, bool includeMarker)
    {
        var index = text.IndexOf(marker, from, StringComparison.Ordinal);
        return index < 0 ? text.Length : index + (includeMarker ? marker.Length : 0);
    }

    private static bool At(string text, int i, string marker) =>
        string.CompareOrdinal(text, i, marker, 0, marker.Length) == 0;

    private static void BlankRange(StringBuilder text, int from, int to)
    {
        for (var k = from; k < to; k++)
        {
            if (text[k] is not '\n' and not '\r')
            {
                text[k] = ' ';
            }
        }
    }

    [GeneratedRegex(@"[\p{L}_][\p{L}\p{Nd}_]*")]
    private static partial Regex Identifier();

    [GeneratedRegex(@"[\p{L}\p{Nd}_-]+")]
    private static partial Regex CssToken();

    [GeneratedRegex(@"--[\p{L}][\p{L}\p{Nd}_-]*")]
    private static partial Regex CustomProperty();

    // The value runs to the quote that closes the attribute, so a value holding C# strings is kept whole.
    [GeneratedRegex("""\bclass="(.*?)"(?=[\s/>])""")]
    private static partial Regex ClassValue();

    [GeneratedRegex(@"@\*.*?\*@|<!--.*?-->", RegexOptions.Singleline)]
    private static partial Regex RazorComment();

    [GeneratedRegex(@"^@code\b", RegexOptions.Multiline)]
    private static partial Regex CodeStart();
}