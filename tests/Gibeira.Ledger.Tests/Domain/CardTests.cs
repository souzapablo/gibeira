using Gibeira.Ledger.Domain;

namespace Gibeira.Ledger.Tests.Domain;

public sealed class CardTests
{
    private static readonly Card Itau = Card.Create("Itaú", 10);

    // CARD-01: a card reads back its id, name and due day.
    [Fact]
    public void CardReadsBackIdNameAndDueDay()
    {
        var card = Card.Create("Santander", 6);

        Assert.NotEqual(Guid.Empty, card.Id.Value);
        Assert.NotEqual(Itau.Id, card.Id);
        Assert.Equal("Santander", card.Name);
        Assert.Equal(6, card.DueDay);
    }

    // CARD-02: due days 0 and 32 are refused.
    [Theory]
    [InlineData(0)]
    [InlineData(32)]
    public void DueDayOutsideMonthIsRefused(int dueDay) =>
        Assert.Throws<LedgerRuleException>(() => Card.Create("Itaú", dueDay));

    // CARD-03: a blank card name is refused.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankCardNameIsRefused(string name) =>
        Assert.Throws<LedgerRuleException>(() => Card.Create(name, 10));

    // CARD-04: the first due date on or after the given date.
    [Theory]
    [InlineData(2026, 10, 29, 2026, 11, 10)]
    [InlineData(2026, 11, 10, 2026, 11, 10)]
    [InlineData(2026, 11, 9, 2026, 11, 10)]
    [InlineData(2026, 12, 15, 2027, 1, 10)]
    public void FirstDueDateOnOrAfter(int year, int month, int day, int dueYear, int dueMonth, int dueDay) =>
        Assert.Equal(new DateOnly(dueYear, dueMonth, dueDay), Itau.DueDate(new DateOnly(year, month, day), 0));

    // CARD-04: Itaú (day 10) from 29/10/2026 gives 10/11/2026, and one month ahead 10/12/2026.
    [Theory]
    [InlineData(0, 2026, 11, 10)]
    [InlineData(1, 2026, 12, 10)]
    [InlineData(-1, 2026, 10, 10)]
    [InlineData(2, 2027, 1, 10)]
    public void DueDateStepsByMonth(int monthsAhead, int year, int month, int day) =>
        Assert.Equal(new DateOnly(year, month, day), Itau.DueDate(new DateOnly(2026, 10, 29), monthsAhead));

    // CARD-05: a card due on day 31 is due on February's last day, and stepping does not drift.
    [Fact]
    public void DueDayClampsToMonthLength()
    {
        var card = Card.Create("Fim", 31);
        var from = new DateOnly(2026, 2, 1);

        Assert.Equal(new DateOnly(2026, 2, 28), card.DueDate(from, 0));
        Assert.Equal(new DateOnly(2026, 3, 31), card.DueDate(from, 1));
        Assert.Equal(new DateOnly(2026, 4, 30), card.DueDate(from, 2));
    }
}