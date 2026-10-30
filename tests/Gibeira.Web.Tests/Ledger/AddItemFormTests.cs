using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.Ledger;

public sealed class AddItemFormTests
{
    private static readonly DateOnly Today = new(2026, 10, 29);
    private static readonly Card Itau = new("Itaú", 10);

    public static TheoryData<Repetition, string> Repetitions => new()
    {
        { new Repetition.None(), "não repete" },
        { new Repetition.Monthly(), "mensalmente" },
        { new Repetition.Weekly(), "semanalmente" },
        { new Repetition.Daily(), "diariamente" },
        { new Repetition.Installments(3), "parcelado em 3x" },
        { new Repetition.Installments(48), "parcelado em 48x" },
    };

    // ADD-04: the type's name is the placeholder and the button reads adicionar <type>.
    [Theory]
    [InlineData(Kind.Entrada, "entrada", "adicionar entrada")]
    [InlineData(Kind.Saida, "saída", "adicionar saída")]
    [InlineData(Kind.Diario, "diário", "adicionar diário")]
    [InlineData(Kind.Economia, "economia", "adicionar economia")]
    [InlineData(Kind.Cartao, "cartão", "adicionar cartão")]
    public void ChoosingTypeSetsPlaceholderAndButtonLabel(Kind kind, string placeholder, string button)
    {
        var form = new AddItemForm(Kind.Entrada, Today);

        form.SetKind(kind);

        Assert.Equal(kind, form.Kind);
        Assert.Equal(placeholder, form.DescriptionPlaceholder);
        Assert.Equal(button, form.ButtonLabel);
    }

    // ADD-01: the modal opens with adicionar entrada and não repete.
    [Fact]
    public void FormOpensEmptyWithTheGivenKindAndDate()
    {
        var form = new AddItemForm(Kind.Entrada, Today);

        Assert.Equal("", form.AmountText);
        Assert.Equal("", form.Description);
        Assert.Equal(Today, form.Date);
        Assert.Equal("data", form.DateLabel);
        Assert.Equal("não repete", form.RepetitionLabel);
        Assert.Equal("tags", form.TagsLabel);
        Assert.Equal("adicionar entrada", form.ButtonLabel);
    }

    // ADD-08 and ADD-09: the row shows the chosen repetition.
    [Theory]
    [MemberData(nameof(Repetitions))]
    public void RepetitionLabelFollowsChoice(Repetition repetition, string label)
    {
        var form = new AddItemForm(Kind.Saida, Today) { Repetition = repetition };

        Assert.Equal(label, form.RepetitionLabel);
    }

    // ADD-16: salvar in vencimento sets the date and relabels the row.
    [Fact]
    public void DueDateRelabelsDataRowAsVencimento()
    {
        var form = new AddItemForm(Kind.Cartao, Today);

        form.ChooseCard(Itau);
        Assert.Equal("data", form.DateLabel);

        form.SetDueDate(new DateOnly(2026, 11, 10));

        Assert.Equal(Itau, form.Card);
        Assert.Equal(new DateOnly(2026, 11, 10), form.Date);
        Assert.Equal("vencimento", form.DateLabel);
    }

    // ADD-17: sem cartão keeps the row labeled data.
    [Fact]
    public void SemCartaoKeepsDataLabel()
    {
        var form = new AddItemForm(Kind.Cartao, Today);

        form.ChooseCard(null);

        Assert.Null(form.Card);
        Assert.Equal(Today, form.Date);
        Assert.Equal("data", form.DateLabel);
    }

    // ADD-18: leaving cartão clears the card and labels the row data again.
    [Fact]
    public void LeavingCartaoClearsCard()
    {
        var form = new AddItemForm(Kind.Cartao, Today);
        form.ChooseCard(Itau);
        form.SetDueDate(new DateOnly(2026, 11, 10));

        form.SetKind(Kind.Cartao);
        Assert.Equal(Itau, form.Card);
        Assert.Equal("vencimento", form.DateLabel);

        form.SetKind(Kind.Saida);

        Assert.Null(form.Card);
        Assert.Equal("data", form.DateLabel);
    }

    // ADD-19: the checked names joined by ", ", or tags when none.
    [Fact]
    public void TagsLabelJoinsCheckedNames()
    {
        var form = new AddItemForm(Kind.Saida, Today);

        form.ToggleTag("casa");
        form.ToggleTag("mercado");
        Assert.Equal(["casa", "mercado"], form.Tags);
        Assert.Equal("casa, mercado", form.TagsLabel);

        form.ToggleTag("casa");
        Assert.Equal("mercado", form.TagsLabel);

        form.ToggleTag("mercado");
        Assert.Empty(form.Tags);
        Assert.Equal("tags", form.TagsLabel);
    }

    // ADD-02: typing goes through the amount filter.
    [Fact]
    public void TypingKeepsTheTextTheAmountRuleRejects()
    {
        var form = new AddItemForm(Kind.Entrada, Today);

        form.Type("12,3");
        form.Type("12,3a");
        form.Type("12,345");

        Assert.Equal("12,3", form.AmountText);
    }

    // ADD-20: disabled while the amount is zero or empty.
    [Theory]
    [InlineData("", false)]
    [InlineData("0", false)]
    [InlineData("0,", false)]
    [InlineData("0,00", false)]
    [InlineData("0,01", true)]
    [InlineData("100", true)]
    public void AddButtonDisabledWhileAmountZeroOrEmpty(string amount, bool canSubmit)
    {
        var form = new AddItemForm(Kind.Entrada, Today);

        form.Type(amount);

        Assert.Equal(canSubmit, form.CanSubmit);
        if (!canSubmit)
        {
            Assert.Null(form.Submit());
        }
    }

    // ADD-22: an empty description is saved as the type's name.
    [Theory]
    [InlineData(Kind.Saida, "", "saída")]
    [InlineData(Kind.Cartao, "   ", "cartão")]
    [InlineData(Kind.Diario, "padaria", "padaria")]
    public void EmptyDescriptionSavesTypeName(Kind kind, string description, string saved)
    {
        var form = new AddItemForm(kind, Today) { Description = description };
        form.Type("10");

        Assert.Equal(saved, form.Submit()!.Description);
    }

    // ADD-21 and ADD-23: the item carries every field, and only the first submit returns it.
    [Fact]
    public void SubmitReturnsItemOnce()
    {
        var form = new AddItemForm(Kind.Entrada, Today);
        form.SetKind(Kind.Cartao);
        form.Type("1189,27");
        form.Description = "mercado";
        form.ChooseCard(Itau);
        form.SetDueDate(new DateOnly(2026, 11, 10));
        form.Repetition = new Repetition.Installments(3);
        form.ToggleTag("casa");

        var item = form.Submit();

        Assert.NotNull(item);
        Assert.Equal(Kind.Cartao, item.Kind);
        Assert.Equal(1189.27m, item.Amount);
        Assert.Equal("mercado", item.Description);
        Assert.Equal(new DateOnly(2026, 11, 10), item.Date);
        Assert.Equal(new Repetition.Installments(3), item.Repetition);
        Assert.Equal("Itaú", item.Card);
        Assert.Equal(["casa"], item.Tags);

        Assert.False(form.CanSubmit);
        Assert.Null(form.Submit());
    }
}