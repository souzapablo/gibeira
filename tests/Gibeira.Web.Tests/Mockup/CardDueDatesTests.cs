using Gibeira.Web.Ledger;
using Gibeira.Web.Mockup;

namespace Gibeira.Web.Tests.Mockup;

public sealed class CardDueDatesTests
{
    private static readonly Card Itau = new("Itaú", 10);

    // ADD-14: the card's first due date on or after the form's date.
    [Theory]
    [InlineData(2026, 10, 29, 2026, 11, 10)]
    [InlineData(2026, 11, 10, 2026, 11, 10)]
    [InlineData(2026, 11, 9, 2026, 11, 10)]
    [InlineData(2026, 12, 15, 2027, 1, 10)]
    public void FirstDueDateOnOrAfter(int year, int month, int day, int dueYear, int dueMonth, int dueDay) =>
        Assert.Equal(new DateOnly(dueYear, dueMonth, dueDay), CardDueDates.DueDate(Itau, new DateOnly(year, month, day), 0));

    // ADD-15: > and < move the due date one month (10/11/2026 to 10/12/2026 and 10/10/2026 for Itaú).
    [Theory]
    [InlineData(1, 2026, 12, 10)]
    [InlineData(-1, 2026, 10, 10)]
    [InlineData(2, 2027, 1, 10)]
    public void DueDateStepsByMonth(int monthsAhead, int year, int month, int day) =>
        Assert.Equal(new DateOnly(year, month, day), CardDueDates.DueDate(Itau, new DateOnly(2026, 10, 29), monthsAhead));

    // A due day the month lacks falls on its last day, and stepping does not drift.
    [Fact]
    public void DueDayClampsToMonthLength()
    {
        var card = new Card("Fim", 31);
        var from = new DateOnly(2026, 2, 1);

        Assert.Equal(new DateOnly(2026, 2, 28), CardDueDates.DueDate(card, from, 0));
        Assert.Equal(new DateOnly(2026, 3, 31), CardDueDates.DueDate(card, from, 1));
        Assert.Equal(new DateOnly(2026, 4, 30), CardDueDates.DueDate(card, from, 2));
    }
}