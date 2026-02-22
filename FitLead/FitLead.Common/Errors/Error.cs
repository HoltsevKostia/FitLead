namespace FitLead.Common.Errors
{
    public sealed record Error(
        string Code,
        string Message,
        ErrorType Type,
        IReadOnlyDictionary<string, object?>? Metadata = null)
    {
        public static readonly Error None =
            new("None", string.Empty, ErrorType.Failure);

        public static readonly Error NullValue =
            new("Null",
                "Null value cant be provided",
                ErrorType.Failure);

        public static Error Validation(string code, string message, IReadOnlyDictionary<string, object?>? metadata = null)
            => new(code, message, ErrorType.Validation, metadata);

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
