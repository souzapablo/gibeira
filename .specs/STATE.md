# STATE

## Decisions

### AD-001
- **Decision**: Gibeira is a modular monolith: one Blazor host; each module has Domain, Application and Infrastructure projects named `Gibeira.<Module>.<Layer>`; no module references another module.
- **Reason**: User chose "Modular monolith + layers"; finance areas grow independently inside one local app.
- **Trade-off**: More projects per module; cross-module needs (a `Contracts` project) must be added deliberately.
- **Scope**: all of `src/`; enforced by `tests/Gibeira.ArchitectureTests`
- **Date**: 2026-10-29
- **Status**: active

### AD-002
- **Decision**: Every architecture rule is a function over a name prefix and is proven by a fixture assembly that violates it.
- **Reason**: With no modules yet, rules over the real solution pass vacuously.
- **Trade-off**: Fixture projects to maintain alongside each new rule.
- **Scope**: `tests/Gibeira.ArchitectureTests`, `tests/Gibeira.ArchitectureTests.Fixtures`
- **Date**: 2026-10-29
- **Status**: active

## Handoff
