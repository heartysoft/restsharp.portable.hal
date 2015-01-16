using System.IO;
using System.Net.Http;
using CacheCow.Client;
using CacheCow.Client.FileCacheStore;
using NUnit.Framework;
using RestSharp.Portable.Hal.CSharp;

namespace RestSharp.Portable.CSharpTests
{
    [TestFixture]
    public class CacheCowIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
            if (File.Exists(TestConfig.CacheFile))
                File.Delete(TestConfig.CacheFile);
        }

        private HalClient createClient(HalClientFactory clientFactory)
        {
            return clientFactory.CreateHalClient(TestConfig.RootUrl);
        }

        [Test]
        public void response_is_cached_when_using_cacheCow()
        {
            var clientFactory = new HalClientFactory().HttpClientFactory(new CacheCowHttpClientFactory())
                .Accept("application/hal+json");

            _client = createClient(clientFactory);

            var response = _client.From("api/cardholders").GetAsync().Result;
            var response2 = _client.From("api/cardholders").GetAsync().Result;
            var response3 = _client.From("api/cardholders").Follow("register").GetAsync().Result;
            var response4 = _client.From("api/cardholders").Follow("register").GetAsync().Result;

            Assert.IsNotNull(response);
            Assert.IsNotNull(response2);
            Assert.IsNotNull(response3);
            Assert.IsNotNull(response4);
        }

        private HalClient _client;

        [Test]
        public void response_is_cached_when_using_cacheCow_with_filestore()
        {
            var clientFactory = new HalClientFactory().HttpClientFactory(
                new CacheCowHttpClientFactory(new FileStore(TestConfig.CacheFile)))
                .Accept("application/hal+json");

            _client = createClient(clientFactory);

            var response = _client.From("api/cardholders").GetAsync().Result;
            var response2 = _client.From("api/cardholders").GetAsync().Result;
            var response3 = _client.From("api/cardholders").Follow("register").GetAsync().Result;
            var response4 = _client.From("api/cardholders").Follow("register").GetAsync().Result;

            Assert.IsNotNull(response);
            Assert.IsNotNull(response2);
            Assert.IsNotNull(response3);
            Assert.IsNotNull(response4);
        }

        [Test]
        public void cache_cow_will_cache()
        {
            var url = string.Format("{0}api/cardholders", TestConfig.RootUrl);

            var cacheCowhandler = new CachingHandler(new FileStore(TestConfig.CacheFile));
            var handler = new HttpClientHandler();
            var httpClient = HttpClientFactory.Create(handler, cacheCowhandler);

            var msg = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "application/hal+json");
            var response = httpClient.SendAsync(msg).Result;

            var msg1 = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "application/hal+json");
            var response1 = httpClient.SendAsync(msg1).Result;

            var handler2 = new HttpClientHandler();
            var cacheCowhandler2 = new CachingHandler(new FileStore(TestConfig.CacheFile));
            var httpClient2 = HttpClientFactory.Create(handler2, cacheCowhandler2);

            var msg2 = new HttpRequestMessage(HttpMethod.Get, url);
            msg2.Headers.Add("Accept", "application/hal+json");
            var response2 = httpClient2.SendAsync(msg2).Result;

            var msg3 = new HttpRequestMessage(HttpMethod.Get, url);
            msg3.Headers.Add("Accept", "application/hal+json");
            var response3 = httpClient2.SendAsync(msg3).Result;

            Assert.IsNotNull(response);
            Assert.IsNotNull(response1);
            Assert.IsNotNull(response2);
            Assert.IsNotNull(response3);
        }
    }
}