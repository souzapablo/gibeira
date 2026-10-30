using System.Globalization;

namespace Gibeira.Web.Ledger;

// Every pt-BR text format of the page. Fixed arrays and an explicit NumberFormatInfo,
// so the output never depends on the machine's culture data.
public static class PtBr
{
    private static readonly string[] MonthAbbrevs = ["jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez"];

    private static readonly string[] MonthNames =
        ["janeiro", "fevereiro", "março", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"];

    private static readonly NumberFormatInfo Numbers = NumberFormatInfo.ReadOnly(new NumberFormatInfo
    {
        NumberGroupSeparator = ".",
        NumberDecimalSeparator = ",",
        NegativeSign = "-",
    });

    // R$ 1.189,27 and R$ -477,67.
    public static string Money(decimal value) => "R$ " + value.ToString("#,##0.00", Numbers);

    // out
    public static string MonthAbbrev(int month) => MonthAbbrevs[month - 1];

    // dezembro de 2026
    public static string MonthTitle(DateOnly month) =>
        string.Create(CultureInfo.InvariantCulture, $"{MonthNames[month.Month - 1]} de {month.Year}");

    // 10/nov
    public static string DayTitle(DateOnly date) =>
        string.Create(CultureInfo.InvariantCulture, $"{date.Day}/{MonthAbbrev(date.Month)}");

    // 10/11
    public static string ShortDate(DateOnly date) => date.ToString("dd/MM", CultureInfo.InvariantCulture);

    // 29/10/26
    public static string RowDate(DateOnly date) => date.ToString("dd/MM/yy", CultureInfo.InvariantCulture);

    // 10/11/2026
    public static string LongDate(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string KindName(Kind kind) => kind switch
    {
        Kind.Entrada => "entrada",
        Kind.Saida => "saída",
        Kind.Diario => "diário",
        Kind.Economia => "economia",
        Kind.Cartao => "cartão",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    // The column and filter names; cartão stays singular.
    public static string KindPlural(Kind kind) => kind == Kind.Cartao ? "cartão" : KindName(kind) + "s";
}