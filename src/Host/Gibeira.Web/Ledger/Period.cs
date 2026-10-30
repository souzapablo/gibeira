using System.Globalization;

namespace Gibeira.Web.Ledger;

// The twelve months the ledger shows, starting at the first day of a month.
public sealed record Period
{
    private Period(DateOnly first) => First = first;

    public DateOnly First { get; }

    public static Period Starting(DateOnly firstMonth) => new(new DateOnly(firstMonth.Year, firstMonth.Month, 1));

    public Period Shift(int months) => new(First.AddMonths(months));

    public IEnumerable<DateOnly> Months => Enumerable.Range(0, 12).Select(First.AddMonths);

    // out/2026 - set/2027
    public string Label
    {
        get
        {
            var last = First.AddMonths(11);
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{PtBr.MonthAbbrev(First.Month)}/{First.Year} - {PtBr.MonthAbbrev(last.Month)}/{last.Year}");
        }
    }

    // The 25 periods from 12 months before to 12 months after today's month.
    public static IReadOnlyList<Period> Choices(DateOnly today) =>
        Enumerable.Range(-12, 25).Select(Starting(today).Shift).ToList();
}