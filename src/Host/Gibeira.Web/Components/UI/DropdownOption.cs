using Microsoft.AspNetCore.Components;

namespace Gibeira.Web.Components.UI;

// One choice of a Dropdown: its value, pt-BR label, and an optional description line and leading icon.
public sealed record DropdownOption<T>(T Value, string Label, string? Description = null, RenderFragment? Icon = null);