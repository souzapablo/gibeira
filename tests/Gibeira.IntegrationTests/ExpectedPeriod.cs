using System.Globalization;

namespace Gibeira.IntegrationTests;

// What the home page should show today, written out from the spec rather than taken from Gibeira.Web (HOME-02, HOME-07).
internal static class ExpectedPeriod
{
    private static readonly string[] Abbrevs = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

    private static readonly string[] Names =
        ["janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"];

    private static DateOnly First
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return new DateOnly(today.Year, today.Month, 1);
        }
    }

    // out/2026 - set/2027
    public static string Label
    {
        get
        {
            var first = First;
            var last = first.AddMonths(11);
            return string.Create(CultureInfo.InvariantCulture, $"{Abbrevs[first.Month - 1]}/{first.Year} - {Abbrevs[last.Month - 1]}/{last.Year}");
        }
    }

    // outubro de 2026, ..., setembro de 2027
    public static string[] MonthTitles
    {
        get
        {
            var first = First;
            return Enumerable.Range(0, 12)
                .Select(first.AddMonths)
                .Select(m => string.Create(CultureInfo.InvariantCulture, $"{Names[m.Month - 1]} de {m.Year}"))
                .ToArray();
        }
    }
}