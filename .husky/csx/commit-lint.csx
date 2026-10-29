// Checks the commit message header against Conventional Commits 1.0 (CONV-02..05).
using System.Text.RegularExpressions;

var maxHeaderLength = 100;
var lines = File.ReadAllLines(Args[0]);
var header = lines.Length > 0 ? lines[0] : "";

// Messages git writes for merges and reverts pass as they are.
if (header.StartsWith("Merge ") || header.StartsWith("Revert \""))
{
    return 0;
}

var conventional = new Regex(@"^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\([a-z0-9-]+\))?!?: \S.*$");
if (header.Length <= maxHeaderLength && conventional.IsMatch(header))
{
    return 0;
}

Console.WriteLine($"Invalid commit message header: \"{header}\"");
if (header.Length > maxHeaderLength)
{
    Console.WriteLine($"The header has {header.Length} characters; the limit is {maxHeaderLength}.");
}

Console.WriteLine("Expected format: type(scope)!: subject");
Console.WriteLine("  type:    feat fix docs style refactor perf test build ci chore revert");
Console.WriteLine("  scope:   optional, lowercase letters, digits and dashes");
Console.WriteLine("  !:       optional, marks a breaking change");
Console.WriteLine($"  header:  at most {maxHeaderLength} characters");
Console.WriteLine("Example: feat(ledger): add account import");
return 1;
