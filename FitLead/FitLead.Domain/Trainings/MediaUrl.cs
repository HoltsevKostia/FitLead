using FitLead.Common.Domain;
using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Domain.Trainings
{
    public sealed class MediaUrl : ValueObject
    {
        public const int MaxLength = 2048;

        public string Value { get; }

        private MediaUrl(string value)
        {
            Value = value;
        }

        public static Result<MediaUrl> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<MediaUrl>.Failure(
                    Error.Validation("media_url.required", "MediaUrl is required when provided"));
            }

            var trimmed = value.Trim();

            if (trimmed.Length > MaxLength)
            {
                return Result<MediaUrl>.Failure(
                    Error.Validation("media_url.too_long", "MediaUrl is too long"));
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                return Result<MediaUrl>.Failure(
                    Error.Validation("media_url.invalid", "MediaUrl must be an absolute URL"));
            }

            if (uri.Scheme is not ("http" or "https"))
            {
                return Result<MediaUrl>.Failure(
                    Error.Validation("media_url.invalid_scheme", "MediaUrl must use http or https"));
            }

            return Result<MediaUrl>.Success(new MediaUrl(trimmed));
        }

        public override string ToString()
            => Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
