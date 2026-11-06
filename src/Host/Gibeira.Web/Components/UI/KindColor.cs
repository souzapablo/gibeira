using Gibeira.Web.Ledger;

namespace Gibeira.Web.Components.UI;

public static class KindColor
{
    // The token that holds a kind's color, as a CSS value.
    public static string Var(Kind kind) => kind switch
    {
        Kind.Income => "var(--kind-income)",
        Kind.Expense => "var(--kind-expense)",
        Kind.DailySpending => "var(--kind-daily-spending)",
        Kind.Savings => "var(--kind-savings)",
        Kind.CreditCard => "var(--kind-credit-card)",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}