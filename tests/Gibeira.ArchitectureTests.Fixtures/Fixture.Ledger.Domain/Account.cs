using Fixture.Budgets.Domain;
using Fixture.Ledger.Application;
using Fixture.Ledger.Infrastructure;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fixture.Ledger.Domain;

public sealed class Account
{
    // ARCH-01: Domain depends on its module's Application and Infrastructure.
    public LedgerService? Service { get; set; }

    public LedgerRepository? Repository { get; set; }

    // ARCH-03: Domain depends on ASP.NET Core and EF Core.
    public HttpContext? Request { get; set; }

    public DbContext? Database { get; set; }

    // ARCH-04: the Ledger module depends on the Budgets module.
    public Budget? Budget { get; set; }
}