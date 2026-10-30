using Gibeira.Web.Ledger;

namespace Gibeira.Web.Components.UI;

public static class KindColor
{
    // The token that holds a kind's color, as a CSS value.
    public static string Var(Kind kind) => kind switch
    {
        Kind.Entrada => "var(--kind-entrada)",
        Kind.Saida => "var(--kind-saida)",
        Kind.Diario => "var(--kind-diario)",
        Kind.Economia => "var(--kind-economia)",
        Kind.Cartao => "var(--kind-cartao)",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };
}