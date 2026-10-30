using System.Globalization;

using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.Ledger;

public sealed class PtBrTests
{
    public static TheoryData<int, string> MonthAbbrevs => new()
    {
        { 1, "jan" },
        { 2, "fev" },
        { 3, "mar" },
        { 4, "abr" },
        { 5, "mai" },
        { 6, "jun" },
        { 7, "jul" },
        { 8, "ago" },
        { 9, "set" },
        { 10, "out" },
        { 11, "nov" },
        { 12, "dez" },
    };

    public static TheoryData<int, string> MonthNames => new()
    {
        { 1, "janeiro" },
        { 2, "fevereiro" },
        { 3, "março" },
        { 4, "abril" },
        { 5, "maio" },
        { 6, "junho" },
        { 7, "julho" },
        { 8, "agosto" },
        { 9, "setembro" },
        { 10, "outubro" },
        { 11, "novembro" },
        { 12, "dezembro" },
    };

    // HOME-08: R$ 1.189,27.
    [Theory]
    [InlineData("1189.27", "R$ 1.189,27")]
    [InlineData("0", "R$ 0,00")]
    [InlineData("15500", "R$ 15.500,00")]
    [InlineData("6.5", "R$ 6,50")]
    [InlineData("999999999.99", "R$ 999.999.999,99")]
    public void MoneyFormatsWithGroupsAndComma(string value, string expected) =>
        Assert.Equal(expected, PtBr.Money(decimal.Parse(value, CultureInfo.InvariantCulture)));

    // HOME-11: R$ -477,67.
    [Theory]
    [InlineData("-477.67", "R$ -477,67")]
    [InlineData("-1189.27", "R$ -1.189,27")]
    public void NegativeAmountFormatsWithMinusAfterSymbol(string value, string expected) =>
        Assert.Equal(expected, PtBr.Money(decimal.Parse(value, CultureInfo.InvariantCulture)));

    [Theory]
    [MemberData(nameof(MonthAbbrevs))]
    public void MonthAbbrevIsThreeLowercaseLetters(int month, string abbrev) =>
        Assert.Equal(abbrev, PtBr.MonthAbbrev(month));

    // DAY-03: 10/nov, the day without a leading zero.
    [Theory]
    [MemberData(nameof(MonthAbbrevs))]
    public void DayTitleIsDayAndMonthAbbrev(int month, string abbrev)
    {
        Assert.Equal($"10/{abbrev}", PtBr.DayTitle(new DateOnly(2026, month, 10)));
        Assert.Equal($"5/{abbrev}", PtBr.DayTitle(new DateOnly(2026, month, 5)));
    }

    // HOME-07: dezembro de 2026.
    [Theory]
    [MemberData(nameof(MonthNames))]
    public void MonthTitleIsLowercaseFullName(int month, string name)
    {
        Assert.Equal($"{name} de 2026", PtBr.MonthTitle(new DateOnly(2026, month, 1)));
        Assert.Equal($"{name} de 2027", PtBr.MonthTitle(new DateOnly(2027, month, 28)));
    }

    // DAY-04 dd/mm, ADD-01 and ADD-06 dd/mm/yy, ADD-05 and ADD-14 dd/mm/yyyy.
    [Theory]
    [InlineData(2027, 1, 5, "05/01", "05/01/27", "05/01/2027")]
    [InlineData(2027, 2, 28, "28/02", "28/02/27", "28/02/2027")]
    [InlineData(2027, 3, 31, "31/03", "31/03/27", "31/03/2027")]
    [InlineData(2027, 4, 1, "01/04", "01/04/27", "01/04/2027")]
    [InlineData(2027, 5, 9, "09/05", "09/05/27", "09/05/2027")]
    [InlineData(2027, 6, 30, "30/06", "30/06/27", "30/06/2027")]
    [InlineData(2027, 7, 14, "14/07", "14/07/27", "14/07/2027")]
    [InlineData(2027, 8, 3, "03/08", "03/08/27", "03/08/2027")]
    [InlineData(2027, 9, 22, "22/09", "22/09/27", "22/09/2027")]
    [InlineData(2026, 10, 29, "29/10", "29/10/26", "29/10/2026")]
    [InlineData(2026, 11, 10, "10/11", "10/11/26", "10/11/2026")]
    [InlineData(2030, 12, 6, "06/12", "06/12/30", "06/12/2030")]
    public void DateFormatsPadDayAndMonth(int year, int month, int day, string shortDate, string rowDate, string longDate)
    {
        var date = new DateOnly(year, month, day);

        Assert.Equal(shortDate, PtBr.ShortDate(date));
        Assert.Equal(rowDate, PtBr.RowDate(date));
        Assert.Equal(longDate, PtBr.LongDate(date));
    }

    // ADD-03 lists the singular names; HOME-07 and DAY-06 the plural ones.
    [Theory]
    [InlineData(Kind.Entrada, "entrada", "entradas")]
    [InlineData(Kind.Saida, "saída", "saídas")]
    [InlineData(Kind.Diario, "diário", "diários")]
    [InlineData(Kind.Economia, "economia", "economias")]
    [InlineData(Kind.Cartao, "cartão", "cartão")]
    public void KindNamesAreSingularAndPlural(Kind kind, string singular, string plural)
    {
        Assert.Equal(singular, PtBr.KindName(kind));
        Assert.Equal(plural, PtBr.KindPlural(kind));
    }
}