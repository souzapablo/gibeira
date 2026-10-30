using Gibeira.Web.Ledger;

namespace Gibeira.Web.Mockup;

// Temporary (AD-007): folds occurrences into the month tables the page draws.
public static class LedgerCalculator
{
    // The opening balance is an entrada occurrence, so a day's saldo is the signed sum of every occurrence up to that day.
    public static IReadOnlyList<MonthLedger> Months(IReadOnlyCollection<Occurrence> occurrences, Period period, DateOnly today)
    {
        var byDay = occurrences.ToLookup(o => o.Date);
        var saldo = occurrences.Where(o => o.Date < period.First).Sum(Signed);
        var months = new List<MonthLedger>();
        foreach (var month in period.Months)
        {
            var days = new List<DayRow>();
            for (var date = month; date.Month == month.Month; date = date.AddDays(1))
            {
                saldo += byDay[date].Sum(Signed);
                days.Add(new DayRow(date, ByKind(byDay[date]), saldo, date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday, date == today));
            }

            months.Add(new MonthLedger(month, days, ByKind(days.SelectMany(d => byDay[d.Date]))));
        }

        return months;
    }

    private static decimal Signed(Occurrence o) => o.Item.Data.Kind == Kind.Entrada ? o.Amount : -o.Amount;

    // Every kind, zero when it has no occurrence.
    private static Dictionary<Kind, decimal> ByKind(IEnumerable<Occurrence> occurrences)
    {
        var sums = Enum.GetValues<Kind>().ToDictionary(k => k, _ => 0m);
        foreach (var o in occurrences)
        {
            sums[o.Item.Data.Kind] += o.Amount;
        }

        return sums;
    }
}