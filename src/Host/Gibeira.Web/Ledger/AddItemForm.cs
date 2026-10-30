using System.Globalization;

namespace Gibeira.Web.Ledger;

// The add modal's state and transitions, kept out of Razor so they are unit-tested.
public sealed class AddItemForm(Kind kind, DateOnly date)
{
    private readonly List<string> tags = [];
    private bool onDueDate;
    private bool submitted;

    public Kind Kind { get; private set; } = kind;

    public string AmountText { get; private set; } = "";

    public string Description { get; set; } = "";

    public string DescriptionPlaceholder => PtBr.KindName(Kind);

    public DateOnly Date { get; set; } = date;

    public string DateLabel => onDueDate ? "vencimento" : "data";

    public Card? Card { get; private set; }

    public Repetition Repetition { get; set; } = new Repetition.None();

    public string RepetitionLabel => Repetition switch
    {
        Repetition.Monthly => "mensalmente",
        Repetition.Weekly => "semanalmente",
        Repetition.Daily => "diariamente",
        Repetition.Installments(var count) => string.Create(CultureInfo.InvariantCulture, $"parcelado em {count}x"),
        _ => "não repete",
    };

    // SPEC_DEVIATION: a list where the design has IReadOnlySet<string>.
    // Reason: the row joins the tags in the order they were checked, and a set has no order.
    public IReadOnlyList<string> Tags => tags;

    public string TagsLabel => tags.Count == 0 ? "tags" : string.Join(", ", tags);

    public string ButtonLabel => "adicionar " + PtBr.KindName(Kind);

    public bool CanSubmit => !submitted && AmountInput.Parse(AmountText) > 0;

    public void SetKind(Kind value)
    {
        if (value != Kind.Cartao)
        {
            ChooseCard(null);
        }

        Kind = value;
    }

    public void Type(string proposed) => AmountText = AmountInput.Accept(AmountText, proposed);

    // Null is sem cartão. The row reads vencimento only once a due date is saved.
    public void ChooseCard(Card? card)
    {
        Card = card;
        onDueDate = false;
    }

    public void SetDueDate(DateOnly dueDate)
    {
        Date = dueDate;
        onDueDate = true;
    }

    public void ToggleTag(string tag)
    {
        if (!tags.Remove(tag))
        {
            tags.Add(tag);
        }
    }

    // The item on the first call that can submit; null on any other.
    public NewItem? Submit()
    {
        if (!CanSubmit)
        {
            return null;
        }

        submitted = true;
        var description = string.IsNullOrWhiteSpace(Description) ? PtBr.KindName(Kind) : Description;
        return new NewItem(Kind, AmountInput.Parse(AmountText), description, Date, Repetition, Card?.Name, [.. tags]);
    }
}