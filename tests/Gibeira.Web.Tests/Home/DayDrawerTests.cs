using Bunit;

using Gibeira.Web.Components.Home;
using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

using Microsoft.Extensions.DependencyInjection;

namespace Gibeira.Web.Tests.Home;

public sealed class DayDrawerTests : BunitContext
{
    private static readonly DateOnly Tenth = new(2026, 11, 10);
    private static readonly DateOnly Eleventh = new(2026, 11, 11);

    private static readonly DayEntry Streaming = new(Guid.NewGuid(), Kind.CreditCard, 39.9m, "streaming", Tenth, "mensalmente");
    private static readonly DayEntry Bakery = new(Guid.NewGuid(), Kind.Expense, 12.5m, "padaria", Tenth, null);
    private static readonly DayEntry Fridge = new(Guid.NewGuid(), Kind.CreditCard, 1000m, "geladeira", Tenth, "parcela 2/10");
    private static readonly DayEntry Groceries = new(Guid.NewGuid(), Kind.CreditCard, 230.45m, "mercado", Eleventh, null);
    private static readonly DayEntry Salary = new(Guid.NewGuid(), Kind.Income, 5000m, "salário", Eleventh, "mensalmente");

    private DateOnly day = Tenth;
    private Kind? filter;
    private int closes;
    private readonly List<DayEntry> removed = [];

    public DayDrawerTests() =>
        Services.AddSingleton<ILedgerStore>(new DayStore([Streaming, Bakery, Fridge, Groceries, Salary]));

    private IRenderedComponent<DayDrawer> RenderDrawer() => Render<DayDrawer>(parameters => parameters
        .Bind(p => p.Day, day, value => day = value)
        .Bind(p => p.Filter, filter, value => filter = value)
        .Add(p => p.OnClose, () => closes++)
        .Add(p => p.OnRemove, entry => removed.Add(entry)));

    // Passes the day and filter back down, as the page that owns them does.
    private void Rebind(IRenderedComponent<DayDrawer> drawer) =>
        drawer.Render(parameters => parameters.Add(p => p.Day, day).Add(p => p.Filter, filter));

    private static string[] Descriptions(IRenderedComponent<DayDrawer> drawer) =>
        drawer.FindAll(".entry-description").Select(d => d.TextContent.Trim()).ToArray();

    private static string FilterPill(IRenderedComponent<DayDrawer> drawer) => drawer.Find(".filter-pill").TextContent.Trim();

    // DAY-03: the drawer is titled <d>/<mmm>.
    [Fact]
    public void DrawerTitleIsDayAndMonthAbbrev()
    {
        var drawer = RenderDrawer();

        Assert.Equal("10/nov", drawer.Find(".drawer-title").TextContent.Trim());
        Assert.Equal("10/nov", drawer.Find(".drawer").GetAttribute("aria-label"));
    }

    // DAY-01, DAY-04: X, < title >, + in the header; each entry in added order with icon, description, amount, dd/mm, ⋮ and its repetition label.
    [Fact]
    public void DrawerListsEntriesInAddedOrderWithLabels()
    {
        var drawer = RenderDrawer();

        Assert.Equal(
            ["fechar", "dia anterior", "dia seguinte", "adicionar"],
            drawer.FindAll(".drawer-header button").Select(b => b.GetAttribute("aria-label")));
        Assert.NotNull(drawer.Find(".drawer-header .drawer-title"));
        Assert.Null(drawer.Find(".filter-pill").Closest(".drawer-header"));

        var entries = drawer.FindAll(".entry");
        Assert.Equal(["streaming", "padaria", "geladeira"], Descriptions(drawer));
        Assert.Equal(["R$ 39,90", "R$ 12,50", "R$ 1.000,00"], entries.Select(e => e.QuerySelector(".money")!.TextContent.Trim()));
        Assert.All(entries, e => Assert.Equal("10/11", e.QuerySelector(".entry-date")!.TextContent.Trim()));
        Assert.Equal(["mensalmente", null, "parcela 2/10"], entries.Select(e => e.QuerySelector(".entry-repetition")?.TextContent.Trim()));
        Assert.All(entries, e => Assert.Equal("opções", e.QuerySelector("button.icon-button")!.GetAttribute("aria-label")));
        Assert.Equal([Kind.CreditCard, Kind.Expense, Kind.CreditCard], drawer.FindComponents<KindIcon>().Select(i => i.Instance.Kind));
        Assert.Contains("var(--kind-credit-card)", entries[0].QuerySelector(".kind-icon")!.GetAttribute("style"));
    }

    // DAY-05: < and > show the previous or next day with the same filter.
    [Fact]
    public void DrawerArrowsStepDayKeepingFilter()
    {
        filter = Kind.CreditCard;
        var drawer = RenderDrawer();
        Assert.Equal(["streaming", "geladeira"], Descriptions(drawer));

        drawer.Find("button[aria-label='dia seguinte']").Click();
        Rebind(drawer);

        Assert.Equal(Eleventh, day);
        Assert.Equal("11/nov", drawer.Find(".drawer-title").TextContent.Trim());
        Assert.Equal(Kind.CreditCard, filter);
        Assert.Equal("cartão", FilterPill(drawer));
        Assert.Equal(["mercado"], Descriptions(drawer));

        drawer.Find("button[aria-label='dia anterior']").Click();
        Rebind(drawer);
        drawer.Find("button[aria-label='dia anterior']").Click();
        Rebind(drawer);

        Assert.Equal(new DateOnly(2026, 11, 9), day);
        Assert.Equal("9/nov", drawer.Find(".drawer-title").TextContent.Trim());
        Assert.Equal("cartão", FilterPill(drawer));
    }

    // DAY-06: the filter pill lists todos to cartão, the current one checked.
    [Fact]
    public void DrawerFilterListsKindsWithCurrentChecked()
    {
        filter = Kind.Savings;
        var drawer = RenderDrawer();
        Assert.Equal("economias", FilterPill(drawer));
        Assert.Empty(drawer.FindAll(".dropdown"));

        drawer.Find(".filter-pill").Click();

        var options = drawer.FindAll(".dropdown-option");
        Assert.Equal(
            ["todos", "entradas", "saídas", "diários", "economias", "cartão"],
            options.Select(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim()));
        Assert.Equal("economias", Assert.Single(options, o => o.GetAttribute("aria-pressed") == "true").QuerySelector(".dropdown-label")!.TextContent.Trim());
    }

    // DAY-07: a filter lists only its kind; todos lists all. Choosing one closes the list.
    [Fact]
    public void DrawerFilterShowsOnlyThatKind()
    {
        var drawer = RenderDrawer();
        Assert.Equal("todos", FilterPill(drawer));
        Assert.Equal(["streaming", "padaria", "geladeira"], Descriptions(drawer));

        drawer.Find(".filter-pill").Click();
        drawer.FindAll(".dropdown-option")[2].Click();
        Rebind(drawer);

        Assert.Equal(Kind.Expense, filter);
        Assert.Equal("saídas", FilterPill(drawer));
        Assert.Equal(["padaria"], Descriptions(drawer));
        Assert.Empty(drawer.FindAll(".dropdown"));

        drawer.Find(".filter-pill").Click();
        drawer.FindAll(".dropdown-option")[0].Click();
        Rebind(drawer);

        Assert.Null(filter);
        Assert.Equal(["streaming", "padaria", "geladeira"], Descriptions(drawer));
    }

    // DAY-08: a day with no matching item shows nenhum lançamento.
    [Fact]
    public void EmptyDayShowsNoEntries()
    {
        filter = Kind.Savings;
        var drawer = RenderDrawer();

        Assert.Empty(drawer.FindAll(".entry"));
        Assert.Equal("nenhum lançamento", drawer.Find(".entries-empty").TextContent.Trim());

        filter = null;
        Rebind(drawer);
        Assert.Empty(drawer.FindAll(".entries-empty"));
        Assert.Equal(3, drawer.FindAll(".entry").Count);
    }

    // DAY-09: X and Esc close the drawer.
    [Fact]
    public void DrawerClosesOnXAndEsc()
    {
        var drawer = RenderDrawer();

        drawer.Find("button[aria-label='fechar']").Click();
        Assert.Equal(1, closes);

        drawer.Find(".drawer").KeyDown(key: "Escape");
        Assert.Equal(2, closes);
    }

    // DAY-11: ⋮ opens a menu with excluir; choosing it raises the entry for removal.
    [Fact]
    public void ItemMenuOffersDelete()
    {
        var drawer = RenderDrawer();
        Assert.Empty(drawer.FindAll(".dropdown"));

        drawer.FindAll(".entry")[1].QuerySelector("button[aria-label='opções']")!.Click();

        var menu = drawer.Find(".entry .dropdown");
        Assert.Equal("padaria", menu.Closest(".entry")!.QuerySelector(".entry-description")!.TextContent.Trim());
        var option = Assert.Single(drawer.FindAll(".dropdown-option"));
        Assert.Equal("excluir", option.TextContent.Trim());

        option.Click();
        Assert.Equal([Bakery], removed);
        Assert.Empty(drawer.FindAll(".dropdown"));
    }

    // Serves the day's entries in the order given, filtered by kind as ILedgerStore.Day promises.
    private sealed class DayStore(IReadOnlyList<DayEntry> entries) : ILedgerStore
    {
        public IReadOnlyList<Card> Cards => [];

        public IReadOnlyList<string> Tags => [];

        public IReadOnlyList<MonthLedger> Months(Period period) => [];

        public IReadOnlyList<DayEntry> Day(DateOnly day, Kind? filter) =>
            entries.Where(e => e.Date == day && (filter is null || e.Kind == filter)).ToList();

        public DateOnly DueDate(Card card, DateOnly onOrAfter, int monthsAhead) => onOrAfter;

        public void Add(NewItem item)
        {
        }

        public void Remove(Guid itemId, DateOnly day)
        {
        }
    }
}