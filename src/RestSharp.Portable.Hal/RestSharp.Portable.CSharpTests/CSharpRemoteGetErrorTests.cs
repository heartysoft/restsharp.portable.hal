using System;
using System.Threading.Tasks;
using Hal;
using Microsoft.Owin.Testing;
using NUnit.Framework;
using RestSharp.Portable.Hal.CSharp;

namespace RestSharp.Portable.CSharpTests
{

	[TestFixture]
	public class CSharpRemoteGetErrorTests
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
        public async Task should_process_validation_errors()
        {
            try
            {
                var resource =
                    await _client.From("api/witherror")
                        .Follow("error-details", new { id = "2" })
                        .GetAsync();
                Assert.Fail("RemoteValidationException not raised when it was expected.");
            }
            catch (RemoteValidationException e)
            {
                Assert.AreEqual("Overall message", e.Message);
                Assert.AreEqual("Your name is a bit weird. Are you sure it's Yoda?", e.Errors["name"][0]);
                Assert.AreEqual("Yeah, right. You ain't 350 and I know it.", e.Errors["age"][0]);
                Assert.AreEqual(2, e.Errors.Count);
                Assert.AreEqual(3, e.TotalErrors());
            }
        }

        [Test]
        public async Task should_process_unexpected_response()
        {
            try
            {
                var resource =
                    await _client.From("api/witherror")
                        .Follow("error-details", new { id = "1" })
                        .GetAsync();
                Assert.Fail("Unexpected response exception not raised when it was expected.");
            }
            catch (UnexpectedResponseException e)
            {
                Assert.Pass("Unexpected response exception raised correctly.");
            }
        }
    }
}