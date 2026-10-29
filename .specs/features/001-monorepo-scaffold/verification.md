# Monorepo Scaffold verification

**Verdict**: FAIL
**Mutations**: 3 - probed C21, C11, C13
**Proofs**: `proofs.md` at 6a2829e (round 1); 8f52751 (round 2: 40 pass, mutations 3 killed)
**Verifier**: independent sub-agent (author != verifier)

## Findings

| Finding | Evidence | Fix | Closed |
| --- | --- | --- | --- |
| Dropping `perf` from the allowed types fails no test | mutation `commit-msg: type perf dropped from the allowed types` not killed; `ConventionalHeaderIsAccepted` only tries `chore`, `feat`, `fix` | accept a header of every type CONV-02 lists | 40f80c0 - C21 accepts a header of every type; mutation killed |
| ARCH-03 is proven for Domain only; dropping Application from the rule fails no test | mutation `ARCH-03: Application layer no longer checked` not killed; `Fixture.Ledger.Application` uses no framework | give the Application fixture a framework dependency and assert its violation | 40f80c0 - C11 asserts `LedgerService` -> `DbContext`; mutation killed |
| SharedKernel -> AppHost is not proven | mutation `ARCH-05: SharedKernel -> AppHost no longer reported` not killed; the fixtures have no AppHost assembly | add a `Fixture.AppHost` that `Money` depends on and assert 3 violations | 40f80c0 - C13 asserts Money -> `Fixture.AppHost.Orchestrator`; mutation killed |
| A module with a dotted name escapes every architecture rule | generated module `A.B`, Domain referencing `Gibeira.A.B.Infrastructure`: 14/14 pass; `ModuleAssembly.Parse` accepts a one-segment module name only, so `A.B` assemblies belong to no module | parse multi-segment module names, or reject them, and prove it | 40f80c0 - rejected per user ("Reject them (Recommended)"): ARCH-10, C36 |
| The pre-commit hook rejects a formatted staged file when its unstaged copy is unformatted | staged formatted + working copy unformatted: hook reports `Messy.cs`, `Clean.cs` and rejects (seen twice); `dotnet format --include ${staged}` reads the working tree, against "SHALL check only staged `.cs` files" | check the staged content, not the working copy | 8f52751 - `.husky/format-staged.sh` checks the staged blob; C37 both directions |
