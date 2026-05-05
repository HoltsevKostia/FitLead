namespace FitLead.IntegrationTests.Helpers;

public static class SetCookieHeaderParser
{
    public static SetCookieHeader GetRequiredCookie(
        this HttpResponseMessage response,
        string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            throw new InvalidOperationException("Response does not contain Set-Cookie headers.");
        }

        var cookieHeader = values.FirstOrDefault(x => x.StartsWith($"{cookieName}=", StringComparison.Ordinal));
        if (cookieHeader is null)
        {
            throw new InvalidOperationException($"Response does not contain cookie '{cookieName}'.");
        }

        return Parse(cookieHeader);
    }

    private static SetCookieHeader Parse(string header)
    {
        var segments = header.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var nameValue = segments[0].Split('=', 2);
        var name = nameValue[0];
        var value = nameValue.Length > 1 ? nameValue[1] : string.Empty;
        var httpOnly = segments.Any(x => string.Equals(x, "HttpOnly", StringComparison.OrdinalIgnoreCase));
        var path = segments
            .Select(x => x.Split('=', 2))
            .FirstOrDefault(x => x.Length == 2 && string.Equals(x[0], "Path", StringComparison.OrdinalIgnoreCase))?[1];

        return new SetCookieHeader(name, value, httpOnly, path);
    }
}
