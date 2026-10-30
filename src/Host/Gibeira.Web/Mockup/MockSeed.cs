using Gibeira.Web.Ledger;

namespace Gibeira.Web.Mockup;

// Temporary (AD-007): the items every load starts with.
public static class MockSeed
{
    public static IReadOnlyList<Card> Cards { get; } = [new("Itaú", 10), new("Santander", 6)];

    public static IReadOnlyList<string> Tags { get; } = ["casa", "mercado", "transporte", "lazer", "saúde"];

    // The opening is the first day of the month 12 months before today's month, so every selectable period has a saldo.
    // Aluguel on day 1 lands before salário on day 5, which keeps the saldo negative for a few days in the first months.
    public static IReadOnlyList<NewItem> Create(DateOnly today)
    {
        var opening = new DateOnly(today.Year, today.Month, 1).AddMonths(-12);
        NewItem Monthly(Kind kind, decimal amount, string description, int day, string? card = null, params string[] tags) =>
            new(kind, amount, description, opening.AddDays(day - 1), new Repetition.Monthly(), card, tags);

        return
        [
            new(Kind.Entrada, 1000m, "saldo inicial", opening, new Repetition.None(), null, []),
            Monthly(Kind.Saida, 1800m, "aluguel", 1, null, "casa"),
            Monthly(Kind.Entrada, 5200m, "salário", 5),
            Monthly(Kind.Economia, 400m, "reserva", 6),
            Monthly(Kind.Cartao, 477.67m, "assinaturas", 6, "Santander", "lazer"),
            Monthly(Kind.Cartao, 950m, "mercado do mês", 10, "Itaú", "mercado"),
            Monthly(Kind.Saida, 119.90m, "internet", 15, null, "casa"),
            Monthly(Kind.Saida, 189.27m, "luz", 20, null, "casa"),
            new(Kind.Diario, 40m, "dia a dia", opening, new Repetition.Daily(), null, []),
        ];
    }
}