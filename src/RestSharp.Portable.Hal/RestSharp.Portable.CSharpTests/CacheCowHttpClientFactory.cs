using System.Net.Http;
using CacheCow.Client;
using CacheCow.Common;
using RestSharp.Portable;
using RestSharp.Portable.HttpClientImpl;

public class CacheCowHttpClientFactory : DefaultHttpClientFactory
{
    private readonly CachingHandler _cachingHandler;

    public CacheCowHttpClientFactory(ICacheStore cacheStore)
        : this(new CachingHandler(cacheStore))
    {
    }

    public CacheCowHttpClientFactory()
        : this(new CachingHandler())
    {
    }

    public CacheCowHttpClientFactory(CachingHandler cachingHandler)
    {
        _cachingHandler = cachingHandler;
        _cachingHandler.DefaultVaryHeaders = new[] {"Accept-Encoding"};
    }

    public override HttpClient CreateClient(IRestClient client, IRestRequest request)
    {
        var handler = CreateMessageHandler(client, request);
        var cacheCowhandler = _cachingHandler;
        var httpClient = HttpClientFactory.Create(handler, cacheCowhandler);
        httpClient.BaseAddress = GetBaseAddress(client);

        return httpClient;
    }

}