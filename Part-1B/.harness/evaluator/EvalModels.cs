namespace Evaluator;

public record EvaluationCase(
    string Id,
    string Category,
    string Input,
    ExpectedOutcome Expected);

public record ExpectedOutcome(
    int? ExpectedExpenseCount,
    List<ExpectedExpense>? ExpectedExpenses,
    bool? ExpectClarification,
    List<string>? InvariantChecks);

public record ExpectedExpense(int? Amount, string? Category);

public record EvaluationResult(
    string CaseId,
    bool StructuralPass,
    bool InvariantPass,
    double SemanticScore,
    bool OverallPass,
    long LatencyMs,
    string RawModelOutput,
    string? FailureReason);
