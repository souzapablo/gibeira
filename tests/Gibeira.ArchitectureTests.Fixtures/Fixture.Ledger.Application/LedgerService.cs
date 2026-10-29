using Fixture.Ledger.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace Fixture.Ledger.Application;

public sealed class LedgerService
{
    // ARCH-02: Application depends on its module's Infrastructure.
    public LedgerRepository? Repository { get; set; }

    // ARCH-03: Application depends on EF Core.
    public DbContext? Database { get; set; }
}