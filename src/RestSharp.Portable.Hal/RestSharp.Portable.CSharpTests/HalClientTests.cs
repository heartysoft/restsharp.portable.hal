﻿using System.Linq;
using System.Net;
using NUnit.Framework;
using RestSharp.Portable.Hal.CSharp;

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
            _client = clientFactory.CreateHalClient("http://localhost:62582/");
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
    }
}