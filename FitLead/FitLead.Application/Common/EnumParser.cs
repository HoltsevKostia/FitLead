using FitLead.Common.Errors;
using FitLead.Common.Results;

namespace FitLead.Application.Common
{
    public static class EnumParser
    {
        public static Result<TEnum> ParseDefined<TEnum>(
            string? value,
            string requiredCode,
            string requiredMessage,
            string invalidCode,
            string invalidMessage)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<TEnum>.Failure(
                    Error.Validation(requiredCode, requiredMessage));
            }

            if (!Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var parsedValue) ||
                !Enum.IsDefined(parsedValue))
            {
                return Result<TEnum>.Failure(
                    Error.Validation(invalidCode, invalidMessage));
            }

            return Result<TEnum>.Success(parsedValue);
        }
    }
}
