using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitLead.Application.Media.Uploadcare;
using Microsoft.Extensions.Options;

namespace FitLead.Infrastructure.Media.Uploadcare
{
    public sealed class UploadcareClient : IUploadcareClient
    {
        private const string ApiVersion = "application/vnd.uploadcare-v0.7+json";
        private const string JsonContentType = "application/json";

        private readonly HttpClient _httpClient;
        private readonly UploadcareOptions _options;

        public UploadcareClient(
            HttpClient httpClient,
            IOptions<UploadcareOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<UploadcareFileInfo?> GetFileInfoAsync(
            string uuid,
            CancellationToken cancellationToken)
        {
            EnsureConfigured();

            var normalizedUuid = uuid.Trim();
            var requestUri = $"/files/{normalizedUuid}/";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            AddSignedHeaders(request, requestUri);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var file = await JsonSerializer.DeserializeAsync<UploadcareFileResponse>(
                contentStream,
                cancellationToken: cancellationToken);

            if (file is null)
            {
                throw new InvalidOperationException("Uploadcare returned an empty file response.");
            }

            return new UploadcareFileInfo(
                file.Uuid,
                file.OriginalFileUrl,
                file.OriginalFilename,
                file.MimeType,
                file.Size,
                file.ContentInfo?.Video?.Duration);
        }

        private void AddSignedHeaders(
            HttpRequestMessage request,
            string requestUri)
        {
            var now = DateTimeOffset.UtcNow;
            var date = now.ToString("r", CultureInfo.InvariantCulture);
            var contentMd5 = Convert.ToHexString(MD5.HashData(Array.Empty<byte>())).ToLowerInvariant();
            var signString = string.Join(
                '\n',
                request.Method.Method,
                contentMd5,
                JsonContentType,
                date,
                requestUri);
            var signature = CreateSignature(signString);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(ApiVersion));
            request.Headers.TryAddWithoutValidation("Date", date);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Uploadcare",
                $"{_options.PublicKey}:{signature}");
            request.Content = new ByteArrayContent(Array.Empty<byte>());
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(JsonContentType);
        }

        private string CreateSignature(string signString)
        {
            using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(_options.SecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signString));

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private void EnsureConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.PublicKey) ||
                string.IsNullOrWhiteSpace(_options.SecretKey))
            {
                throw new InvalidOperationException("Uploadcare configuration is missing.");
            }
        }

        private sealed class UploadcareFileResponse
        {
            [JsonPropertyName("uuid")]
            public string Uuid { get; init; } = string.Empty;

            [JsonPropertyName("original_file_url")]
            public string OriginalFileUrl { get; init; } = string.Empty;

            [JsonPropertyName("original_filename")]
            public string? OriginalFilename { get; init; }

            [JsonPropertyName("mime_type")]
            public string MimeType { get; init; } = string.Empty;

            [JsonPropertyName("size")]
            public long Size { get; init; }

            [JsonPropertyName("content_info")]
            public UploadcareContentInfo? ContentInfo { get; init; }
        }

        private sealed class UploadcareContentInfo
        {
            [JsonPropertyName("video")]
            public UploadcareVideoInfo? Video { get; init; }
        }

        private sealed class UploadcareVideoInfo
        {
            [JsonPropertyName("duration")]
            public int? Duration { get; init; }
        }
    }
}
