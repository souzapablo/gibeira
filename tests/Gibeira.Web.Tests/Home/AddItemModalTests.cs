using AngleSharp.Dom;

using Bunit;

using Gibeira.Web.Components.Home;
using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;
using Gibeira.Web.Mockup;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Gibeira.Web.Tests.Home;

public sealed class AddItemModalTests : BunitContext
{
    private static readonly DateOnly Today = new(2026, 10, 29);

    private readonly List<NewItem> added = [];
    private int closes;

    public AddItemModalTests()
    {
        var time = new FixedTime(Today);
        Services.AddSingleton<ILedgerStore>(new InMemoryLedgerStore(time));
    }

    private IRenderedComponent<AddItemModal> RenderModal(Kind kind = Kind.Entrada) => Render<AddItemModal>(parameters => parameters
        .Add(p => p.Form, new AddItemForm(kind, Today))
        .Add(p => p.OnAdd, item => added.Add(item))
        .Add(p => p.OnClose, () => closes++));

    // The rows in order: amount, type, description, data, repetition, tags.
    private static IElement FieldRow(IRenderedComponent<AddItemModal> modal, int index) => modal.FindAll(".field-row")[index];

    // A row's label, then its value when it has one: "data 29/10/26".
    private static string Text(IElement row) =>
        string.Join(' ', new[] { row.QuerySelector(".field-row-label"), row.QuerySelector(".field-row-value") }
            .Where(e => e is not null)
            .Select(e => e!.TextContent.Trim()));

    // The type row's name, without the letter of its kind badge.
    private static string TypeName(IRenderedComponent<AddItemModal> modal) => FieldRow(modal, 1).QuerySelector(".field-row-label")!.TextContent.Trim();

    private static IElement[] Options(IRenderedComponent<AddItemModal> modal) => modal.FindAll(".dropdown-option").ToArray();

    private static IElement Option(IRenderedComponent<AddItemModal> modal, string label) =>
        modal.FindAll(".dropdown-option").Single(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim() == label);

    private static string StepperValue(IRenderedComponent<AddItemModal> modal) => modal.Find(".stepper .stepper-value").TextContent.Trim();

    private static void Step(IRenderedComponent<AddItemModal> modal, string label, int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            modal.Find($".stepper button[aria-label='{label}']").Click();
        }
    }

    private static void Save(IRenderedComponent<AddItemModal> modal) => modal.Find(".stepper .button").Click();

    private static void ChooseType(IRenderedComponent<AddItemModal> modal, string type)
    {
        FieldRow(modal, 1).Click();
        Option(modal, type).Click();
    }

    // ADD-01: title, X, the six rows with their icons, chevrons on the four pickers, adicionar entrada, the amount focused.
    [Fact]
    public void AddModalOpensWithRowsAndFocusedAmount()
    {
        var modal = RenderModal();

        Assert.Equal("adicionar", modal.Find(".modal-title").TextContent.Trim());
        Assert.NotNull(modal.Find(".modal-header button[aria-label='fechar']"));

        var rows = modal.FindAll(".field-row");
        Assert.Equal(6, rows.Count);
        Assert.All(rows, r => Assert.NotEmpty(r.QuerySelector(".field-row-icon")!.Children));

        var amount = rows[0].QuerySelector("input.amount-input")!;
        Assert.Equal("0,00", amount.GetAttribute("placeholder"));
        Assert.Equal("R$", rows[0].QuerySelector(".field-row-icon")!.TextContent.Trim());
        Assert.Equal("entrada", rows[1].QuerySelector(".field-row-label")!.TextContent.Trim());
        Assert.NotNull(rows[1].QuerySelector(".kind-icon"));
        Assert.Equal("entrada", rows[2].QuerySelector("input")!.GetAttribute("placeholder"));
        Assert.Equal("data 29/10/26", Text(rows[3]));
        Assert.Equal("não repete", Text(rows[4]));
        Assert.Equal("tags", Text(rows[5]));

        Assert.Equal([false, true, false, true, true, true], rows.Select(r => r.QuerySelector(".field-row-chevron") is not null));
        Assert.Equal("adicionar entrada", modal.Find(".modal-body > .add-button-slot .button").TextContent.Trim());

        // The amount field is the only element focused on open.
        var target = (ElementReference)JSInterop.VerifyFocusAsyncInvoke().Arguments[0]!;
        Assert.Equal(amount.GetAttribute("blazor:elementReference"), target.Id);
    }

    // ADD-03: the type row lists the five kinds with the current one checked.
    [Fact]
    public void TypeRowListsKindsWithCurrentChecked()
    {
        var modal = RenderModal(Kind.Diario);

        FieldRow(modal, 1).Click();

        var options = Options(modal);
        Assert.Equal(["entrada", "saída", "diário", "economia", "cartão"], options.Select(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim()));
        Assert.Equal("diário", Assert.Single(options, o => o.GetAttribute("aria-pressed") == "true").QuerySelector(".dropdown-label")!.TextContent.Trim());
        Assert.All(options, o => Assert.NotNull(o.QuerySelector(".kind-icon")));
    }

    // ADD-04: a chosen type shows its icon and name, sets the placeholder, and labels and colors the button.
    [Fact]
    public void ChoosingTypeUpdatesRowPlaceholderAndButton()
    {
        var modal = RenderModal();

        ChooseType(modal, "saída");

        Assert.Empty(modal.FindAll(".dropdown"));
        Assert.Equal("saída", TypeName(modal));
        Assert.Contains("var(--kind-saida)", FieldRow(modal, 1).QuerySelector(".kind-icon")!.GetAttribute("style"));
        Assert.Equal("saída", FieldRow(modal, 2).QuerySelector("input")!.GetAttribute("placeholder"));
        var button = modal.Find(".add-button-slot .button");
        Assert.Equal("adicionar saída", button.TextContent.Trim());
        Assert.Contains("button--kind", button.ClassList);
        Assert.Contains("var(--kind-saida)", button.GetAttribute("style"));

        ChooseType(modal, "economia");
        Assert.Equal("adicionar economia", modal.Find(".add-button-slot .button").TextContent.Trim());
        Assert.Contains("var(--kind-economia)", modal.Find(".add-button-slot .button").GetAttribute("style"));
    }

    // ADD-05: the data popover shows dd/mm/yyyy between < and >, each one day, and salvar.
    [Fact]
    public void DataPopoverStepsOneDay()
    {
        var modal = RenderModal();

        FieldRow(modal, 3).Click();

        Assert.Equal("data", modal.Find(".stepper .stepper-title").TextContent.Trim());
        Assert.Equal("29/10/2026", StepperValue(modal));
        Assert.Equal("salvar", modal.Find(".stepper .button").TextContent.Trim());

        Step(modal, "próximo");
        Assert.Equal("30/10/2026", StepperValue(modal));
        Step(modal, "próximo", 2);
        Assert.Equal("01/11/2026", StepperValue(modal));
        Step(modal, "anterior", 4);
        Assert.Equal("28/10/2026", StepperValue(modal));
    }

    // ADD-06: salvar sets the date, shows it as dd/mm/yy and closes the popover.
    [Fact]
    public void DataSalvarSetsDateAndCloses()
    {
        var modal = RenderModal();
        FieldRow(modal, 3).Click();
        Step(modal, "próximo", 3);

        Save(modal);

        Assert.Empty(modal.FindAll(".stepper"));
        Assert.Equal("data 01/11/26", Text(FieldRow(modal, 3)));
        Assert.Equal(new DateOnly(2026, 11, 1), modal.Instance.Form.Date);
    }

    // ADD-07: repetir lists the four options with their descriptions and não repete, the current one checked.
    [Fact]
    public void RepetirListsOptionsWithDescriptions()
    {
        var modal = RenderModal();

        FieldRow(modal, 4).Click();

        Assert.Equal("repetir", modal.Find(".dropdown .dropdown-title").TextContent.Trim());
        var options = Options(modal);
        Assert.Equal(
            ["mensalmente", "semanalmente", "diariamente", "parcelado", "não repete"],
            options.Select(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim()));
        Assert.Equal(
            ["repete o mesmo valor mensalmente", "repete o mesmo valor semanalmente", "repete o mesmo valor diariamente", "divide o valor pelo número de parcelas", null],
            options.Select(o => o.QuerySelector(".dropdown-description")?.TextContent.Trim()));
        Assert.Equal("não repete", Assert.Single(options, o => o.GetAttribute("aria-pressed") == "true").QuerySelector(".dropdown-label")!.TextContent.Trim());
    }

    // ADD-08: mensalmente, semanalmente, diariamente and não repete show on the row.
    [Fact]
    public void ChoosingRepetitionShowsItOnRow()
    {
        var modal = RenderModal();

        foreach (var label in new[] { "mensalmente", "semanalmente", "diariamente", "não repete" })
        {
            FieldRow(modal, 4).Click();
            Option(modal, label).Click();

            Assert.Empty(modal.FindAll(".dropdown"));
            Assert.Equal(label, Text(FieldRow(modal, 4)));
        }

        FieldRow(modal, 4).Click();
        Option(modal, "semanalmente").Click();
        FieldRow(modal, 4).Click();
        Assert.Equal("semanalmente", Options(modal).Single(o => o.GetAttribute("aria-pressed") == "true").QuerySelector(".dropdown-label")!.TextContent.Trim());
        Assert.IsType<Repetition.Weekly>(modal.Instance.Form.Repetition);
    }

    // ADD-09: parcelado opens parcelas from 2 to 48, starting at 2; salvar shows parcelado em <N>x.
    [Fact]
    public void ParceladoOpensCountPopoverFrom2To48()
    {
        var modal = RenderModal();
        FieldRow(modal, 4).Click();

        Option(modal, "parcelado").Click();

        Assert.Empty(modal.FindAll(".dropdown"));
        Assert.Equal("parcelas", modal.Find(".stepper .stepper-title").TextContent.Trim());
        Assert.Equal("2", StepperValue(modal));
        Step(modal, "anterior");
        Assert.Equal("2", StepperValue(modal));
        Step(modal, "próximo");
        Assert.Equal("3", StepperValue(modal));
        Save(modal);

        Assert.Empty(modal.FindAll(".stepper"));
        Assert.Equal("parcelado em 3x", Text(FieldRow(modal, 4)));

        FieldRow(modal, 4).Click();
        Option(modal, "parcelado").Click();
        Assert.Equal("3", StepperValue(modal));
        Step(modal, "próximo", 50);
        Assert.Equal("48", StepperValue(modal));
        Save(modal);
        Assert.Equal("parcelado em 48x", Text(FieldRow(modal, 4)));
        Assert.Equal(new Repetition.Installments(48), modal.Instance.Form.Repetition);
    }

    // ADD-13: choosing cartão opens the card list: header cartão with a disabled add-card button, Itaú, Santander, sem cartão.
    [Fact]
    public void CartaoOpensCardDropdown()
    {
        var modal = RenderModal();

        ChooseType(modal, "cartão");

        Assert.Equal("cartão", modal.Find(".dropdown .dropdown-title").TextContent.Trim());
        var addCard = modal.Find(".dropdown-header button");
        Assert.Equal("adicionar cartão", addCard.TextContent.Trim());
        Assert.True(addCard.HasAttribute("disabled"));
        var options = Options(modal);
        Assert.Equal(["Itaú", "Santander", "sem cartão"], options.Select(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim()));
        Assert.Equal("sem cartão", Assert.Single(options, o => o.GetAttribute("aria-pressed") == "true").QuerySelector(".dropdown-label")!.TextContent.Trim());
        Assert.Equal("cartão", TypeName(modal));
    }

    // ADD-14: Itaú on 29/10/2026 opens vencimento at 10/11/2026.
    [Fact]
    public void ChoosingCardOpensVencimentoAtFirstDueDate()
    {
        var modal = RenderModal();
        ChooseType(modal, "cartão");

        Option(modal, "Itaú").Click();

        Assert.Empty(modal.FindAll(".dropdown"));
        Assert.Equal("vencimento", modal.Find(".stepper .stepper-title").TextContent.Trim());
        Assert.Equal("10/11/2026", StepperValue(modal));
        Assert.Equal("salvar", modal.Find(".stepper .button").TextContent.Trim());
    }

    // ADD-15: > in vencimento goes to 10/12/2026 and < goes back a month.
    [Fact]
    public void VencimentoArrowsStepOneMonth()
    {
        var modal = RenderModal();
        ChooseType(modal, "cartão");
        Option(modal, "Itaú").Click();

        Step(modal, "próximo");
        Assert.Equal("10/12/2026", StepperValue(modal));
        Step(modal, "anterior");
        Assert.Equal("10/11/2026", StepperValue(modal));
        Step(modal, "anterior");
        Assert.Equal("10/10/2026", StepperValue(modal));
    }

    // ADD-16: salvar in vencimento sets the date and labels the row vencimento.
    [Fact]
    public void VencimentoSalvarSetsDateAndRelabelsRow()
    {
        var modal = RenderModal();
        ChooseType(modal, "cartão");
        Option(modal, "Santander").Click();
        Assert.Equal("06/11/2026", StepperValue(modal));
        Step(modal, "próximo");

        Save(modal);

        Assert.Empty(modal.FindAll(".stepper"));
        Assert.Equal("vencimento 06/12/26", Text(FieldRow(modal, 3)));
        Assert.Equal(new DateOnly(2026, 12, 6), modal.Instance.Form.Date);
        Assert.Equal("Santander", modal.Instance.Form.Card!.Name);
    }

    // ADD-17: sem cartão keeps the row labeled data and opens no popover.
    [Fact]
    public void SemCartaoOpensNoVencimento()
    {
        var modal = RenderModal();
        ChooseType(modal, "cartão");

        Option(modal, "sem cartão").Click();

        Assert.Empty(modal.FindAll(".dropdown"));
        Assert.Empty(modal.FindAll(".stepper"));
        Assert.Equal("data 29/10/26", Text(FieldRow(modal, 3)));
        Assert.Null(modal.Instance.Form.Card);
    }

    // ADD-19: the tags list checks several; the row joins them with ", ", or reads tags when none.
    [Fact]
    public void TagsDropdownChecksSeveral()
    {
        var modal = RenderModal();

        FieldRow(modal, 5).Click();
        Assert.Equal(["casa", "mercado", "transporte", "lazer", "saúde"], Options(modal).Select(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim()));
        Assert.Equal(5, modal.FindAll(".dropdown-box").Count);

        Option(modal, "mercado").Click();
        Option(modal, "casa").Click();

        Assert.Single(modal.FindAll(".dropdown"));
        Assert.Equal(["casa", "mercado"], Options(modal).Where(o => o.GetAttribute("aria-pressed") == "true").Select(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim()));
        Assert.Equal("tags mercado, casa", Text(FieldRow(modal, 5)));

        Option(modal, "mercado").Click();
        Option(modal, "casa").Click();
        Assert.Equal("tags", Text(FieldRow(modal, 5)));
    }

    // ADD-20: the add button is disabled while the amount is zero or empty.
    [Fact]
    public void AddButtonIsDisabledForZeroAmount()
    {
        var modal = RenderModal();
        var amount = modal.Find("input.amount-input");
        IElement Button() => modal.Find(".add-button-slot .button");

        Assert.True(Button().HasAttribute("disabled"));
        Button().Click();
        Assert.Empty(added);

        amount.Input("0");
        Assert.True(Button().HasAttribute("disabled"));
        amount.Input("0,00");
        Assert.True(Button().HasAttribute("disabled"));

        amount.Input("12,5");
        Assert.False(Button().HasAttribute("disabled"));
        Button().Click();
        Assert.Equal(12.5m, Assert.Single(added).Amount);
    }

    // ADD-24: X, Esc and a backdrop click close the modal and add nothing.
    [Fact]
    public void ModalClosesOnXEscAndBackdropWithoutAdding()
    {
        var modal = RenderModal();
        modal.Find("input.amount-input").Input("50");

        modal.Find(".modal-header button[aria-label='fechar']").Click();
        Assert.Equal(1, closes);

        modal.Find(".modal").KeyDown(key: "Escape");
        Assert.Equal(2, closes);

        modal.Find(".modal-backdrop").Click();
        Assert.Equal(3, closes);
        Assert.Empty(added);
    }

    // ADD-25: Esc or an outside click closes a dropdown or popover and keeps the previous value.
    [Fact]
    public void PopoverClosesOnEscAndOutsideKeepingValue()
    {
        var modal = RenderModal();

        FieldRow(modal, 3).Click();
        Step(modal, "próximo", 2);
        modal.Find(".stepper").KeyDown(key: "Escape");
        Assert.Empty(modal.FindAll(".stepper"));
        Assert.Equal(0, closes);
        Assert.Equal("data 29/10/26", Text(FieldRow(modal, 3)));

        FieldRow(modal, 3).Click();
        Assert.Equal("29/10/2026", StepperValue(modal));
        Step(modal, "próximo");
        modal.Find(".stepper-layer").Click();
        Assert.Empty(modal.FindAll(".stepper"));
        Assert.Equal("data 29/10/26", Text(FieldRow(modal, 3)));

        FieldRow(modal, 1).Click();
        modal.Find(".dropdown").KeyDown(key: "Escape");
        Assert.Empty(modal.FindAll(".dropdown"));
        FieldRow(modal, 1).Click();
        modal.Find(".dropdown-layer").Click();
        Assert.Empty(modal.FindAll(".dropdown"));
        Assert.Equal("entrada", TypeName(modal));

        FieldRow(modal, 4).Click();
        Option(modal, "mensalmente").Click();
        FieldRow(modal, 4).Click();
        Option(modal, "parcelado").Click();
        Step(modal, "próximo", 4);
        modal.Find(".stepper").KeyDown(key: "Escape");
        Assert.Equal("mensalmente", Text(FieldRow(modal, 4)));

        ChooseType(modal, "cartão");
        Option(modal, "Itaú").Click();
        Step(modal, "próximo");
        modal.Find(".stepper-layer").Click();
        Assert.Equal("data 29/10/26", Text(FieldRow(modal, 3)));
        Assert.Equal(0, closes);
        Assert.Empty(added);

        // The dismissed vencimento took no card: the card list still checks sem cartão and the item has none.
        ChooseType(modal, "cartão");
        Assert.Equal("true", Option(modal, "sem cartão").GetAttribute("aria-pressed"));
        modal.Find(".dropdown").KeyDown(key: "Escape");
        modal.Find("input.amount-input").Input("100");
        modal.Find(".add-button-slot .button").Click();
        Assert.Null(added.Single().Card);
        Assert.Equal(Today, added.Single().Date);
    }
}