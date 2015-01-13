using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using CacheCow.Client;
using CacheCow.Client.FileCacheStore;
using CacheCow.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp.Portable.Hal.CSharp;
using RestSharp.Portable.HttpClientImpl;

namespace RestSharp.Portable.CSharpTests
{
    [TestFixture]
    public class HalClientTests
    {
        private HalClient _client;

        [SetUp]
        public void SetUp()
        {
            var clientFactory = new HalClientFactory()
                 .Accept("application/hal+json");
            _client = clientFactory.CreateHalClient("http://c2383:62582/");
        }

        [Test]
        public void should_get_resource()
        {
            var resource = _client.From("api/cardholders").GetAsync().Result;
            Assert.IsInstanceOf<Resource>(resource);
        }

        [Test]
        public void should_get_resource_following_link()
        {
            var resource = _client.From("api/cardholders")
                .Follow("register")
                .GetAsync().Result;

            var tjo = resource.Parse<RegistrationForm>();

            Assert.AreEqual(-1, tjo.Id);
        }

        [Test]
        public void should_get_resource_following_multiple_links()
        {
            var resource = _client.From("api/cardholders")
                .Follow("register")
                .Follow("self")
                .GetAsync().Result;

            var tjo = resource.Parse<RegistrationForm>();

            Assert.AreEqual(-1, tjo.Id);
        }

        [Test]
        public void should_follow_templated_link()
        {
            var resource = _client.From("api/cardholders")
                .Follow("cardholder", new { id = "123" })
                .GetAsync<CardHolderDetails>().Result;

            Assert.AreEqual(123, resource.Id);
            Assert.AreEqual("Customer Number123", resource.Name);
            Assert.AreEqual("again", resource.AnotherCard.IdAgain);
        }

        [Test]
        public void should_allow_url_segment_state()
        {
            var resource = _client.From("/api/cardholders")
                .UrlSegments(new { id = "123" })
                .Follow("cardholder")
                .GetAsync<CardHolderDetails>().Result;

            Assert.AreEqual(123, resource.Id);
            Assert.AreEqual("Customer Number123", resource.Name);
            Assert.AreEqual("again", resource.AnotherCard.IdAgain);
        }


        [Test]
        public void provided_url_segment_should_take_precedence()
        {
            var resource = _client.From("/api/cardholders")
                .UrlSegments(new { id = "123" })
                .Follow("cardholder", new{id="112"})
                .GetAsync<CardHolderDetails>().Result;

            Assert.AreEqual(112, resource.Id);
            Assert.AreEqual("Customer Number112", resource.Name);
            Assert.AreEqual("again", resource.AnotherCard.IdAgain);
        }

        [Test]
        public void should_follow_multiple_rels_in_one_go()
        {
            var resource = _client.From("/api/cardholders")
                .UrlSegments(new { id = "112" })
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                .Follow("cardholder", "card")
                .GetAsync<CardEmbedded>().Result;

            Assert.AreEqual("101", resource.Number);
            Assert.AreEqual("mastercard", resource.Type);
        }

        [Test]
        public void should_get_embedded_resource()
        {
            var resource = _client.From("/api/cardholders")
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                .Follow("cardholder", new{id=112})
                .Follow("card")
                .GetAsync<CardEmbedded>().Result;

            Assert.AreEqual("101", resource.Number);
            Assert.AreEqual("mastercard", resource.Type);
        }

        [Test]
        public void should_follow_link_in_embedded_resource()
        {
            var resource = _client.From("/api/cardholders")
                .Follow("cardholder", new { id = 112 })
                .Follow("card")
                .Follow("loadcard")
                .GetAsync<LoadCardForm>()
                .Result;

            Assert.AreEqual(100M, resource.Amount);
            Assert.AreEqual("GBP", resource.Currency);
        }

        [Test]
        public void should_post_form_to_server()
        {
            var newData = new RegistrationForm {Id = 55, Name = "Johny"};
            var resource =
                _client.From("/api/cardholders")
                    .Follow("register")
                    .PostAsync(newData)
                    .Result;

            Assert.AreEqual(HttpStatusCode.Created, resource.Response.StatusCode);

            var location = resource.Response.Headers.GetValues("Location").First();
            Assert.AreEqual("/api/cardholders/55", location);
        }

        [Test]
        public void should_post_form_to_server_and_parse_body_if_you_want()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var resource =
                _client.From("/api/cardholders")
                    .Follow("register")
                    .PostAsync(newData)
                    .Result;

            var body = resource.Parse<CardHolderDetails>();

            Assert.AreEqual("Johny", body.Name);
            Assert.AreEqual("lala", body.AnotherCard.IdAgain);
        }

        [Test]
        public void should_post_form_to_server_and_parse_body_nicely_if_you_want()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var resource =
                _client.From("/api/cardholders")
                    .Follow("register")
                    .PostAsyncAndParse<CardHolderDetails>(newData)
                    .Result;

            Assert.AreEqual("Johny", resource.Name);
            Assert.AreEqual("lala", resource.AnotherCard.IdAgain);
        }

        [Test]
        public void should_put_to_server()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var resource =
                _client.From("/api/cardholders")
                    .Follow("register")
                    .PutAsync(newData)
                    .Result;

            Assert.AreEqual(HttpStatusCode.Created, resource.Response.StatusCode);

            var location = resource.Response.Headers.GetValues("Location").First();
            Assert.AreEqual("/api/cardholders/55", location);
        }

        [Test]
        public void should_delete_to_server()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var resource =
                _client.From("/api/cardholders")
                    .Follow("register")
                    .DeleteAsync(newData)
                    .Result;

            Assert.AreEqual(HttpStatusCode.OK, resource.Response.StatusCode);

            var location = resource.Response.Headers.GetValues("Location").First();
            Assert.AreEqual("/api/cardholders", location);
        }

        [Test]
        public void should_follow_location_header()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var resource =
                _client.From("/api/cardholders")
                    .Follow("register")
                    .PostAsync(newData)
                    .Result;

            var newResource = resource.FollowHeader("Location").GetAsync<CardHolderDetails>().Result;

            Assert.AreEqual("Customer Number55", newResource.Name);
            Assert.AreEqual("again", newResource.AnotherCard.IdAgain);
        }

        [Test]
        public void should_follow_location_header_and_continue_traversal()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var resource =
                _client.From("/api/cardholders")
                    .Follow("register")
                    .PostAsync(newData)
                    .Result;

            var newResource = resource.FollowHeader("Location")
                .Follow("updatecardholder")
                .GetAsync<UpdateCardHolderForm>()
                .Result;

            Assert.AreEqual(0, newResource.Id);
        }

        [Test]
        public void can_pass_in_custom_httpClientFactory()
        {
            var clientFactory = new HalClientFactory().HttpClientFactory(new TestClientFactory())
                .Accept("application/hal+json");

            _client = clientFactory.CreateHalClient("http://localhost:62582/");

            Assert.IsNotNull(_client);
        }

        [Test]
        public void response_is_cached_when_using_cacheCow()
        {
            var clientFactory = new HalClientFactory().HttpClientFactory(new CacheCowHttpClientFactory())
                .Accept("application/hal+json");

            _client = clientFactory.CreateHalClient("http://c2383:62582/");

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
        public void response_is_cached_when_using_cacheCow_with_filestore()
        {
            var clientFactory = new HalClientFactory().HttpClientFactory(new CacheCowHttpClientFactory(new FileStore("c:\\Cache")))
                .Accept("application/hal+json");

            _client = clientFactory.CreateHalClient("http://c2383:62582/");

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
            var url = "http://c2383:62582/api/cardholders";

            var cacheCowhandler = new CachingHandler(new FileStore("c:\\Cache"));
            var handler = new HttpClientHandler();
            var httpClient = HttpClientFactory.Create(handler, cacheCowhandler);

            var msg = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "application/json");
            var response = httpClient.SendAsync(msg).Result;

            var msg1 = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("Accept", "application/json");
            var response1 = httpClient.SendAsync(msg1).Result;
           
            var handler2 = new HttpClientHandler();
            var cacheCowhandler2 = new CachingHandler(new FileStore("c:\\Cache"));
            var httpClient2 = HttpClientFactory.Create(handler2, cacheCowhandler2);

            var msg2 = new HttpRequestMessage(HttpMethod.Get, url);
            msg2.Headers.Add("Accept", "application/json");
            var response2 = httpClient2.SendAsync(msg2).Result;

            var msg3 = new HttpRequestMessage(HttpMethod.Get, url);
            msg3.Headers.Add("Accept", "application/json");
            var response3 = httpClient2.SendAsync(msg3).Result;

            Assert.IsNotNull(response);
            Assert.IsNotNull(response1);
            Assert.IsNotNull(response2);
            Assert.IsNotNull(response3);
        }
    }

    public class TestClientFactory : DefaultHttpClientFactory
    {
        
    }

    public class CacheCowHttpClientFactory : DefaultHttpClientFactory
    {
        private readonly CachingHandler _cachingHandler;

        public CacheCowHttpClientFactory(ICacheStore cacheStore) : this(new CachingHandler(cacheStore))
        {
        }

        public CacheCowHttpClientFactory() : this(new CachingHandler())
        {          
        }

        public CacheCowHttpClientFactory(CachingHandler cachingHandler)
        {
            _cachingHandler = cachingHandler;
            _cachingHandler.DefaultVaryHeaders = new[] { "Accept-Encoding" };           
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
}
