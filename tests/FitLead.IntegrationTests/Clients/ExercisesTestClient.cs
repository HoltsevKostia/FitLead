using System.Net;
using System.Net.Http.Json;
using FitLead.Api.Exercises.Contracts;
using FitLead.Domain.Trainings;
using FitLead.IntegrationTests.Helpers;

namespace FitLead.IntegrationTests.Clients;

public sealed class ExercisesTestClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly CookieContainer _cookieJar = new();
    private readonly Uri _baseUri = httpClient.BaseAddress ?? new Uri("http://localhost", UriKind.Absolute);

    public async Task<HttpResponseMessage> UpdateAsync(
        Guid exerciseId,
        string name = "Оновлена вправа",
        string description = "Оновлений опис",
        string? mediaUrl = null,
        MuscleGroup? muscleGroup = MuscleGroup.Core,
        Equipment? equipment = Equipment.Bodyweight,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Put, $"/api/exercises/{exerciseId:D}");
        request.Content = JsonContent.Create(new UpdateExerciseRequest(
            name,
            description,
            mediaUrl,
            muscleGroup,
            equipment));

        return await SendAsync(request, includeCsrfHeader: true, cancellationToken);
    }

    public async Task<HttpResponseMessage> DeleteAsync(
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Delete, $"/api/exercises/{exerciseId:D}");
        return await SendAsync(request, includeCsrfHeader: true, cancellationToken);
    }

    public async Task<HttpResponseMessage> ConfirmDeleteAsync(
        Guid exerciseId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Post, $"/api/exercises/{exerciseId:D}/deletion-confirmations");
        request.Content = JsonContent.Create(new ConfirmDeleteExerciseRequest(token));

        return await SendAsync(request, includeCsrfHeader: true, cancellationToken);
    }

    public async Task CopyAuthStateFromAsync(
        AuthTestClient authClient,
        CancellationToken cancellationToken = default)
    {
        var authStateResponse = await authClient.GetAsync("/auth/current-user", cancellationToken);
        authStateResponse.EnsureSuccessStatusCode();

        if (authStateResponse.RequestMessage?.Headers.TryGetValues("Cookie", out var cookieValues) != true)
        {
            throw new InvalidOperationException("Auth test client did not send cookies on authenticated request.");
        }

        foreach (var cookieHeader in cookieValues)
        {
            ApplyCookieHeader(cookieHeader);
        }

        await RefreshCsrfStateAsync(cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        bool includeCsrfHeader,
        CancellationToken cancellationToken)
    {
        if (includeCsrfHeader)
        {
            await EnsureCsrfTokenAsync(cancellationToken);
            request.Headers.Add(ApiCsrfTokenNames.RequestHeader, GetRequiredCookieValue(ApiCsrfTokenNames.RequestTokenCookie));
        }

        AddCookieHeader(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        CaptureCookies(response);
        return response;
    }

    private async Task EnsureCsrfTokenAsync(CancellationToken cancellationToken)
    {
        if (HasRequiredCsrfCookies())
            return;

        await RefreshCsrfStateAsync(cancellationToken);
    }

    private async Task RefreshCsrfStateAsync(CancellationToken cancellationToken)
    {
        var request = CreateRequest(HttpMethod.Get, "/auth/csrf-token");
        AddCookieHeader(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        CaptureCookies(response);
    }

    private void ApplyCookieHeader(string cookieHeader)
    {
        var parts = cookieHeader.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var nameValue = part.Split('=', 2);
            if (nameValue.Length == 2)
            {
                _cookieJar.Add(_baseUri, new Cookie(nameValue[0], nameValue[1], "/", _baseUri.Host));
            }
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
        => new(method, new Uri(_baseUri, path));

    private void AddCookieHeader(HttpRequestMessage request)
    {
        var cookies = _cookieJar.GetCookies(request.RequestUri!);
        if (cookies.Count == 0)
            return;

        var header = string.Join("; ",
            cookies.Cast<Cookie>()
                .Where(cookie => !string.IsNullOrWhiteSpace(cookie.Value))
                .Select(cookie => $"{cookie.Name}={cookie.Value}"));

        if (!string.IsNullOrWhiteSpace(header))
        {
            request.Headers.Add("Cookie", header);
        }
    }

    private void CaptureCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            return;

        foreach (var value in values)
        {
            _cookieJar.SetCookies(_baseUri, value);
        }
    }

    private string GetRequiredCookieValue(string cookieName)
    {
        var cookie = _cookieJar.GetCookies(_baseUri)[cookieName];
        if (cookie is null || string.IsNullOrWhiteSpace(cookie.Value))
        {
            throw new InvalidOperationException($"Cookie '{cookieName}' is missing from the exercises test cookie jar.");
        }

        return cookie.Value;
    }

    private bool HasCookie(string cookieName)
    {
        var cookie = _cookieJar.GetCookies(_baseUri)[cookieName];
        return cookie is not null && !string.IsNullOrWhiteSpace(cookie.Value);
    }

    private bool HasRequiredCsrfCookies()
        => HasCookie(ApiCsrfTokenNames.RequestTokenCookie)
            && HasCookie(ApiCsrfTokenNames.AntiforgeryCookie);
}
