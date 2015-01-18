using System;
using System.IO;
using System.Net.Http;
using CacheCow.Client;
using CacheCow.Client.FileCacheStore;
using CacheCow.Common;
using Hal;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using RestSharp.Portable.Hal.CSharp;

namespace RestSharp.Portable.CSharpTests
{
    public class TestCacheCowHttpClientFactory :
        TestHttpClientFactory
    {
        public TestCacheCowHttpClientFactory(TestServer server, ICacheStore cacheStore = null) : base(server, getHandler(cacheStore))
        {
        }

        private static CachingHandler getHandler(ICacheStore cacheStore)
        {
            var handler = cacheStore == null? new CachingHandler() : new CachingHandler(cacheStore);
            handler.DefaultVaryHeaders = new[] {"Accept-Encoding"};
            return handler;
        }
    }

    [TestFixture]
    public class CacheCowIntegrationTests
    {
        private TestServer _server;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            _server = TestServer.Create<Startup>();
        }

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
            var clientFactory = new HalClientFactory().HttpClientFactory(new TestCacheCowHttpClientFactory(_server))
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
                new TestCacheCowHttpClientFactory(_server, new FileStore(TestConfig.CacheFile)))
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

        
        /// <summary>
        /// This is not a test of the HalClient, rather a reference to see how cache cow works.
        /// </summary>
        [Test]
        public void cache_cow_will_cache()
        {
            var url = string.Format("{0}api/cardholders", TestConfig.RootUrl);

            var cacheCowhandler = new CachingHandler(new FileStore(TestConfig.CacheFile));
            var handler = new HttpClientHandler();
            
            var httpClient = createClientRaw(_server, cacheCowhandler);

            var msg = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "application/hal+json");
            var response = httpClient.SendAsync(msg).Result;

            var msg1 = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "application/hal+json");
            var response1 = httpClient.SendAsync(msg1).Result;

            var cacheCowhandler2 = new CachingHandler(new FileStore(TestConfig.CacheFile));
            var httpClient2 = createClientRaw(_server, cacheCowhandler2);

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

        /// <summary>
        /// Only used in the cache cow reference test. Not part of hal client testing.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="delegatingHandlers"></param>
        /// <returns></returns>
        private static HttpClient createClientRaw(TestServer server, params DelegatingHandler[] delegatingHandlers)
        {
            var handler = HttpClientFactory.CreatePipeline(server.Handler, delegatingHandlers);
            return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        }

    }
}