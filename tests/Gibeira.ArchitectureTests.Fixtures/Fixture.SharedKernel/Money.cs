using Fixture.Budgets.Domain;
using Fixture.Web;

namespace Fixture.SharedKernel;

public sealed class Money
{
    // ARCH-05: the shared kernel depends on a module and on the Web host.
    public Budget? Budget { get; set; }

    // ARCH-06: an assembly other than the AppHost depends on the Web host.
    public HomePage? Page { get; set; }
}