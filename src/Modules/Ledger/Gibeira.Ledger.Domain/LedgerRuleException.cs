namespace Gibeira.Ledger.Domain;

// A value the ledger's rules refuse. The form prevents every refusal, so one reaching here is a bug.
public sealed class LedgerRuleException(string message) : Exception(message);