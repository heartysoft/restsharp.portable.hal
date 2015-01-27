using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Hal;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using RestSharp.Portable.CSharpTests;
using RestSharp.Portable.Hal.CSharp;
using RestSharp.Portable.HttpClientImpl;
using Simple.Data.Extensions;
using System.Threading.Tasks;

namespace RestSharp.Portable.CSharpTests
{
    public class TestHttpClientFactory : DefaultHttpClientFactory
    {
        private readonly TestServer _server;
        private readonly DelegatingHandler[] _delegatingHandlers;

        public TestHttpClientFactory(TestServer server, params DelegatingHandler[] delegatingHandlers)
        {
            _server = server;
            _delegatingHandlers = delegatingHandlers;
        }

        public override HttpClient CreateClient(IRestClient client, IRestRequest request)
        {
            var handler =  HttpClientFactory.CreatePipeline(_server.Handler, _delegatingHandlers);
            return new HttpClient(handler){BaseAddress = new Uri("http://localhost")};
        }

    }
    
    [TestFixture]
    public class HalClientCSharpTests
    {

        private HalClient _client;
        private TestServer _server;

        [TestFixtureSetUp]
        public void FixtueSetup()
        {
            _server = TestServer.Create<Startup>();
        }

        [SetUp]
        public void SetUp()
        {
            var clientFactory = new HalClientFactory()
                 .Accept("application/hal+json")
                 .HttpClientFactory(new TestHttpClientFactory(_server));
            _client = clientFactory.CreateHalClient("http://dummy-unsused");
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
        public void can_convert_url_segments_to_camel_case()
        {
            var query = new { Id = 55};

            var resource =
                _client.From("/api/cardholders")
                    .UrlSegments(query, true)
                    .Follow("cardholder").GetAsync<CardHolderDetails>()
                    .Result;

            Assert.AreEqual(55, resource.Id);
        }

        [Test]
        public void can_follow_with_object_to_camel_case()
        {
            var query = new { Id = 55 };

            var resource =
                _client.From("/api/cardholders")
                    .Follow("cardholder", query, true).GetAsync<CardHolderDetails>()
                    .Result;

            Assert.AreEqual(55, resource.Id);
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
        public void should_get_embedded_resource_from_embedded_property()
        {
            var resource = _client.From("/api/cardholders")
                .Follow("cardholder", new { id = 112 })
                .GetAsync()
                .Result
                .Embedded["card"].ToObject<CardEmbedded>();

            Assert.AreEqual("101", resource.Number);
            Assert.AreEqual("mastercard", resource.Type);
        }

        [Test]
        public void should_get_links_from_links_property()
        {
            var resource = _client.From("/api/cardholders")
                .Follow("cardholder", new {id = 112})
                .GetAsync()
                .Result
                .Links;

            var selfLink = resource["self"].ToObject<Link>();

            Assert.AreEqual("/api/cardholders/112", selfLink.Href);
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
            Assert.IsNotNull(_client);
        }

        [Test]
        public void can_follow_templated_link_with_complex_object()
        {
            var query = new SearchQuery() {Name = "Johny", Id = 101, CardNumber = "12345578778"};

            var resource = _client.From("/api/cardholders")
                .Follow("search", query, true)
                .GetAsync<SearchQuery>()
                .Result;

            Assert.AreEqual("Here's Johny!", resource.Name);
            Assert.AreEqual(101, resource.Id);
            Assert.AreEqual("12345578778", resource.CardNumber);
        }

        [Test]
        public void should_follow_from_resource()
        {
            var resource =
                _client.From("api/cardholders")
                    .GetAsync().Result;
            var next = resource.Follow("register").GetAsync<RegistrationForm>().Result;

            Assert.AreEqual(-1, next.Id);
        }

        [Test]
        public void should_follow_with_segments_from_resource()
        {
            var resource =
                 _client.From("api/cardholders")
                     .GetAsync().Result;
            var next = resource.Follow("cardholder", new{id=123}).GetAsync<CardHolderDetails>().Result;

            Assert.AreEqual(123, next.Id);
            Assert.AreEqual("Customer Number123",next.Name);
            Assert.AreEqual("again", next.AnotherCard.IdAgain);
        }

        [Test]
        public async Task should_post_from_resource()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var form = await 
                _client.From("/api/cardholders")
                    .Follow("register").GetAsync();
            //use form data, etc.
            var resource = await form.PostAsync(newData);

            Assert.AreEqual(HttpStatusCode.Created, resource.Response.StatusCode);

            var location = resource.Response.Headers.GetValues("Location").First();
            Assert.AreEqual("/api/cardholders/55", location);
        }

        [Test]
        public async Task should_put_to_server_from_resource()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var form =
                await _client.From("/api/cardholders")
                    .Follow("register").GetAsync();
            var resource = await form.PutAsync(newData);

            Assert.AreEqual(HttpStatusCode.Created, resource.Response.StatusCode);

            var location = resource.Response.Headers.GetValues("Location").First();
            Assert.AreEqual("/api/cardholders/55", location);
        }

        [Test]
        public async Task should_delete_to_server_from_resource()
        {
            var newData = new RegistrationForm { Id = 55, Name = "Johny" };
            var form =
                await _client.From("/api/cardholders")
                    .Follow("register").GetAsync();
            var resource = await form.DeleteAsync(newData);

            Assert.AreEqual(HttpStatusCode.OK, resource.Response.StatusCode);

            var location = resource.Response.Headers.GetValues("Location").First();
            Assert.AreEqual("/api/cardholders", location);
        }
    }
}