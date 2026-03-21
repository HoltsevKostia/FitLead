using System.Net.Http.Json;
using System.Text.Json;

namespace FitLead.IntegrationTests.Helpers;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> ReadRequiredJsonAsync<T>(this HttpResponseMessage response)
    {
        var payload = await response.Content.ReadFromJsonAsync<T>();
        return payload ?? throw new InvalidOperationException(
            $"Response payload could not be deserialized into {typeof(T).Name}.");
    }

    public static async Task<string?> ReadErrorCodeAsync(this HttpResponseMessage response)
    {
        if (response.Content is null)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        if (!json.RootElement.TryGetProperty("errorCode", out var errorCode))
            return null;

        return errorCode.GetString();
    }

    public static async Task<ApiProblemDetails> ReadProblemDetailsAsync(this HttpResponseMessage response)
    {
        if (response.Content is null)
            return new ApiProblemDetails(null, null, null);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        var title = json.RootElement.TryGetProperty("title", out var titleElement)
            ? titleElement.GetString()
            : null;

        var detail = json.RootElement.TryGetProperty("detail", out var detailElement)
            ? detailElement.GetString()
            : null;

        var errorCode = json.RootElement.TryGetProperty("errorCode", out var errorCodeElement)
            ? errorCodeElement.GetString()
            : null;

        return new ApiProblemDetails(title, detail, errorCode);
    }
}
