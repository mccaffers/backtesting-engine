using System.Net;
using System.Text;
using System.Text.Json;

namespace Utilities;

public sealed class FxMacroDataClient
{
    private static readonly Uri DefaultBaseUri = new("https://fxmacrodata.com/api/v1/");

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly Uri _baseUri;

    public FxMacroDataClient(string apiKey, HttpClient? httpClient = null, Uri? baseUri = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("FXMacroData API key is required.", nameof(apiKey));
        }

        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient();
        _baseUri = NormalizeBaseUri(baseUri ?? DefaultBaseUri);
    }

    public static FxMacroDataClient FromEnvironment(HttpClient? httpClient = null, Uri? baseUri = null)
    {
        var apiKey = Environment.GetEnvironmentVariable("FXMACRODATA_API_KEY")
            ?? Environment.GetEnvironmentVariable("FXMD_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Set FXMACRODATA_API_KEY or FXMD_API_KEY.");
        }

        return new FxMacroDataClient(apiKey, httpClient, baseUri);
    }

    public Task<JsonElement> GetAsync(
        string path,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync(path, query, cancellationToken);

    public Task<JsonElement> DataCatalogueAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"data_catalogue/{currency}", query, cancellationToken);

    public Task<JsonElement> AnnouncementsAsync(
        string currency,
        string indicator,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"announcements/{currency}/{indicator}", query, cancellationToken);

    public Task<JsonElement> LatestAnnouncementsAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"announcements/{currency}/latest", query, cancellationToken);

    public Task<JsonElement> AnnouncementChangesAsync(
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync("announcements/changes", query, cancellationToken);

    public Task<JsonElement> CalendarAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"calendar/{currency}", query, cancellationToken);

    public Task<JsonElement> PredictionsAsync(
        string currency,
        string indicator,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"predictions/{currency}/{indicator}", query, cancellationToken);

    public Task<JsonElement> ForexAsync(
        string baseCurrency,
        string quoteCurrency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"forex/{baseCurrency}/{quoteCurrency}", query, cancellationToken);

    public Task<JsonElement> CotAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"cot/{currency}", query, cancellationToken);

    public Task<JsonElement> CommodityAsync(
        string indicator,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"commodities/{indicator}", query, cancellationToken);

    public Task<JsonElement> CommoditiesLatestAsync(
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync("commodities/latest", query, cancellationToken);

    public Task<JsonElement> CurvesAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"curves/{currency}", query, cancellationToken);

    public Task<JsonElement> CurveProxiesAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"curve_proxies/{currency}", query, cancellationToken);

    public Task<JsonElement> ForwardCurvesAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"forward_curves/{currency}", query, cancellationToken);

    public Task<JsonElement> RateDifferentialsAsync(
        string baseCurrency,
        string quoteCurrency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"rate_differentials/{baseCurrency}/{quoteCurrency}", query, cancellationToken);

    public Task<JsonElement> ForwardDifferentialsAsync(
        string baseCurrency,
        string quoteCurrency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"forward_differentials/{baseCurrency}/{quoteCurrency}", query, cancellationToken);

    public Task<JsonElement> MarketSessionsAsync(
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync("market_sessions", query, cancellationToken);

    public Task<JsonElement> RiskSentimentAsync(
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync("risk_sentiment", query, cancellationToken);

    public Task<JsonElement> NewsAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"news/{currency}", query, cancellationToken);

    public Task<JsonElement> PressReleasesAsync(
        string currency,
        IReadOnlyDictionary<string, string?>? query = null,
        CancellationToken cancellationToken = default) =>
        SendGetAsync($"press-releases/{currency}", query, cancellationToken);

    public async Task<JsonElement> GraphQlAsync(
        string query,
        JsonElement? variables = null,
        CancellationToken cancellationToken = default)
    {
        var body = JsonSerializer.Serialize(new
        {
            query,
            variables
        });

        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await _httpClient
            .PostAsync(BuildUri("graphql"), content, cancellationToken)
            .ConfigureAwait(false);

        return await ParseJsonAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public Uri BuildUri(string path, IReadOnlyDictionary<string, string?>? query = null)
    {
        var relativePath = path.TrimStart('/');
        var uri = new Uri(_baseUri, relativePath);

        var parameters = new List<KeyValuePair<string, string?>>
        {
            new("api_key", _apiKey)
        };

        if (query is not null)
        {
            parameters.InsertRange(0, query);
        }

        var queryString = string.Join(
            "&",
            parameters
                .Where(pair => pair.Value is not null)
                .Select(pair => $"{WebUtility.UrlEncode(pair.Key)}={WebUtility.UrlEncode(pair.Value)}"));

        return new UriBuilder(uri)
        {
            Query = queryString
        }.Uri;
    }

    private async Task<JsonElement> SendGetAsync(
        string path,
        IReadOnlyDictionary<string, string?>? query,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient
            .GetAsync(BuildUri(path, query), cancellationToken)
            .ConfigureAwait(false);

        return await ParseJsonAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<JsonElement> ParseJsonAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"FXMacroData HTTP {(int)response.StatusCode}: {body}",
                null,
                response.StatusCode);
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    private static Uri NormalizeBaseUri(Uri baseUri)
    {
        var value = baseUri.ToString();
        return value.EndsWith("/", StringComparison.Ordinal)
            ? baseUri
            : new Uri(value + "/");
    }
}
