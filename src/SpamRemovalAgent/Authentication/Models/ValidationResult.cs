namespace SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public sealed record ValidationResult
{
    /// <summary>
    /// Indicates whether validation passed.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// List of validation error messages (empty if IsSuccess = true).
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new ValidationResult { IsSuccess = true };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ValidationResult Failure(IEnumerable<string> errors) => new ValidationResult
    {
        IsSuccess = false,
        Errors = errors.ToList()
    };
}
