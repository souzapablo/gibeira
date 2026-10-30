using Gibeira.Web.Ledger;

namespace Gibeira.Web.Mockup;

// Temporary (AD-007): when a card's bill falls due.
public static class CardDueDates
{
    // The card's first due date on or after a date, moved by monthsAhead months.
    public static DateOnly DueDate(Card card, DateOnly onOrAfter, int monthsAhead)
    {
        var month = new DateOnly(onOrAfter.Year, onOrAfter.Month, 1);
        if (DueIn(card, month) < onOrAfter)
        {
            month = month.AddMonths(1);
        }

        return DueIn(card, month.AddMonths(monthsAhead));
    }

    // The due day, or the month's last day when the month is shorter.
    private static DateOnly DueIn(Card card, DateOnly month) =>
        new(month.Year, month.Month, Math.Min(card.DueDay, DateTime.DaysInMonth(month.Year, month.Month)));
}