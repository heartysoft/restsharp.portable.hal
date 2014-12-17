using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp.Portable.Hal.CSharp;

namespace RestSharp.Portable.CSharpTests
{
    [TestFixture]
    public class HalClientTests
    {
        [Test]
        public void should_get_resource()
        {
            var clientFactory = new HalClientFactory()
                .Accept("application/hal+json");
            var client = clientFactory.CreateHalClient("http://localhost:62582/");

            var resource = client.From("api/cardholders").GetAsync().Result;
            Assert.IsInstanceOf<Resource>(resource);
        }
    }
}
