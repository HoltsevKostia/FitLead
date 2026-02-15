namespace FitLead.Common.Errors
{
    public sealed record Error(
        string Code,
        string Message,
        ErrorType Type,
        IReadOnlyDictionary<string, object?>? Metadata = null)
    {
        public static Error Validation(string code, string message, IReadOnlyDictionary<string, object?>? metadata = null)
            => new(code, message, ErrorType.Validation, metadata);

        // probably redundant
        public static Error Unauthorized(string code, string message, IReadOnlyDictionary<string, object?>? metadata = null)
            => new(code, message, ErrorType.Unauthorized, metadata);

        public static Error Forbidden(string code, string message, IReadOnlyDictionary<string, object?>? metadata = null)
            => new(code, message, ErrorType.Forbidden, metadata);

        public static Error NotFound(string code, string message, IReadOnlyDictionary<string, object?>? metadata = null)
            => new(code, message, ErrorType.NotFound, metadata);

        public static Error Conflict(string code, string message, IReadOnlyDictionary<string, object?>? metadata = null)
            => new(code, message, ErrorType.Conflict, metadata);

        public static Error Failure(string code, string message, IReadOnlyDictionary<string, object?>? metadata = null)
            => new(code, message, ErrorType.Failure, metadata);
    }
}
