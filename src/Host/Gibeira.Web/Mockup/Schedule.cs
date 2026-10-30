using System.Globalization;

using Gibeira.Web.Ledger;

namespace Gibeira.Web.Mockup;

// Temporary (AD-007): the mockup's records and repetition rules, deleted when a finance module provides the data.

// An added item and its place in the order of adding.
public sealed record LedgerItem(Guid Id, long Sequence, NewItem Data);

// One day an item falls on, with that day's amount.
public sealed record Occurrence(LedgerItem Item, DateOnly Date, decimal Amount, string? RepetitionLabel);

public static class Schedule
{
    // The item's occurrences from `from` to `to`, both included, in date order.
    public static IEnumerable<Occurrence> Expand(LedgerItem item, DateOnly from, DateOnly to) =>
        All(item).SkipWhile(o => o.Date < from).TakeWhile(o => o.Date <= to);

    // Repeating items have no end; Expand's window bounds them.
    private static IEnumerable<Occurrence> All(LedgerItem item)
    {
        var (start, amount) = (item.Data.Date, item.Data.Amount);
        switch (item.Data.Repetition)
        {
            case Repetition.Monthly:
                // AddMonths from the start clamps day 31 to the month's last day without drifting.
                for (var k = 0; ; k++)
                {
                    yield return new(item, start.AddMonths(k), amount, "mensalmente");
                }

            case Repetition.Weekly:
                for (var k = 0; ; k++)
                {
                    yield return new(item, start.AddDays(7 * k), amount, "semanalmente");
                }

            case Repetition.Daily:
                for (var k = 0; ; k++)
                {
                    yield return new(item, start.AddDays(k), amount, "diariamente");
                }

            case Repetition.Installments(var count):
                // Each part is the amount divided by count, cut to cents; the first carries the remainder.
                var part = decimal.Floor(amount / count * 100) / 100;
                for (var k = 0; k < count; k++)
                {
                    var label = string.Create(CultureInfo.InvariantCulture, $"parcela {k + 1}/{count}");
                    yield return new(item, start.AddMonths(k), k == 0 ? amount - (part * (count - 1)) : part, label);
                }

                break;

            default:
                yield return new(item, start, amount, null);
                break;
        }
    }
}