using FluentValidation.Results;

namespace Common.Validation;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string message) : base(message)
    {
        Errors = Enumerable.Empty<ValidationFailure>();
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = Enumerable.Empty<ValidationFailure>();
    }

    /// <summary>
    /// Gets validation errors as a dictionary
    /// </summary>
    public IDictionary<string, string[]> GetErrorsDictionary()
    {
        return Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );
    }
}
