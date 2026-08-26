using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Xunit;

namespace tests.backtesting.UtilitesTests;

public class FxMacroDataClientTests
{
    [Fact]
    public async Task ForexAsyncAddsApiKeyAndQueryParameters()
    {
        var handler = new CaptureHandler();
        var client = new FxMacroDataClient(
            "test-key",
            new HttpClient(handler),
            new Uri("https://example.com/api/v1/"));

        var result = await client.ForexAsync(
            "eur",
            "usd",
            new Dictionary<string, string?>
            {
                ["limit"] = "1"
            });

        Assert.True(result.GetProperty("ok").GetBoolean());
        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://example.com/api/v1/forex/eur/usd?limit=1&api_key=test-key",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public void BuildUriPreservesFullEndpointSurface()
    {
        var client = new FxMacroDataClient(
            "test-key",
            new HttpClient(new CaptureHandler()),
            new Uri("https://example.com/api/v1/"));

        var paths = new[]
        {
            "data_catalogue/usd",
            "announcements/usd/non_farm_payrolls",
            "announcements/usd/latest",
            "announcements/changes",
            "calendar/usd",
            "predictions/usd/non_farm_payrolls",
            "forex/eur/usd",
            "cot/usd",
            "commodities/brent",
            "commodities/latest",
            "curves/usd",
            "curve_proxies/usd",
            "forward_curves/usd",
            "rate_differentials/eur/usd",
            "forward_differentials/eur/usd",
            "market_sessions",
            "risk_sentiment",
            "news/usd",
            "press-releases/usd",
            "graphql"
        };

        foreach (var path in paths)
        {
            var uri = client.BuildUri(path);

            Assert.StartsWith("https://example.com/api/v1/", uri.ToString());
            Assert.Contains("api_key=test-key", uri.Query);
        }
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}")
            });
        }
    }
}
