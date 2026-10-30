using System.Globalization;
using System.Text.RegularExpressions;

namespace Gibeira.Web.Ledger;

// The amount field's keystroke filter: digits and one comma with at most two decimals, up to 999.999.999,99.
public static partial class AmountInput
{
    private const decimal Max = 999_999_999.99m;

    // The text becomes the proposed one when the rule accepts it, else it stays the previous one.
    public static string Accept(string previous, string proposed) =>
        Shape().IsMatch(proposed) && Parse(proposed) <= Max ? proposed : previous;

    public static decimal Parse(string text) =>
        text.Length == 0 ? 0 : decimal.Parse(text.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

    // ASCII digits only, at most ten before the comma so the value always fits a decimal.
    [GeneratedRegex(@"^([0-9]{1,10}(,[0-9]{0,2})?)?$")]
    private static partial Regex Shape();
}