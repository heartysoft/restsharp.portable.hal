using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Hal.Models;
using WebApi.Hal;

namespace Hal.Controllers
{
    public class SearchController : ApiController
    {
        public SearchResults Get([FromUri]SearchQuery query)
        {
            return new SearchResults(query.Name, query.Id, query.CardNumber);
        }
    }

    public class SearchQuery
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string CardNumber { get; set; }
    }

    public class SearchResults : Representation
    {
        public SearchResults(string name, int id, string cardNumber)
        {
            Name = "Here's " + name + "!";
            Id = id;
            CardNumber = cardNumber;
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public string CardNumber { get; set; }

        public override string Rel { get { return LinkTemplates.SearchLinks.Search.Rel; } }
        public override string Href { get { return LinkTemplates.SearchLinks.Search.CreateLink().Href; } }
    }
}
