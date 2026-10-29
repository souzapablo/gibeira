using Fixture.Ledger.Infrastructure;

namespace Fixture.Ledger.Application;

// ARCH-02: Application depends on its module's Infrastructure.
public sealed class LedgerService
{
    public LedgerRepository? Repository { get; set; }
}