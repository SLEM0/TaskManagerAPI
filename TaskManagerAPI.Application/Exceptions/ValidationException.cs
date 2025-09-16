namespace TaskManagerAPI.Application.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public IDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}